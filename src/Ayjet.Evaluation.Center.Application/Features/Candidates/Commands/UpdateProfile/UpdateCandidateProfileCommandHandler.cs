using System.ComponentModel.DataAnnotations;
using Ayjet.Evaluation.Center.Application.Common.Exceptions;
using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Domain.Entities;
using Ayjet.Evaluation.Center.Domain.Enums;
using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.Candidates.Commands.UpdateProfile;

public class UpdateCandidateProfileCommandHandler : IRequestHandler<UpdateCandidateProfileCommand>
{
    private readonly ICandidateRepository _candidateRepo;
    private readonly IUnitOfWork _unitOfWork;
    public UpdateCandidateProfileCommandHandler(ICandidateRepository repo, IUnitOfWork uow) 
        => (_candidateRepo, _unitOfWork) = (repo, uow);

    public async Task Handle(UpdateCandidateProfileCommand request, CancellationToken cancellationToken)
    {
        var candidate = await _candidateRepo.GetByIdAsync(request.Id, cancellationToken)
                        ?? throw new NotFoundException(nameof(Candidate), request.Id);

        if (!string.IsNullOrWhiteSpace(request.InitialCode))
        {
            // Kendisi hariç, bu koda sahip başka bir aday var mı diye kontrol et
            var existingWithSameCode = await _candidateRepo.FirstOrDefaultAsync(
                c => c.Id != request.Id && c.InitialCode != null && c.InitialCode.ToLower() == request.InitialCode.ToLower(), 
                cancellationToken);
        
            if (existingWithSameCode != null)
            {
                throw new ValidationException("This Initial Code has already been used by another candidate.");
            }
        }
        
        // Tüm alanları güncelle
        candidate.FirstName = request.FirstName;
        candidate.LastName = request.LastName;
        candidate.Email = request.Email;
        //candidate.Department = (Department)request.Department;
        //candidate.CandidateType = (CandidateType)request.CandidateType;
        candidate.Gender = (Gender?)request.Gender;
        candidate.InitialCode = string.IsNullOrWhiteSpace(request.InitialCode) ? null : request.InitialCode;
        candidate.FleetCode = string.IsNullOrWhiteSpace(request.FleetCode) ? null : request.FleetCode;

        candidate.BirthDate = request.BirthDate;
        candidate.MaritalStatus = request.MaritalStatus;
        candidate.Profession = request.Profession;
        candidate.EducationLevel = request.EducationLevel;
        candidate.UpdatedAt = DateTime.UtcNow;

        _candidateRepo.Update(candidate);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}