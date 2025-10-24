using Ayjet.Evaluation.Center.Application.Features.Accounts.DTOs;
using Ayjet.Evaluation.Center.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Ayjet.Evaluation.Center.Application.Features.Accounts.Queries.GetList;

public class GetUserListQueryHandler : IRequestHandler<GetUserListQuery, List<UserProfileDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    public GetUserListQueryHandler(UserManager<ApplicationUser> userManager) => _userManager = userManager;

    public async Task<List<UserProfileDto>> Handle(GetUserListQuery request, CancellationToken cancellationToken)
    {
        var users = await _userManager.Users.ToListAsync(cancellationToken);
        var userDtos = new List<UserProfileDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userDtos.Add(new UserProfileDto(user.Id, user.FirstName, user.LastName, user.FullName, user.Email!, user.ProfilePictureUrl, roles));
        }
        return userDtos;
    }
}