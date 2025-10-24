using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.TestDefinitions.Commands.Update;

public class UpdateTestDefinitionCommandHandler : IRequestHandler<UpdateTestDefinitionCommand>
{
    private readonly ITestDefinitionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTestDefinitionCommandHandler(ITestDefinitionRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateTestDefinitionCommand request, CancellationToken cancellationToken)
    {
        // 1. Güncellenecek varlığı ID ile bul.
        var testToUpdate = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (testToUpdate is null)
        {
            // Varlık bulunamazsa hata fırlat (bunu daha sonra özel exception'larla yöneteceğiz)
            throw new Exception("Güncellenecek test tanımı bulunamadı.");
        }

        // 2. Varlığın özelliklerini gelen yeni verilerle güncelle.
        testToUpdate.Title = request.Title;
        testToUpdate.Description = request.Description;
        testToUpdate.DefaultTimeLimitInMinutes = request.TimeLimitInMinutes;
        testToUpdate.UpdatedAt = DateTime.UtcNow;
        testToUpdate.PassingScore = request.PassingScore;
        // 3. Değişiklikleri kaydet.
        // Not: EF Core, varlığı takip ettiği için Update gibi özel bir komuta gerek yoktur.
        // Sadece SaveChanges demek yeterlidir.
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}