using System.Text.Json;
using AAyjet.Evaluation.Center.Persistence.Context;
using Ayjet.Evaluation.Center.Domain.Entities;
using Ayjet.Evaluation.Center.Domain.Enums;
using Microsoft.EntityFrameworkCore;

// JsonNode için

namespace AAyjet.Evaluation.Center.Persistence.DataSeeders;

 public static class TestContentSeeder
{
    public static async Task SeedMmpiTestAsync(ApplicationDbContext context)
    {
        // Seeder'ın tekrar tekrar çalışmasını engelleyen ana kontrol.
        if (await context.TestDefinitions.AnyAsync(td => td.Title == "MMPI")) return;

        // 1. Ölçekleri oluştur ve veritabanına kaydet ki ID'leri oluşsun.
        var scales = new Dictionary<string, PsychometricScale>();
        if (!await context.PsychometricScales.AnyAsync())
        {
            var scaleNames = new[] { "L", "F", "K", "Hs", "D", "Hy", "Pd", "Mf", "Pa", "Pt", "Sc", "Ma", "Si" };
            foreach (var name in scaleNames)
            {
                scales.Add(name, new PsychometricScale { Name = name, CreatedAt = DateTime.UtcNow });
            }
            await context.PsychometricScales.AddRangeAsync(scales.Values);
            await context.SaveChangesAsync(); // Ölçekleri kaydet
        }
        // Mevcut ölçekleri ID'leriyle birlikte hafızaya al
        var scalesFromDb = await context.PsychometricScales.ToDictionaryAsync(s => s.Name, s => s);

        // 2. MMPI Test tanımını oluştur
        var mmpiTest = new TestDefinition 
        { 
            Title = "MMPI", 
            Description = "Minnesota Çok Yönlü Kişilik Envanteri",
            Type = TestType.Psychometric, 
            IsActive = true,
            DefaultQuestionCount = 567,
            CreatedAt = DateTime.UtcNow,
            Language = "tr-TR" // Varsayılan dil
        };
        
        // 3. JSON dosyasını oku ve yeni DTO yapısına göre deserialize et.
        var jsonPath = Path.Combine(AppContext.BaseDirectory, "DataSeeders/Raw/questions.json");
        var jsonText = await File.ReadAllTextAsync(jsonPath);
        var questionsData = JsonSerializer.Deserialize<List<JsonQuestionData>>(jsonText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (questionsData == null) return;

        // 4. Her bir JSON kaydı için entity'leri oluştur.
        foreach (var qData in questionsData)
        {
            var question = new PsychometricQuestion
            {
                DisplayOrder = qData.QuestionId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            
            // Çevirileri ekle
            foreach(var trans in qData.Translations)
            {
                question.Translations.Add(new PsychometricQuestionTranslation { Language = trans.Language, Text = trans.Text });
            }

            // Puanlama kurallarını (mapping) ekle
            foreach (var scoringInfo in qData.ScoringInfo)
            {
                if(scalesFromDb.TryGetValue(scoringInfo.Key, out var scale))
                {
                    var direction = scoringInfo.KeyedResponse.Equals("True", StringComparison.OrdinalIgnoreCase) ? ScoringDirection.True : ScoringDirection.False;
                    
                    Gender? requiredGender = null;
                    if (!string.IsNullOrEmpty(scoringInfo.Gender))
                    {
                        requiredGender = Enum.Parse<Gender>(scoringInfo.Gender, true);
                    }

                    question.ScaleMappings.Add(new QuestionScaleMapping 
                    { 
                        PsychometricScaleId = scale.Id, // Doğrudan ID ile ilişki kur
                        ScoringDirection = direction,
                        RequiredGender = requiredGender
                    });
                }
            }
            mmpiTest.PsychometricQuestions.Add(question);
        }

        // 5. Test tanımını (ve ona bağlı tüm soruları, çevirileri, mapping'leri) veritabanına ekle
        await context.TestDefinitions.AddAsync(mmpiTest);
        
        // 6. Tek bir komutla tüm değişiklikleri kaydet.
        await context.SaveChangesAsync();
    }
    
    // --- JSON'u deserialize etmek için yeni, doğru yardımcı sınıflar ---
    private class JsonQuestionData
    {
        public int QuestionId { get; set; }
        public List<JsonTranslation> Translations { get; set; } = new();
        public List<JsonScoringInfo> ScoringInfo { get; set; } = new();
    }

    private class JsonTranslation
    {
        public string Language { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }

    private class JsonScoringInfo
    {
        public string Key { get; set; } = string.Empty;
        public string KeyedResponse { get; set; } = string.Empty;
        public string? Gender { get; set; }
    }
}