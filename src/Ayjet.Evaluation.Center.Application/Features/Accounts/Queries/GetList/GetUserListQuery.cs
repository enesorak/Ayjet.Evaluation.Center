using Ayjet.Evaluation.Center.Application.Features.Accounts.DTOs;
using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.Accounts.Queries.GetList;

public record GetUserListQuery() : IRequest<List<UserProfileDto>>;