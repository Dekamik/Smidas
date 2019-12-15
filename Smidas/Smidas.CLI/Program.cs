using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Smidas.Batch;
using Smidas.Common;
using Smidas.Core.Analysis;
using Smidas.Exporting.Excel;
using Smidas.WebScraping.WebScrapers.DagensIndustri;
using System.IO;

namespace Smidas.CLI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            using ServiceProvider serviceProvider = ConfigureServices().BuildServiceProvider();
            serviceProvider.GetService<ConsoleApplication>().Run();
        }

        private static IConfigurationRoot LoadConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false)
                .Build();
        }

        private static IServiceCollection ConfigureServices()
        {
            IConfigurationRoot config = LoadConfiguration();

            ServiceCollection services = new ServiceCollection();
            services.AddOptions();
            services.AddLogging(logging => 
            { 
                logging.AddConsole();
                logging.AddConfiguration(config.GetSection("Logging"));
            });
            
            services.Configure<AppSettings>(config.GetSection("App"));
            services.Configure<ConsoleLoggerOptions>(config.GetSection("Logging"));

            services.AddScoped<AktieReaJob>();
            services.AddScoped<DagensIndustriWebScraper>();
            services.AddScoped<AktieRea>();
            services.AddScoped<ExcelExporter>();
            services.AddScoped<ConsoleApplication>();

            return services;
        }
    }
}
