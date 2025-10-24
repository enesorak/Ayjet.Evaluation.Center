using Ayjet.Evaluation.Center.Application.Common.Exceptions;
using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Domain.Entities;
using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.Candidates.Commands.UpdateProfilePicture;

public class UpdateCandidateProfilePictureCommandHandler : IRequestHandler<UpdateCandidateProfilePictureCommand, string>
{
    private readonly ICandidateRepository _candidateRepo;
    private readonly IFileStorageService _fileStorage; // Daha önce oluşturmuştuk
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCandidateProfilePictureCommandHandler(ICandidateRepository repo, IFileStorageService fileStorage, IUnitOfWork uow)
    {
        _candidateRepo = repo; _fileStorage = fileStorage; _unitOfWork = uow;
    }

    public async Task<string> Handle(UpdateCandidateProfilePictureCommand request, CancellationToken cancellationToken)
    {
        var candidate = await _candidateRepo.GetByIdAsync(request.CandidateId, cancellationToken)
                        ?? throw new NotFoundException(nameof(Candidate), request.CandidateId);

        // Dosyayı kaydet ve public URL'ini al
        var fileUrl = await _fileStorage.SaveFileAsync(
            request.ProfilePicture.OpenReadStream(), 
            request.ProfilePicture.FileName);

        candidate.ProfilePictureUrl = fileUrl;
        _candidateRepo.Update(candidate);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return fileUrl;
    }
}