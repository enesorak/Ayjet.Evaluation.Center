// SeedCandidatesCommandHandler.cs
using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Domain.Entities;
using Ayjet.Evaluation.Center.Domain.Enums;
using Bogus; // <-- Bogus kütüphanesini ekliyoruz
using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.DeveloperTools.SeedCandidates;

public class SeedCandidatesCommandHandler : IRequestHandler<SeedCandidatesCommand, int>
{
    private readonly ICandidateRepository _candidateRepo;
    private readonly IUnitOfWork _unitOfWork;

    public SeedCandidatesCommandHandler(ICandidateRepository candidateRepo, IUnitOfWork unitOfWork)
    {
        _candidateRepo = candidateRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(SeedCandidatesCommand request, CancellationToken cancellationToken)
    {
        // Sahte veri üreticimizi "tr" (Türkçe) yerel ayarlarıyla yapılandırıyoruz.
        var faker = new Faker<Candidate>("tr")
            .RuleFor(c => c.FirstName, f => f.Name.FirstName())
            .RuleFor(c => c.LastName, f => f.Name.LastName())
            .RuleFor(c => c.Email, (f, c) => f.Internet.Email(c.FirstName, c.LastName))
            .RuleFor(c => c.Department, f => f.PickRandom<Department>())
            .RuleFor(c => c.CandidateType, f => f.PickRandom<CandidateType>())
            .RuleFor(c => c.Gender, f => f.PickRandom<Gender>())
            .RuleFor(c => c.BirthDate, f => DateOnly.FromDateTime(f.Person.DateOfBirth))
            .RuleFor(c => c.Profession, f => f.Name.JobTitle())
            .RuleFor(c => c.MaritalStatus, f => f.PickRandom(new[] { "Bekar", "Evli", "Boşanmış" }))
            .RuleFor(c => c.EducationLevel, f => f.PickRandom(new[] { "Lise", "Ön Lisans", "Lisans", "Yüksek Lisans" }))
            .RuleFor(c => c.CreatedAt, DateTime.UtcNow);

        var candidates = faker.Generate(request.Count);

        await _candidateRepo.AddRangeAsync(candidates, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return candidates.Count;
    }
}