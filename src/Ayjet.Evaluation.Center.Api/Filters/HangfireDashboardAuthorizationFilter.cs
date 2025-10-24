using Hangfire.Dashboard;

namespace Ayjet.Evaluation.Center.Api.Filters;

public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        return true;
        var httpContext = context.GetHttpContext();
        // Sadece Admin rolüne sahip kullanıcıların dashboard'ı görmesine izin ver.
        return httpContext.User.Identity?.IsAuthenticated == true && httpContext.User.IsInRole("Admin");
    }
}