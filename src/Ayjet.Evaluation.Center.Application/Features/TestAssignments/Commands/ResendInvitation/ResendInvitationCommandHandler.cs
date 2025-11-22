using Ayjet.Evaluation.Center.Application.Common.Exceptions;
using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Domain.Entities;
using Ayjet.Evaluation.Center.Domain.Enums;
using Hangfire;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace Ayjet.Evaluation.Center.Application.Features.TestAssignments.Commands.ResendInvitation;

public class ResendInvitationCommandHandler : IRequestHandler<ResendInvitationCommand>
{
    private readonly ITestAssignmentRepository _assignmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IConfiguration _configuration;

    public ResendInvitationCommandHandler(
        ITestAssignmentRepository assignmentRepository, 
        IUnitOfWork unitOfWork, 
        IBackgroundJobClient backgroundJobClient, 
        IConfiguration configuration)
    {
        _assignmentRepository = assignmentRepository;
        _unitOfWork = unitOfWork;
        _backgroundJobClient = backgroundJobClient;
        _configuration = configuration;
    }

    public async Task Handle(ResendInvitationCommand request, CancellationToken cancellationToken)
    {
        var assignment = await _assignmentRepository.GetByIdWithDetailsAsync(request.AssignmentId, cancellationToken)
                         ?? throw new NotFoundException(nameof(TestAssignment), request.AssignmentId);

        if (assignment.Status == TestAssignmentStatus.Completed)
        {
            throw new Exception("Cannot resend invitation for a completed test.");
        }

        // Süre uzatma istendiyse, tarihi güncelle
        if (request.NewExpirationInDays.HasValue)
        {
            assignment.ExpiresAt = DateTime.UtcNow.AddDays(request.NewExpirationInDays.Value);
            _assignmentRepository.Update(assignment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        // E-posta gönderme işini tetikle
        var clientUrl = _configuration["ClientUrl"] ?? "http://evaluation.ayjet.aero";
        var testLink = $"{clientUrl}/take-test/{assignment.Id}";

        _backgroundJobClient.Enqueue<IEmailService>(service =>
            service.SendTestInvitationEmailAsync(
                assignment.Candidate.Email,
                assignment.Candidate.FullName,
                assignment.TestDefinition.Title,
                testLink,
                assignment.ExpiresAt
            )
        );
    }
}