using MediatR;
using Microsoft.AspNetCore.Http;

namespace Ayjet.Evaluation.Center.Application.Features.Candidates.Commands.UpdateProfilePicture;

public record UpdateCandidateProfilePictureCommand(
    string CandidateId, 
    IFormFile ProfilePicture) : IRequest<string>; // Resmin URL'ini dönecek