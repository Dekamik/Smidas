using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OfficeOpenXml;
using Smidas.Common.Attributes;
using Smidas.Core.Analysis;
using Smidas.Exporting.Excel;
using Smidas.WebScraping.WebScrapers.DagensIndustri;

namespace Smidas.API.BatchJobs
{
    public class BatchJobService : IBatchJobService
    {
        private readonly AppSettings _settings;
        private readonly IExcelExporter _excelExporter;
        private readonly ILogger<BatchJobService> _logger;
        private readonly IDagensIndustriWebScraper _webScraper;
        private readonly IAktieRea _aktieRea;

        public BatchJobService(
            IOptions<AppSettings> settings, 
            IExcelExporter excelExporter,
            ILogger<BatchJobService> logger,
            IDagensIndustriWebScraper webScraper,
            IAktieRea aktieRea)
        {
            _aktieRea = aktieRea;
            _webScraper = webScraper;
            _logger = logger;
            _excelExporter = excelExporter;
            _settings = settings.Value;
        }
        
        public void RunOnIndex(string index)
        {
            if (!_settings.ScrapingSets.ContainsKey(index))
                throw new NullReferenceException($"{index} couldn't resolve to a predefined AktieRea index");
            
            var query = _settings.ScrapingSets
                .Single(i => i.Key == index)
                .Value;

            var exportPath = Path.Combine(query.ExportDirectory ?? "~",
                $"AktieREA {index} {DateTimeOffset.Now:u}.xlsx");

            var stockData = _webScraper.Scrape(query);
            var analysisResult = _aktieRea.Analyze(query, stockData).ToList();
            
            _logger.LogInformation($"Exporting analysis to {exportPath}");

            using ExcelPackage excel = new ExcelPackage(new FileInfo(exportPath));
            ExcelWorksheet worksheet = excel.Workbook.Worksheets.Add($"AktieREA {DateTime.Today:yyyy-MM-dd}");
            
            _excelExporter.ExportStocksToWorksheet(ref worksheet, analysisResult, query.CurrencyCode);
            
            excel.Save();
        }
    }
}