using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.Accounts.Commands.UpdateRoles;

public record UpdateUserRolesCommand(string UserId, List<string> Roles) : IRequest;