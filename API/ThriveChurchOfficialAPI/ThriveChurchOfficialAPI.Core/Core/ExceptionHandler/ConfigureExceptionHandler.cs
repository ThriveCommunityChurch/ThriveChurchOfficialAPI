using Microsoft.AspNetCore.Builder;

namespace ThriveChurchOfficialAPI.Core.Core.ExceptionHandler
{
    public static class ConfigureExceptionHandler
    {
        public static void ConfigureCustomExceptionMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<ExceptionHandler>();
        }
    }
}
