using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Ayjet.Evaluation.Center.Infrastructure.Hubs;

[Authorize] // Sadece giriş yapmış kullanıcılar bağlanabilir
public class NotificationHub : Hub
{
    // Bir kullanıcı bağlandığında çalışır
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRoles = Context.User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        if (!string.IsNullOrEmpty(userId))
        {
            // Kullanıcıyı kendi özel grubuna ekle (örn: 'user-12345')
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
        }

        // Kullanıcıyı rollerine göre gruplara ekle (örn: 'group-Admin')
        if(userRoles != null)
        {
            foreach (var role in userRoles)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"group-{role}");
            }
        }

        await base.OnConnectedAsync();
    }
}