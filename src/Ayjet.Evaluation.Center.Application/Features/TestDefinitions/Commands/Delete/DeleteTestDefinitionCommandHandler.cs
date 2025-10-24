using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.TestDefinitions.Commands.Delete;

public class DeleteTestDefinitionCommandHandler : IRequestHandler<DeleteTestDefinitionCommand>
{
    private readonly ITestDefinitionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteTestDefinitionCommandHandler(ITestDefinitionRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteTestDefinitionCommand request, CancellationToken cancellationToken)
    {
        var testToDelete = await _repository.GetByIdAsync(request.Id, cancellationToken)
                           ?? throw new Exception("Silinecek test tanımı bulunamadı.");

        _repository.Delete(testToDelete);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}