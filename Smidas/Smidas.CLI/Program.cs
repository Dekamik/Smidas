using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO;

namespace Smidas.CLI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var serviceProvider = ConfigureServices().BuildServiceProvider();

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
            var services = new ServiceCollection();

            var config = LoadConfiguration();
            services.AddOptions();
            services.Configure<AppSettings>(config.GetSection("Configuration"));

            services.AddLogging(logging =>
                {
                    logging.AddConsole();
                    logging.AddFilter("Smidas", LogLevel.Debug);
                    logging.AddFilter("Smidas.WebScraping", LogLevel.Trace);
                })
                .Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Information);

            services.AddScoped<ConsoleApplication>();

            return services;
        }
    }
}
