using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Domain.Entities;
using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.TestDefinitions.Commands.Create;

// Bu sınıf, IRequestHandler<GelenKomut, DönenSonuç> arayüzünü uygular.
public class CreateTestDefinitionCommandHandler : IRequestHandler<CreateTestDefinitionCommand, string>
{
    private readonly ITestDefinitionRepository _repository;
    private readonly IUnitOfWork _unitOfWork; // <-- Unit of Work'ü ekle

    public CreateTestDefinitionCommandHandler(ITestDefinitionRepository repository, IUnitOfWork unitOfWork) // <-- Constructor'ı güncelle
    {
        _repository = repository;
        _unitOfWork = unitOfWork; // <-- Ata
    }

    public async Task<string> Handle(CreateTestDefinitionCommand request, CancellationToken cancellationToken)
    {
        // YENİ NESNEYİ OLUŞTURAN VE VERİLERİ ATAYAN KISIM
        // Lütfen bu bölümün sizdeki kodla aynı olduğundan emin olun.
        var newTestDefinition = new TestDefinition
        {
            Title = request.Title,
            Description = request.Description,
            Type = request.Type,
            IsActive = true,
            
            // === DÜZELTİLMİŞ ATAMALAR ===
            DefaultTimeLimitInMinutes = request.DefaultTimeLimitInMinutes,
            DefaultQuestionCount = request.DefaultQuestionCount,
            // ============================
            PassingScore = request.PassingScore
                ,
            CreatedAt = DateTime.UtcNow
        };
        
        await _repository.AddAsync(newTestDefinition, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return newTestDefinition.Id;
    }
}