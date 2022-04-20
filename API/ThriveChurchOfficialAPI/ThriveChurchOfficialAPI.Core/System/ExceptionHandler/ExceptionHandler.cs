using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;
using System.Net;
using System.Net.Mail;
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
        private readonly IConfiguration _configuration;
 
        /// <summary>
        /// Exception C'tor
        /// </summary>
        /// <param name="next"></param>
        /// <param name="logger"></param>
        public ExceptionHandler(RequestDelegate next, IConfiguration Configuration)
        {
            _next = next;
            _configuration = Configuration;
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
                string exceptionId = Guid.NewGuid().ToString();

                // log this as fatal in the logfile
                Log.Fatal(string.Format(SystemMessages.ExceptionMessage, exceptionId, ex));

                try  
                {
                    StringBuilder sb = new StringBuilder();

                    if (ex.Message.Contains("IP", StringComparison.OrdinalIgnoreCase))
                    {
                        HttpRequest request = httpContext.Request;

                        sb.AppendLine("\nHeaders:");
                        foreach (var header in request.Headers)
                        {
                            sb.AppendLine($"{header.Key} = {header.Value}");
                        }

                        sb.AppendLine("\nRequestInfo:");
                        var protocolText = request.IsHttps ? "Https" : "Http";
                        sb.AppendLine($"{protocolText} {request.Method} {request.Path}{request.QueryString}");

                        sb.AppendLine("\nRequestBody:");
                        sb.AppendLine(GetRequestBody(request));

                        Log.Fatal($"Caller has invalid IP address. We gathered the following data. {sb}");
                    }
                }
                catch (Exception e)
                {
                    Log.Fatal(string.Format(SystemMessages.ExceptionMessage, exceptionId, e));
                    Log.Fatal("ABOVE ERROR OCURRED WHEN ATTEMPTING TO READ INFO ON UNKNOWN CALLER.");
                }

                GenerateEmail(exceptionId);

                await HandleExceptionAsync(httpContext, exceptionId);
            }
        }

        private static string GetRequestBody(HttpRequest request)
        {
            var bodyStr = "";

            // Allows using several time the stream in ASP.Net Core
            request.EnableBuffering();

            // Arguments: Stream, Encoding, detect encoding, buffer size 
            // AND, the most important: keep stream opened
            using (StreamReader reader
                      = new StreamReader(request.Body, Encoding.UTF8, true, 1024, true))
            {
                bodyStr = reader.ReadToEndAsync().Result;
            }

            // Rewind, so the core is not lost when it looks the body for the request
            request.Body.Position = 0;

            // Do whatever work with bodyStr here
            return bodyStr;
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
        /// <param name="exceptionId"></param>
        private void GenerateEmail(string exceptionId)
        {
            using (SmtpClient smtpClient = new SmtpClient())
            {
                NetworkCredential basicCredential = new NetworkCredential("api@thrive-fl.org", _configuration["EmailPW"]);
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
                        // Error, could not send the message
                        Log.Error(string.Format("Error sending email: {0}", ex));
                    }
                }
            }
        }
    }
}
