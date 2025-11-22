using Ayjet.Evaluation.Center.Application.Common.Exceptions;
using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Application.Features.CandidateAnswers.Commands.Submit;
using Ayjet.Evaluation.Center.Application.Features.TestAssignments.Commands.Create;
using Ayjet.Evaluation.Center.Application.Features.TestAssignments.Commands.Finish;
using Ayjet.Evaluation.Center.Application.Features.TestAssignments.Commands.Start;
using Ayjet.Evaluation.Center.Domain.Entities;
using Ayjet.Evaluation.Center.Domain.Enums;
using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.DeveloperTools;

public class RunMMPISimulationCommandHandler : IRequestHandler<RunMMPISimulationCommand, Dictionary<int, int>>
{
    private readonly ISender _mediator;
    private readonly ICandidateRepository _candidateRepo;
    private readonly ITestDefinitionRepository _testDefinitionRepo;
    private readonly IPsychometricQuestionRepository _psychometricQuestionRepo; // <-- Yeni Bağımlılık
    private readonly ITestAssignmentRepository _assignmentRepo;
    private readonly IUnitOfWork _unitOfWork;

    public RunMMPISimulationCommandHandler(
        ISender mediator, ICandidateRepository candidateRepo, ITestDefinitionRepository testDefinitionRepo,
        ITestAssignmentRepository assignmentRepo, IUnitOfWork unitOfWork,
        IPsychometricQuestionRepository psychometricQuestionRepo) // <-- Constructor Güncellendi
    {
        _mediator = mediator;
        _candidateRepo = candidateRepo;
        _testDefinitionRepo = testDefinitionRepo;
        _assignmentRepo = assignmentRepo;
        _unitOfWork = unitOfWork;
        _psychometricQuestionRepo = psychometricQuestionRepo; // <-- Yeni
    }

    public async Task<Dictionary<int, int>> Handle(RunMMPISimulationCommand request,
        CancellationToken cancellationToken)
    {
        var mmpiTest =
            (await _testDefinitionRepo.ListAsync(td => td.Title == "MMPI", cancellationToken)).FirstOrDefault()
            ?? throw new NotFoundException("MMPI test tanımı bulunamadı.");

        var demoCandidate =
            await _candidateRepo.FirstOrDefaultAsync(c => c.Email == "demoaday@ayjet.com", cancellationToken);
        if (demoCandidate == null)
        {
            demoCandidate = new Candidate
            {
                FirstName = "Demo", LastName = "Aday", Email = "demoaday@ayjet.com", Gender = Gender.Male,
                Department = Department.HumanResources, CandidateType = CandidateType.Employee
            };
            await _candidateRepo.AddAsync(demoCandidate, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var assignmentCommand = new AssignTestCommand(
            mmpiTest.Id,
            [demoCandidate.Id],
            mmpiTest.DefaultTimeLimitInMinutes,
            mmpiTest.DefaultQuestionCount, "tr-TR", 1);
        var assignmentIds = await _mediator.Send(assignmentCommand, cancellationToken);
        var assignmentId = assignmentIds.First();

        await _mediator.Send(new StartTestCommand(assignmentId), cancellationToken);

        var assignment = await _assignmentRepo.GetByIdAsync(assignmentId, cancellationToken)
                         ?? throw new NotFoundException("Atama bulunamadı.");

        var allQuestions = await _psychometricQuestionRepo.GetAllByTestDefinitionIdAsync(mmpiTest.Id);

        var random = new Random();
        var generatedAnswers = new Dictionary<int, int>();

        // --- YENİ VE AKILLI CEVAP OLUŞTURMA MANTIĞI ---
        var answerPool = new List<int>();
        // 1. Havuza en fazla 15 tane '0' (Fikrim Yok) cevabı ekle
        int noIdeaCount = random.Next(5, 16); // 5 ile 15 arasında rastgele sayıda
        for (int i = 0; i < noIdeaCount; i++) answerPool.Add(0);
        // 2. Havuzun geri kalanını '1' ve '2' ile doldur
        while (answerPool.Count < allQuestions.Count)
        {
            answerPool.Add(random.Next(1, 3)); // 1 (Doğru) veya 2 (Yanlış)
        }

        // 3. Havuzu karıştır
        var shuffledAnswers = answerPool.OrderBy(a => random.Next()).ToList();
        // ------------------------------------------------

        for (int i = 0; i < allQuestions.Count; i++)
        {
            var question = allQuestions[i];
            var response = shuffledAnswers[i];

            generatedAnswers.Add(question.DisplayOrder, response);

            var answerCommand = new SubmitAnswerCommand(
                assignmentId, question.Id, null, response);

            await _mediator.Send(answerCommand, cancellationToken);
        }

       // await _mediator.Send(new FinishTestCommand(assignmentId), cancellationToken);

        return generatedAnswers;
    }
}