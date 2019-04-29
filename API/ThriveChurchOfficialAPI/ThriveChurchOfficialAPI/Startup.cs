/*
    MIT License

    Copyright (c) 2019 Thrive Community Church

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.
*/

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Swagger;
using ThriveChurchOfficialAPI.Services;
using ThriveChurchOfficialAPI.Repositories;
using Newtonsoft.Json.Serialization;
using AspNetCoreRateLimit;
using System.Reflection;
using System.IO;
using ThriveChurchOfficialAPI.Core.System.ExceptionHandler;
using ThriveChurchOfficialAPI.Core;
using log4net.Config;
using log4net;
using System.Collections.Generic;
using System.Linq;
using Swashbuckle.AspNetCore.Filters;

namespace ThriveChurchOfficialAPI
{
    #pragma warning disable CS1591

    public class Startup
    {
        /// <summary>
        /// System Logger
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// System Configruation
        /// </summary>
        public IConfigurationRoot Configuration { get; set; }


        public Startup(IHostingEnvironment env, ILogger<Startup> logger)
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(env.ContentRootPath)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();

            _logger = logger;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // enable in-memory caching
            services.AddMemoryCache();

            services.AddMvc(options =>
            {
                // if we ever get to 50 Model Validation errors, ignore subsequent ones
                // more on this here https://docs.microsoft.com/en-us/aspnet/core/mvc/models/validation?view=aspnetcore-2.2#top-level-node-validation
                options.MaxModelValidationErrors = 50;
            })
            .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Thrive Church Official API", Version = "v1" });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

                // Adding auth to swagger docs
                c.AddSecurityDefinition("oauth2", new ApiKeyScheme
                {
                    Description = "Please enter your API Key.",
                    In = "header",
                    Name = "Authorization",
                    Type = "apiKey"
                });

                c.OperationFilter<SecurityRequirementsOperationFilter>(false);
            });

            // Preserve Casing of JSON Objects
            services.AddMvc()
            .AddJsonOptions(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver());

            // Add functionality to inject IOptions<T>
            services.AddOptions();

            #region Rate Limiting

            // load configuration from appsettings.json
            services.Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimiting"));

            // load IP rules from appsettings.json
            services.Configure<IpRateLimitPolicies>(Configuration.GetSection("IpRateLimitPolicies"));

            // inject counter and rules stores
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();

            #endregion

            #region File Logging

            services.AddLogging(builder =>
            {
                string path = @"C:\";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                builder.AddConfiguration(Configuration.GetSection("Logging"));
                builder.AddFile(o => o.RootPath = path);
            });

            // configure loggers for our error handlers
            SystemResponseBase.ConfigureLogger(_logger);

            #endregion

            // Add our Config object so it can be injected later
            services.Configure<AppSettings>(options => Configuration.GetSection("EsvApiKey").Bind(options));
            services.Configure<AppSettings>(options => Configuration.GetSection("MongoConnectionString").Bind(options));

            services.AddSingleton(Configuration);

            // Manually register DI dependencies
            services.AddTransient(typeof(ISermonsService), typeof(SermonsService));
            services.AddTransient(typeof(IPassagesRepository), typeof(PassagesRepository));
            services.AddTransient(typeof(ISermonsRepository), typeof(SermonsRepository));
            services.AddTransient(typeof(IPassagesService), typeof(PassagesService));
            services.AddTransient(typeof(ITokenRepo), typeof(TokenRepo));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            // add rate limiting
            app.UseIpRateLimiting();

            // add exception filtering 
            app.ConfigureCustomExceptionMiddleware();

            // Custom API Key validation
            app.UseApiKeyValidation();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Thrive Church Official API v1");
                c.RoutePrefix = "swagger"; // enable swagger at ~/swagger  
            });

            // enable log4net for file logging
            var logRepo = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepo, new FileInfo("log4net.config"));

            var log = LogManager.GetLogger(typeof(Startup));
            log.Warn("Starting application...");

            // Enable Swagger middleware as a JSON endpoint.
            app.UseSwagger();

            // finally enable the MVC middleware
            app.UseMvc();
        }
    }

    #pragma warning restore CS1591
}
