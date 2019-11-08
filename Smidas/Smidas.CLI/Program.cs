using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Smidas.CLI.Jobs;
using Smidas.Core.Stocks;
using System.Collections.Generic;

namespace Smidas.CLI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var serviceProvider = ConfigureServices().BuildServiceProvider();

            serviceProvider.GetService<ConsoleApplication>().Run();
        }

        private static IServiceCollection ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddLogging(logging =>
                {
                    logging.AddConsole();
                    logging.AddFilter("Smidas", LogLevel.Debug);
                    logging.AddFilter("Smidas.WebScraping", LogLevel.Trace);
                })
                .Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Information);

            services.AddScoped<WebScrapingJob>();
            services.AddScoped<ConsoleApplication>();

            return services;
        }
    }
}
