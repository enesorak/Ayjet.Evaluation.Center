using System.ComponentModel.DataAnnotations;
using Ayjet.Evaluation.Center.Application.Common.Exceptions;
using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Domain.Entities;
using Ayjet.Evaluation.Center.Domain.Enums;
using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.Candidates.Commands.Create;

public class CreateCandidateCommandHandler : IRequestHandler<CreateCandidateCommand, string>
{
    private readonly ICandidateRepository _candidateRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCandidateCommandHandler(ICandidateRepository candidateRepository, IUnitOfWork unitOfWork)
    {
        _candidateRepository = candidateRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<string> Handle(CreateCandidateCommand request, CancellationToken cancellationToken)
    {
        var existingCandidate = await _candidateRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingCandidate != null)
        {
            throw new ConflictException("Bu e-posta adresi ile zaten bir aday kayıtlı.");

        }
        if (!string.IsNullOrWhiteSpace(request.InitialCode))
        {
            var existingWithSameCode = await _candidateRepository.FirstOrDefaultAsync(
                c => c.InitialCode != null && c.InitialCode.ToLower() == request.InitialCode.ToLower(), 
                cancellationToken);
        
            if (existingWithSameCode != null)
            {
                throw new ValidationException("Bu Initial Code daha önce başka bir aday için kullanılmış.");
            }
        }
        var candidate = new Candidate
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Department = (Department)request.Department,
            CandidateType = (CandidateType)request.CandidateType,
            Gender = (Gender?)request.Gender,
            InitialCode = string.IsNullOrWhiteSpace(request.InitialCode) ? null : request.InitialCode,
            FleetCode = string.IsNullOrWhiteSpace(request.FleetCode) ? null : request.FleetCode,

            BirthDate = request.BirthDate, // <-- Eklendi
            MaritalStatus = request.MaritalStatus,
            Profession = request.Profession,
            EducationLevel = request.EducationLevel,
            CreatedAt = DateTime.UtcNow
        };

        await _candidateRepository.AddAsync(candidate, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return candidate.Id;

    }
}