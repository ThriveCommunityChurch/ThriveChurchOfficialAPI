using log4net;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using ThriveChurchOfficialAPI.Core;
using ThriveChurchOfficialAPI.Repositories;

namespace ThriveChurchOfficialAPI.Services
{
    public class AuthHandler
    {
        private readonly RequestDelegate _next;

        private ITokenRepo TokenRepo { get; set; }

        private static ILog Logger 
        {
            get { return LogManager.GetLogger(typeof(AuthHandler)); }
        }

        public AuthHandler(RequestDelegate next, ITokenRepo _repo)
        {
            _next = next;
            TokenRepo = _repo;
        }

        /// <summary>
        /// Read the headers on the every request and authenticate requests for a whitelisted ApiKey
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            // allow users to navigate to the swagger page
            if (context.Request.Path == "/swagger/index.html" ||
            context.Request.Path == "/favicon.ico" ||
            context.Request.Path == "/swagger/v1/swagger.json"
               )
            {
                await _next.Invoke(context);
                return;
            }

            if (!context.Request.Headers.Keys.Contains("Authorization"))
            {
                context.Response.StatusCode = 401; // Unauthorized      
                await context.Response.WriteAsync(SystemMessages.NoApiKey);

                Logger.Warn(string.Format(SystemMessages.NoAPIKeyDebug, GetIP(context) ?? context.Request.Headers["UserAgent"]));

                return;
            }
            else
            {
                var requestedToken = context.Request.Headers["Authorization"].ToString();

                // Prevent any kind of NoSQL syntax
                if (requestedToken.Contains('}') || 
                    requestedToken.Contains('{') || 
                    requestedToken.Contains('$') ||
                    requestedToken.Contains('\"') ||
                    requestedToken.Contains('"') ||
                    requestedToken.Contains(';'))
                {
                    await GenerateFailureForInvalidKey(context);
                    return;
                }

                var validKey = Guid.TryParse(requestedToken, out Guid _);
                if (!validKey)
                {
                    await GenerateFailureForInvalidKey(context);
                    return;
                }

                // the key looks formed correctly. Does the salt match?
                var keySalt = TokenHandler.GenerateHashedKey(requestedToken);

                var validationresponse = TokenRepo.ValidateToken(keySalt);
                if (validationresponse.HasErrors)
                {
                    await GenerateFailureForInvalidKey(context);
                    return;
                }
            }

            await _next.Invoke(context);
        }

        /// <summary>
        /// Generate a 401 Unauthorized response for an invalid API Key
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task GenerateFailureForInvalidKey(HttpContext context)
        {
            var requestedToken = context.Request.Headers["Authorization"];

            context.Response.StatusCode = 401;

            Logger.Error(string.Format(SystemMessages.InvalidAPIKeyDebug, GetIP(context) ?? context.Request.Headers["UserAgent"], requestedToken));
            await context.Response.WriteAsync(SystemMessages.WrongApiKey);
        }

        /// <summary>
        /// Get the IP address of the requesting user
        /// </summary>
        /// <returns></returns>
        private static string GetIP(HttpContext context)
        {
            var ip = context.Connection.RemoteIpAddress.ToString();
            return ip;
        }
    }
}
