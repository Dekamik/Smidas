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
        private readonly IOptions<AppSettings> _settings;
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
            _settings = settings;
        }

        public async Task RunOnIndex(string index)
        {
            if (!_settings.Value.AktieRea.ContainsKey(index))
                throw new NullReferenceException($"{index} couldn't resolve to a predefined AktieRea index");
            
            var query = _settings.Value.AktieRea
                .Single(i => i.Key == index)
                .Value;

            var exportPath = Path.Combine(query.ExportDirectory ?? "~",
                $"AktieREA_{index}_{DateTime.UtcNow:yyyy-MM-dd_HHmm}.xlsx");

            var stocks = (await _aktieReaJob.Run(query)).ToList();
            
            _logger.LogInformation($"Exporterar analys om {stocks.Count} aktier till {exportPath}");

            using ExcelPackage excel = new ExcelPackage(new FileInfo(exportPath));
            ExcelWorksheet worksheet = excel.Workbook.Worksheets.Add($"AktieREA {DateTime.Today:yyyy-MM-dd}");
            
            _excelExporter.ExportStocksToWorksheet(ref worksheet, stocks, query.CurrencyCode);
            
            excel.Save();
            
            _logger.LogInformation("Exportering slutförd");
        }
    }
}