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
using Swashbuckle.AspNetCore.Swagger;
using ThriveChurchOfficialAPI.Services;
using ThriveChurchOfficialAPI.Repositories;
using Newtonsoft.Json.Serialization;
using AspNetCoreRateLimit;
using System.Reflection;
using System.IO;
using ThriveChurchOfficialAPI.Core.System.ExceptionHandler;
using ThriveChurchOfficialAPI.Core;
using Serilog;
using Hangfire.Mongo;
using Hangfire;

namespace ThriveChurchOfficialAPI
{
    #pragma warning disable CS1591

    public class Startup
    {

        /// <summary>
        /// System Configruation
        /// </summary>
        public IConfigurationRoot Configuration { get; set; }


        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(env.ContentRootPath)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
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

            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File("C:/logs/logfile.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();

            #endregion

            // Add our Config object so it can be injected later
            services.Configure<AppSettings>(options => Configuration.GetSection("EsvApiKey").Bind(options));
            services.Configure<AppSettings>(options => Configuration.GetSection("MongoConnectionString").Bind(options));
            services.Configure<AppSettings>(options => Configuration.GetSection("OverrideEsvApiKey").Bind(options));
            services.Configure<AppSettings>(options => Configuration.GetSection("EmailPW").Bind(options));

            services.AddSingleton(Configuration);

            // Manually register DI dependencies
            services.AddTransient(typeof(ISermonsService), typeof(SermonsService));
            services.AddTransient(typeof(IPassagesRepository), typeof(PassagesRepository));
            services.AddTransient(typeof(ISermonsRepository), typeof(SermonsRepository));
            services.AddTransient(typeof(IPassagesService), typeof(PassagesService));

            #region Hangfire Tasks

            var hangfireMigrationOptions = new MongoMigrationOptions
            {
                Strategy = MongoMigrationStrategy.Migrate,
                BackupStrategy = MongoBackupStrategy.Collections
            };

            var hangfireStorageOptions = new MongoStorageOptions
            {
                MigrationOptions = hangfireMigrationOptions
            };

            // Add framework services.
            services.AddHangfire(config =>
            {
                config.UseMongoStorage(Configuration["HangfireConnectionString"], hangfireStorageOptions);
            });

            #endregion
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

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // add exception filtering 
            app.ConfigureCustomExceptionMiddleware();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Thrive Church Official API v1");
                c.RoutePrefix = "swagger"; // enable swagger at ~/swagger  
            });

            #region Hangfire Tasks

            app.UseHangfireServer(new BackgroundJobServerOptions() { WorkerCount = 2 });

            // Map Dashboard to the `http://<your-app>/hf-dashboard` URL.
            //app.UseHangfireDashboard("/hf-dashboard", new DashboardOptions { IsReadOnlyFunc = (DashboardContext context) => true }); // read only for prod
            app.UseHangfireDashboard("/hf-dashboard");

            #endregion

            app.UseIpRateLimiting(); // enable rate limits

            app.UseMvc();
        }
    }

    #pragma warning restore CS1591
}