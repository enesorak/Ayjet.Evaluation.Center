using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.Accounts.Commands.UpdateProfilePicture;

public record UpdateProfilePictureCommand(
    string UserId,
    Stream FileStream,
    string FileName) : IRequest<string>; // Kaydedilen dosyanın URL'ini dönecek