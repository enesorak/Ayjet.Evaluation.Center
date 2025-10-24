using Ayjet.Evaluation.Center.Application.Common.Interfaces;
using Ayjet.Evaluation.Center.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Ayjet.Evaluation.Center.Infrastructure.Notifications;

public class SignalRNotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public SignalRNotificationService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendScoreUpdateNotificationAsync(string message, string assignmentId)
    {
        // Bu servis, "Admin" grubundaki herkese haber verir.
        await _hubContext.Clients
            .Group("group-Admin")
            .SendAsync("ReceiveScoreUpdate", message, assignmentId);
    }
}

 