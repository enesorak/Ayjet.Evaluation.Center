using Ayjet.Evaluation.Center.Application.Common.Exceptions;
using Ayjet.Evaluation.Center.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Ayjet.Evaluation.Center.Application.Features.Accounts.Commands.UpdateRoles;

public class UpdateUserRolesCommandHandler : IRequestHandler<UpdateUserRolesCommand>
{
    private readonly UserManager<ApplicationUser> _userManager;
    public UpdateUserRolesCommandHandler(UserManager<ApplicationUser> userManager) => _userManager = userManager;

    public async Task Handle(UpdateUserRolesCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId) 
                   ?? throw new NotFoundException("Kullanıcı bulunamadı.");

        var currentRoles = await _userManager.GetRolesAsync(user);

        // Kullanıcıyı mevcut rollerinden çıkar, yeni rollerine ekle
        var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
        if (!removeResult.Succeeded) throw new Exception("Kullanıcı rollerini güncellerken hata oluştu.");

        var addResult = await _userManager.AddToRolesAsync(user, request.Roles);
        if (!addResult.Succeeded) throw new Exception("Kullanıcıya yeni roller atanırken hata oluştu.");
    }
}