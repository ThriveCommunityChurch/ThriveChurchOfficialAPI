using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ThriveChurchOfficialAPI.Core.System.ExceptionHandler
{
    /// <summary>
    /// Exception Handler Middleware. Logs to file if an exception ocurrs.
    /// Will auto assign guid to error, without revealing to users what occurred
    /// </summary>
    public class ExceptionHandler
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
 
        /// <summary>
        /// Exception C'tor
        /// </summary>
        /// <param name="next"></param>
        /// <param name="logger"></param>
        public ExceptionHandler(RequestDelegate next, ILogger<ExceptionHandler> logger)
        {
            _logger = logger;
            _next = next;
        }
 
        /// <summary>
        /// On each request listen for exceptions
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                // create an exception Guid so we can look it up later in the logs
                var exceptionId = Guid.NewGuid().ToString();

                _logger.LogError(string.Format(SystemMessages.ExceptionMessage, exceptionId, ex));
                await HandleExceptionAsync(httpContext, exceptionId);
            }
        }
 
        /// <summary>
        /// In the event an exception occurs, notify the user of the exception Id
        /// </summary>
        /// <param name="context"></param>
        /// <param name="exceptionId"></param>
        /// <returns></returns>
        private static Task HandleExceptionAsync(HttpContext context, string exceptionId)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 500;
 
            return context.Response.WriteAsync(new ExceptionHandlerResponse
            {
                Message = string.Format(SystemMessages.UnknownExceptionOcurred, exceptionId)
            }.ToString());
        }
    }
}
