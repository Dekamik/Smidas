using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Smidas.Batch;
using Smidas.Core.Analysis;
using Smidas.Exporting.Excel;
using Smidas.WebScraping.WebScrapers.DagensIndustri;

[assembly: FunctionsStartup(typeof(Smidas.Function.Startup))]
namespace Smidas.Function
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddLogging();

            builder.Services.AddScoped<DagensIndustriWebScraper>();
            builder.Services.AddScoped<AktieRea>();
            builder.Services.AddScoped<ExcelExporter>();
            builder.Services.AddScoped<AktieReaJob>();
        }
    }
}
