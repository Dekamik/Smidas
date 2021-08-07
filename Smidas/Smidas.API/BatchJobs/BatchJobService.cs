using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OfficeOpenXml;
using Smidas.Batch;
using Smidas.Exporting.Excel;

namespace Smidas.API.BatchJobs
{
    public class BatchJobService : IBatchJobService
    {
        private readonly AppSettings _settings;
        private readonly IAktieReaJob _aktieReaJob;
        private readonly IExcelExporter _excelExporter;
        private readonly ILogger<BatchJobService> _logger;

        public BatchJobService(
            IOptions<AppSettings> settings, 
            IAktieReaJob aktieReaJob, 
            IExcelExporter excelExporter,
            ILogger<BatchJobService> logger)
        {
            _logger = logger;
            _excelExporter = excelExporter;
            _aktieReaJob = aktieReaJob;
            _settings = settings.Value;
        }

        public async Task RunOnIndex(string index)
        {
            if (!_settings.ScrapingSets.ContainsKey(index))
                throw new NullReferenceException($"{index} couldn't resolve to a predefined AktieRea index");
            
            var query = _settings.ScrapingSets
                .Single(i => i.Key == index)
                .Value;

            var exportPath = Path.Combine(query.ExportDirectory ?? "~",
                $"AktieREA {index} {DateTimeOffset.Now:u}.xlsx");

            var stocks = (await _aktieReaJob.Run(query)).ToList();
            
            _logger.LogInformation($"Exporting analysis on {stocks.Count} stocks to {exportPath}");

            using ExcelPackage excel = new ExcelPackage(new FileInfo(exportPath));
            ExcelWorksheet worksheet = excel.Workbook.Worksheets.Add($"AktieREA {DateTime.Today:yyyy-MM-dd}");
            
            _excelExporter.ExportStocksToWorksheet(ref worksheet, stocks, query.CurrencyCode);
            
            excel.Save();
        }
    }
}