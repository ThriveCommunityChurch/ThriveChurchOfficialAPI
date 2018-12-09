using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Configuration;
using Swashbuckle.AspNetCore.Swagger;
using ThriveChurchOfficialAPI.Services;
using ThriveChurchOfficialAPI.Repositories;

namespace ThriveChurchOfficialAPI
{
    public class Startup
    {
        public readonly string EsvApiKey;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(env.ContentRootPath)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; set; }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // enable in-memory caching
            services.AddMemoryCache();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Thrive Church Official API", Version = "v1" });

                // Set the comments path for the Swagger JSON and UI.
                //var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                //var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                //c.IncludeXmlComments(xmlPath);
            });

            services.AddMvc();
            // Add functionality to inject IOptions<T>
            services.AddOptions();

            // Add our Config object so it can be injected later
            services.Configure<AppSettings>(options => Configuration.GetSection("EsvApiKey").Bind(options));
            services.Configure<AppSettings>(options => Configuration.GetSection("MongoConnectionString").Bind(options));

            services.AddSingleton(Configuration);

            // do DI properly
            services.AddTransient(typeof(ISermonsService), typeof(SermonsService));
            services.AddTransient(typeof(IPassagesRepository), typeof(PassagesRepository));
            services.AddTransient(typeof(ISermonsRepository), typeof(SermonsRepository));
            services.AddTransient(typeof(IPassagesService), typeof(PassagesService));
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

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Thrive Church Official API v1");
                c.RoutePrefix = "swagger"; // enable swagger at ~/swagger  
            });

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
