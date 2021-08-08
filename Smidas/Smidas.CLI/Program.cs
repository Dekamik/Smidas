using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Smidas.Batch;
using Smidas.Core.Analysis;
using Smidas.Exporting.Excel;
using Smidas.WebScraping.WebScrapers;
using Smidas.WebScraping.WebScrapers.DagensIndustri;
using System.IO;
using System.Threading.Tasks;

namespace Smidas.CLI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await using ServiceProvider serviceProvider = ConfigureServices().BuildServiceProvider();
            await serviceProvider.GetService<ConsoleApplication>().Run();
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
            
            services.Configure<AppSettings>(config.GetSection("AktieRea"));
            services.Configure<ConsoleLoggerOptions>(config.GetSection("Logging"));

            services.AddScoped<IAktieReaJob, AktieReaJob>();
            services.AddScoped<IDagensIndustriWebScraper, DagensIndustriWebScraper>();
            services.AddScoped<IAktieRea, AktieRea>();
            services.AddScoped<IExcelExporter, ExcelExporter>();
            services.AddScoped<ConsoleApplication>();

            return services;
        }
    }
}
