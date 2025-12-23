/*
    MIT License

    Copyright (c) 2026 Thrive Community Church

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

using AspNetCoreRateLimit;
using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Server.IISIntegration;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Serilog;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using ThriveChurchOfficialAPI.Core;
using ThriveChurchOfficialAPI.Core.System.ExceptionHandler;
using ThriveChurchOfficialAPI.Repositories;
using ThriveChurchOfficialAPI.Services;

namespace ThriveChurchOfficialAPI
{
#pragma warning disable CS1591

    public class Startup
    {

        /// <summary>
        /// System Configruation
        /// </summary>
        public IConfigurationRoot Configuration { get; set; }

        readonly string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

        public Startup(IWebHostEnvironment env)
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
            services.AddCors(options =>
            {
                options.AddPolicy(name: MyAllowSpecificOrigins,
                                  builder =>
                                  {
                                      builder.WithOrigins("*").AllowAnyHeader().AllowAnyMethod();
                                  });
            });

            #region Response Compression

            // Add response compression for mobile bandwidth optimization
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true; // Enable compression for HTTPS
                options.Providers.Add<GzipCompressionProvider>();

                // Add MIME types to compress (JSON responses)
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "application/json", "text/json" });
            });

            // Configure Gzip compression level
            services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Fastest; // Balance between speed and compression ratio
            });

            #endregion

            // enable in-memory caching
            services.AddMemoryCache();

            services.AddMvc(options =>
            {
                // if we ever get to 50 Model Validation errors, ignore subsequent ones
                // more on this here https://docs.microsoft.com/en-us/aspnet/core/mvc/models/validation?view=aspnetcore-2.2#top-level-node-validation
                options.MaxModelValidationErrors = 50;
            });

            // Configure request size limits for file uploads
            services.Configure<IISServerOptions>(options =>
            {
                options.MaxRequestBodySize = 52428800; // 50MB in bytes
            });

            services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = 52428800; // 50MB in bytes
            });

            services.Configure<FormOptions>(options =>
            {
                options.ValueLengthLimit = int.MaxValue;
                options.MultipartBodyLengthLimit = 52428800; // 50MB in bytes
                options.MultipartHeadersLengthLimit = int.MaxValue;
            });

            services.AddHsts(options =>
            {
                options.MaxAge = TimeSpan.FromDays(365);
                options.IncludeSubDomains = true;
            });

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Thrive Church Official API", Version = "v1" });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

                // Add JWT Authentication to Swagger
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });

            // Preserve Casing of JSON Objects
            services
                .AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    options.JsonSerializerOptions.PropertyNamingPolicy = null;
                });

            // Add functionality to inject IOptions<T>
            services.AddOptions();

            #region Rate Limiting

            // load configuration from appsettings.json
            services.Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimiting"));

            // load IP rules from appsettings.json
            services.Configure<IpRateLimitPolicies>(Configuration.GetSection("IpRateLimitPolicies"));

            services.AddInMemoryRateLimiting();

            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

            // inject counter and rules stores
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();

            #endregion

            #region File Logging

            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File("logs/logfile.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();

            #endregion

            // Add our Config object so it can be injected later
            services.Configure<AppSettings>(options => Configuration.GetSection("EsvApiKey").Bind(options));
            services.Configure<AppSettings>(options => Configuration.GetSection("MongoConnectionString").Bind(options));
            services.Configure<AppSettings>(options => Configuration.GetSection("OverrideEsvApiKey").Bind(options));
            services.Configure<AppSettings>(options => Configuration.GetSection("EmailPW").Bind(options));
            services.Configure<AwsSettings>(options => Configuration.GetSection("S3").Bind(options));
            services.Configure<JwtSettings>(options => Configuration.GetSection("JWT").Bind(options));

            services.AddSingleton(Configuration);

            #region JWT Authentication Configuration

            // Configure JWT Authentication
            var jwtSettings = Configuration.GetSection("JWT");
            var secretKey = jwtSettings["SecretKey"];
            var key = Encoding.ASCII.GetBytes(secretKey);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                // RequireHttpsMetadata: This setting controls whether HTTPS is required when
                // fetching OpenID Connect metadata (like signing keys from /.well-known/openid-configuration).
                // Since we use a locally-stored symmetric key (not fetched from an external identity provider),
                // this setting doesn't affect our security. Setting to false allows the JWT middleware
                // to work properly behind AWS App Runner's load balancer (which terminates TLS).
                // All actual token validation (signature, issuer, audience, expiration) still applies.
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = jwtSettings["Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(60)
                };
            });

            services.AddAuthorization();

            #endregion

            #region Health Checks

            // Add ASP.NET Core Health Checks for AWS App Runner health probes
            // The HealthController uses [AllowAnonymous] to bypass JWT authentication
            // for internal health check traffic from the load balancer
            services.AddHealthChecks();

            #endregion

            // Manually register DI dependencies
            services.AddTransient(typeof(ISermonsService), typeof(SermonsService));
            services.AddTransient(typeof(IPassagesRepository), typeof(PassagesRepository));
            services.AddTransient(typeof(ISermonsRepository), typeof(SermonsRepository));
            services.AddTransient(typeof(IPassagesService), typeof(PassagesService));
            services.AddTransient(typeof(IConfigService), typeof(ConfigService));
            services.AddTransient(typeof(IConfigRepository), typeof(ConfigRepository));
            services.AddTransient(typeof(IMessagesRepository), typeof(MessagesRepository));
            services.AddTransient(typeof(IS3Repository), typeof(S3Repository));
            services.AddTransient(typeof(IPodcastMessagesRepository), typeof(PodcastMessagesRepository));

            // Authentication services
            services.AddTransient(typeof(IUserRepository), typeof(UserRepository));
            services.AddTransient(typeof(IRefreshTokenRepository), typeof(RefreshTokenRepository));
            services.AddTransient(typeof(IAuthenticationService), typeof(AuthenticationService));
            services.AddTransient(typeof(IJwtService), typeof(JwtService));

            // Lambda services
            services.AddSingleton(typeof(IPodcastLambdaService), typeof(PodcastLambdaService));

            #region Hangfire Tasks

            var hangfireStorageOptions = new MongoStorageOptions
            {
                MigrationOptions = new MongoMigrationOptions
                {
                    MigrationStrategy = new MigrateMongoMigrationStrategy(),
                    BackupStrategy = new CollectionMongoBackupStrategy()
                }
            };

            // Add framework services.
            services.AddHangfire(config =>
            {
                config.UseMongoStorage(Configuration["HangfireConnectionString"], hangfireStorageOptions);
            });

            Log.Information("Services configured.");

            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Enable response compression (must be early in pipeline)
            app.UseResponseCompression();

            Log.Information("Response compression configured.");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                // Enable middleware to serve generated Swagger as a JSON endpoint.
                app.UseSwagger();

                // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
                // specifying the Swagger JSON endpoint.
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Thrive Church Official API v1");
                    c.RoutePrefix = "swagger"; // enable swagger at ~/swagger
                    c.DefaultModelsExpandDepth(-1); // Disable swagger schemas at bottom
                });
            }
            else
            {
                app.UseHsts();
            }

            Log.Information("UseHsts configured.");

            // add exception filtering
            app.ConfigureCustomExceptionMiddleware();

            Log.Information("Exception middleware configured.");

            #region Hangfire Tasks

            #endregion

            app.UseIpRateLimiting();

            Log.Information("Rate limiting configured.");

            app.UseRouting();

            Log.Information("Routing middleware configured.");

            app.UseCors(MyAllowSpecificOrigins);

            Log.Information("CORS middleware configured.");

            // Add authentication and authorization middleware
            app.UseAuthentication();
            app.UseAuthorization();

            Log.Information("Authentication and authorization middleware configured.");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                // Map the built-in health check endpoint at /health
                // This is in addition to the HealthController which provides more detailed endpoints
                endpoints.MapHealthChecks("/health");
            });

            Log.Information("Service started.");
        }
    }

    #pragma warning restore CS1591
}