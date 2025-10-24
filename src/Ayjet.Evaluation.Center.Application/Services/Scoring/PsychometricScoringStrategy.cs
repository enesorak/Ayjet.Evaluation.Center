using System.Text.Json;
using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Domain.Entities;
using Ayjet.Evaluation.Center.Domain.Enums;

namespace Ayjet.Evaluation.Center.Application.Services.Scoring;

public class PsychometricScoringStrategy : IScoringStrategy
{
    private readonly IPsychometricQuestionRepository _questionRepo;
    public TestType Handles => TestType.Psychometric;

    public PsychometricScoringStrategy(IPsychometricQuestionRepository questionRepo)
    {
        _questionRepo = questionRepo;
    }

    public async Task<TestResult> ScoreAsync(TestAssignment assignment)
    {
        var questionIds = assignment.AssignedQuestions
            .Where(aq => aq.PsychometricQuestionId.HasValue)
            .Select(aq => aq.PsychometricQuestionId!.Value)
            .ToList();
       
        var mappingsLookup = await _questionRepo.GetMappingsForQuestionsAsync(questionIds);
        
        var rawScores = CalculateRawScores(assignment, mappingsLookup);
        var correctedScores = ApplyKCorrection(rawScores);
        var tScores = ConvertToTScores(correctedScores, assignment.Candidate.Gender ?? Gender.Male); // Varsayılan Erkek

        var finalResultPayload = new MMPIScoringResult
        {
            RawScores = rawScores,
            CorrectedScores = correctedScores,
            TScores = tScores
        };

        // 3. Bu zengin nesneyi JSON olarak kaydet
        var result = new TestResult
        {
            TestAssignmentId = assignment.Id,
            Score = tScores.TryGetValue("F", out var fScore) ? fScore : 0,
            ResultsPayloadJson = JsonSerializer.Serialize(finalResultPayload),
            CreatedAt = DateTime.UtcNow
        };
        return result;
    }

    private Dictionary<string, int> CalculateRawScores(TestAssignment assignment, ILookup<int, QuestionScaleMapping> mappingsLookup)
    {
        var scaleNames = MMPI1ScoringDataProvider.TScoreNorms[Gender.Male].Keys.Select(k => k.Split('+')[0]).Union(new[] { "L", "F", "K", "?" }).Distinct();
        var rawScores = scaleNames.ToDictionary(k => k, v => 0);
        rawScores["?"] = assignment.CandidateAnswers.Count(a => a.PsychometricResponse == 0);

        foreach (var answer in assignment.CandidateAnswers.Where(a => a.PsychometricResponse > 0 && a.PsychometricQuestionId.HasValue))
        {
            var mappings = mappingsLookup[answer.PsychometricQuestionId!.Value];
            foreach (var mapping in mappings)
            {
                var responseDirection = answer.PsychometricResponse == 1 ? ScoringDirection.True : ScoringDirection.False;
                if (mapping.ScoringDirection == responseDirection && (!mapping.RequiredGender.HasValue || mapping.RequiredGender == assignment.Candidate.Gender))
                {
                    rawScores[mapping.PsychometricScale.Name]++;
                }
            }
        }
        return rawScores;
    }

    private Dictionary<string, double> ApplyKCorrection(Dictionary<string, int> rawScores)
    {
        var correctedScores = rawScores.ToDictionary(kvp => kvp.Key, kvp => (double)kvp.Value);
        if (!rawScores.TryGetValue("K", out var kValue)) return correctedScores;
        if (!MMPI1ScoringDataProvider.KNormTable.TryGetValue(kValue, out var kNorm)) return correctedScores;

        foreach (var rule in MMPI1ScoringDataProvider.KCorrectionRules)
        {
            if (correctedScores.TryGetValue(rule.Key, out var currentScore))
            {
                double correction = rule.Value switch
                {
                    1.0 => kValue, 0.5 => kNorm.For05, 0.4 => kNorm.For04, 0.2 => kNorm.For02, _ => 0
                };
                correctedScores[rule.Key] = currentScore + correction;
            }
        }
        return correctedScores;
    }

    private Dictionary<string, int> ConvertToTScores(Dictionary<string, double> correctedScores, Gender gender)
    {
        var tScores = new Dictionary<string, int>();
        var norms = (gender == Gender.Female) ? MMPI1ScoringDataProvider.TScoreNorms[Gender.Female] : MMPI1ScoringDataProvider.TScoreNorms[Gender.Male];

        // "?" Skalası için özel hesaplama
        if (correctedScores.TryGetValue("?", out var rawQuestionMarkScoreInt))
        {
            var rawScore = (int)rawQuestionMarkScoreInt;
            switch (rawScore)
            {
                case >= 131 and <= 565:
                    tScores["?"] = 80;
                    break;
                case 566:
                    tScores["?"] = 0;
                    break;
                default:
                {
                    var lookup = MMPI1ScoringDataProvider.SpecialTScoreLookupForQuestionMark.FirstOrDefault(t => t.Item1 == rawScore);
                    tScores["?"] = lookup?.Item2 ?? 30; // Bulamazsa varsayılan
                    break;
                }
            }
        }

        // Diğer skalalar için formül
        foreach (var scorePair in correctedScores)
        {
            if (scorePair.Key == "?") continue;

            var normKey = norms.Keys.FirstOrDefault(k => k.StartsWith(scorePair.Key)) ?? scorePair.Key;
            if (norms.TryGetValue(normKey, out var norm) && norm.StandardDeviation != 0)
            {
                var tScore = 50 + 10 * ((scorePair.Value - norm.Mean) / norm.StandardDeviation);
                tScores[scorePair.Key] = (int)Math.Round(tScore);
            }
        }
        return tScores;
    }
    
    public class MMPIScoringResult
    {
        public Dictionary<string, int> RawScores { get; set; } = new();
        public Dictionary<string, double> CorrectedScores { get; set; } = new();
        public Dictionary<string, int> TScores { get; set; } = new();
    }
}