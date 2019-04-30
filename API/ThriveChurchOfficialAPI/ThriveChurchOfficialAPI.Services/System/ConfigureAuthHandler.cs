using Microsoft.AspNetCore.Builder;

namespace ThriveChurchOfficialAPI.Services
{
    public static class ConfigureAuthHandler
    {
        public static IApplicationBuilder UseApiKeyValidation(this IApplicationBuilder app)
        {
            app.UseMiddleware<AuthHandler>();
            return app;
        }
    }
}
