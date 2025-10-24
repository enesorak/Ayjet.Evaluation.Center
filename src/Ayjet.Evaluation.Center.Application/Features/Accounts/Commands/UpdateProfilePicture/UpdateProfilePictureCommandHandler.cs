using Ayjet.Evaluation.Center.Application.Common.Exceptions;
using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Ayjet.Evaluation.Center.Application.Features.Accounts.Commands.UpdateProfilePicture;

public class UpdateProfilePictureCommandHandler : IRequestHandler<UpdateProfilePictureCommand, string>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IFileStorageService _fileStorageService;

    public UpdateProfilePictureCommandHandler(UserManager<ApplicationUser> userManager, IFileStorageService fileStorageService)
    {
        _userManager = userManager;
        _fileStorageService = fileStorageService;
    }

    public async Task<string> Handle(UpdateProfilePictureCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId)
                   ?? throw new NotFoundException(nameof(ApplicationUser), request.UserId);

        var fileUrl = await _fileStorageService.SaveFileAsync(request.FileStream, request.FileName);

        user.ProfilePictureUrl = fileUrl;
        await _userManager.UpdateAsync(user);

        return fileUrl;
    }
}