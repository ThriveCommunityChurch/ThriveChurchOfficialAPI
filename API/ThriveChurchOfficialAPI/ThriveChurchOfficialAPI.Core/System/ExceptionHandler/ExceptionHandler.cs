using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;
using ThriveChurchOfficialAPI.Core.System.Variables;

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

        private static ILog _log { get; set; }
 
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

                _log = LogManager.GetLogger(typeof(ExceptionHandler));

                // log this as a critical failure in the console
                _logger.LogCritical(string.Format(SystemMessages.ExceptionMessage, exceptionId, ex));

                GenerateEmail(exceptionId);

                // log this as fatal in the logfile
                _log.Fatal(string.Format(SystemMessages.ExceptionMessage, exceptionId, ex));
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

        /// <summary>
        /// Send an email to notify us that exceptions are occurring
        /// </summary>
        private void GenerateEmail(string exceptionId)
        {
            using (SmtpClient smtpClient = new SmtpClient())
            {
                var basicCredential = new NetworkCredential("api@thrive-fl.org", SystemVariables.EmailPW);
                using (MailMessage message = new MailMessage())
                {
                    MailAddress fromAddress = new MailAddress("api@thrive-fl.org");

                    smtpClient.Host = "smtp.gmail.com";
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = basicCredential;
                    smtpClient.Port = 587;
                    smtpClient.EnableSsl = true;

                    message.From = fromAddress;

                    // make the subject something descriptive, and include the last 6 digits of the exception id
                    message.Subject = string.Format("Exceptions with ThriveChurchOfficialAPI - {0}", exceptionId.Split('-')[4].Remove(0, 6));
                    message.IsBodyHtml = true;
                    message.Body = string.Format("Unknown exception occurred. \n\nSee exception with Id: {0}", exceptionId);
                    message.To.Add("wyatt@thrive-fl.org");

                    try
                    {
                        smtpClient.Send(message);
                    }
                    catch (Exception ex)
                    {
                        //Error, could not send the message
                        _log.Error(ex);
                        _logger.LogCritical(string.Format("Exception Sending email: {0}", ex));
                    }
                }
            }
        }
    }
}
