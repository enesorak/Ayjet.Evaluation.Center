using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.TestAssignments.Commands.ResendInvitation;

public record ResendInvitationCommand(
    string AssignmentId, 
    int? NewExpirationInDays // Opsiyonel: Yeni süre (gün olarak)
) : IRequest;