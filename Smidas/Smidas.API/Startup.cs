using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;
using Smidas.API.BatchJobs;
using Smidas.Core.Analysis;
using Smidas.Exporting.Excel;
using Smidas.WebScraping.WebScrapers;
using Smidas.WebScraping.WebScrapers.DagensIndustri;

namespace Smidas.API
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        
        public Startup(IWebHostEnvironment env)
        {
            Configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", false, true)
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .CreateLogger();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<AppSettings>(Configuration.GetSection(nameof(AppSettings)));

            services.AddScoped<IWebScraper, DagensIndustriWebScraper>();
            services.AddScoped<IAktieRea, AktieRea>();
            services.AddScoped<IExcelExporter, ExcelExporter>();
            services.AddScoped<IBatchJobService, BatchJobService>();
            
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "Smidas.API", Version = "v1"});
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Smidas.API v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}