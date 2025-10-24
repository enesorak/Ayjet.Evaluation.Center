using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Hangfire;
using MediatR;

namespace Ayjet.Evaluation.Center.Application.Features.TestResults.Commands.ReScore;

public class ReScoreTestCommandHandler : IRequestHandler<ReScoreTestCommand>
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    public ReScoreTestCommandHandler(IBackgroundJobClient backgroundJobClient) 
        => _backgroundJobClient = backgroundJobClient;

    public Task Handle(ReScoreTestCommand request, CancellationToken cancellationToken)
    {
        // İşi doğrudan Hangfire'a gönderiyoruz.
        _backgroundJobClient.Enqueue<IScoringService>(service => 
            service.CalculateAndSaveScoreAsync(request.AssignmentId));
        return Task.CompletedTask;
    }
}