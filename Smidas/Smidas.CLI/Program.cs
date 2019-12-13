using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
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
            using var serviceProvider = ConfigureServices().BuildServiceProvider();
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
            var config = LoadConfiguration();

            var services = new ServiceCollection();
            services.AddOptions();
            services.AddLogging(logging => 
            { 
                logging.AddConsole();
                logging.AddConfiguration(config.GetSection("Logging"));
            });
            
            services.Configure<AppSettings>(config.GetSection("App"));
            services.Configure<ConsoleLoggerOptions>(config.GetSection("Logging"));

            services.AddScoped<DagensIndustriWebScraper>();
            services.AddScoped<AktieRea>();
            services.AddScoped<ExcelExporter>();
            services.AddScoped<ConsoleApplication>();

            return services;
        }
    }
}
