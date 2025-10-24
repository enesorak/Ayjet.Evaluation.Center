using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Application.Features.Candidates.Commands.Create;
using Ayjet.Evaluation.Center.Domain.Entities;
using Ayjet.Evaluation.Center.Domain.Enums;
using Moq;
using FluentAssertions;
 

namespace Ayjet.Evaluation.Center.Application.Tests.Features.Candidates.Commands;

public class CreateCandidateCommandHandlerTests
{
    
    
    private readonly Mock<ICandidateRepository> _mockCandidateRepo;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly CreateCandidateCommandHandler _handler;

    public CreateCandidateCommandHandlerTests()
    {
        // Bağımlılıkların sahtelerini (mock) oluşturuyoruz
        _mockCandidateRepo = new Mock<ICandidateRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        // Test edeceğimiz asıl sınıfı (Handler), bu sahte bağımlılıklarla oluşturuyoruz
        _handler = new CreateCandidateCommandHandler(
            _mockCandidateRepo.Object, 
            _mockUnitOfWork.Object
        );
    }

    [Fact]
    public async Task Handle_Should_CreateCandidate_WhenEmailIsUnique()
    {
        // ARRANGE (Hazırlık): Test senaryomuzu hazırlıyoruz
        var command = new CreateCandidateCommand(
            "Test", "User", "test@example.com", (int)Department.FlightOps, 
            (int)CandidateType.Student, (int)Gender.Male, "T123", "F123", 
            new DateOnly(1990, 1, 1), "Single", "Pilot", "University"
        );

        // ICandidateRepository'nin GetByEmailAsync metodu çağrıldığında,
        // bu e-postanın unique olduğunu varsayarak 'null' dönmesini sağlıyoruz.
        _mockCandidateRepo.Setup(r => r.GetByEmailAsync(command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Candidate)null);

        // ACT (Eylem): Test edeceğimiz metodu çağırıyoruz
        var result = await _handler.Handle(command, CancellationToken.None);

        // ASSERT (Doğrulama): Sonuçları kontrol ediyoruz
        result.Should().NotBeNullOrEmpty(); // Sonuç olarak bir ID dönmeli.

        // AddAsync metodunun, içinde doğru bilgiler olan bir Candidate nesnesiyle
        // en az bir kez çağrıldığını doğruluyoruz.
        _mockCandidateRepo.Verify(r => r.AddAsync(It.Is<Candidate>(c => 
            c.FirstName == "Test" && 
            c.Email == "test@example.com"
        ), It.IsAny<CancellationToken>()), Times.Once);

        // Tüm işlemler bittikten sonra, SaveChangesAsync'in en az bir kez
        // çağrıldığını, yani verinin kaydedilmeye çalışıldığını doğruluyoruz.
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}