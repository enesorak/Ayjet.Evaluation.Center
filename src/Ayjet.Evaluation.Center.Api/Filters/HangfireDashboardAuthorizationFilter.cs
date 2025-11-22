using Hangfire.Dashboard;

namespace Ayjet.Evaluation.Center.Api.Filters;

public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
     /*   var httpContext = context.GetHttpContext();
        
        // Kullanıcı login olmuş VE Admin rolüne sahip mi?
        return httpContext.User.Identity?.IsAuthenticated == true 
               && httpContext.User.IsInRole("Admin");*/
     
     return true;
    }
}