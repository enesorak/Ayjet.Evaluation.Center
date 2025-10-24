using Ayjet.Evaluation.Center.Application.Common.Exceptions;
using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Domain.Entities;
using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.MultipleChoiceQuestions.Commands.Delete;

public class DeleteQuestionCommandHandler : IRequestHandler<DeleteQuestionCommand>
{
    private readonly IRepository<MultipleChoiceQuestion> _questionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteQuestionCommandHandler(IRepository<MultipleChoiceQuestion> questionRepository, IUnitOfWork unitOfWork)
    {
        _questionRepository = questionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteQuestionCommand request, CancellationToken cancellationToken)
    {
        var questionToDelete = await _questionRepository.GetByIdAsync(request.Id, cancellationToken)
                               ?? throw new NotFoundException(nameof(MultipleChoiceQuestion), request.Id);

        questionToDelete.IsActive = false; // Silmek yerine deaktif et
        // _questionRepo.Delete(questionToDelete); // Bu satırı siliyoruz
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}