using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Smidas.Batch;
using Smidas.Common;
using Smidas.Core.Analysis;
using Smidas.Exporting.Excel;
using Smidas.Function;
using Smidas.WebScraping.WebScrapers.DagensIndustri;

[assembly: FunctionsStartup(typeof(Startup))]
namespace Smidas.Function
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddLogging();

            builder.Services
                   .AddScoped<DagensIndustriWebScraper>()
                   .AddScoped<AktieRea>()
                   .AddScoped<ExcelExporter>()
                   .AddScoped<AktieReaJob>();
        }
    }
}
