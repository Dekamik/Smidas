using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenQA.Selenium.Chrome;
using Smidas.Common;
using Smidas.Core.Analysis;
using Smidas.Exporting.Excel;
using Smidas.WebScraping;
using Smidas.WebScraping.AffarsVarlden;
using System;
using System.Diagnostics;
using System.Linq;

namespace Smidas.CLI
{
    public class ConsoleApplication
    {
        private readonly string _menu = @"
   Smidas

-----------------------------------------------------
   AktieREA-analyser

1: AktieREA - OMX Stockholm Large Cap
2: AktieREA - OMX Köpenhamn Large Cap
3: AktieREA - OMX Helsingfors Large Cap

-----------------------------------------------------
   Övriga åtgärder

0: Avsluta

-----------------------------------------------------";

        private readonly ILoggerFactory _loggerFactory;

        private readonly ILogger _logger;

        private readonly IOptions<AppSettings> _config;

        private readonly IWebScraper _webScraper;

        private readonly IAnalysis _analysis;

        private readonly ExcelExporter _excelExporter;

        public ConsoleApplication(ILoggerFactory loggerFactory, IOptions<AppSettings> config, AffarsVarldenWebScraper webScraper, AktieRea aktieRea, ExcelExporter excelExporter)
        {
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger<ConsoleApplication>();
            _config = config;
            _webScraper = webScraper;
            _analysis = aktieRea;
            _excelExporter = excelExporter;
        }

        public void Run()
        {
            Console.WriteLine(_menu);
            Console.Write(">> ");
            var input = Console.ReadLine();

            Console.WriteLine();

            if (input == "0")
            {
                Console.WriteLine("Avslutar");
                Environment.Exit(0);
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            string exportPath = null;

            switch (input)
            {
                case "1":
                    exportPath = _config.Value.ExportDirectory + $"\\AktieREA_{DateTime.Now.ToString("yyyy-MM-dd_HHmm")}.xlsx";
                    break;

                case "2":
                    exportPath = _config.Value.ExportDirectory + $"\\AktieREA_OMX_Köpenhamn_Large_Cap_{DateTime.Now.ToString("yyyy-MM-dd_HHmm")}.xlsx";
                    break;

                case "3":
                    exportPath = _config.Value.ExportDirectory + $"\\AktieREA_OMX_Helsingfors_Large_Cap_{DateTime.Now.ToString("yyyy-MM-dd_HHmm")}.xlsx";
                    break;

                default:
                    break;
            }

            // Select webscraper
            switch (input)
            {
                case "1":
                    (_webScraper as AffarsVarldenWebScraper).Index = AffarsVarldenIndexes.StockholmLargeCap;
                    break;

                case "2":
                    (_webScraper as AffarsVarldenWebScraper).Index = AffarsVarldenIndexes.CopenhagenLargeCap;
                    break;

                case "3":
                    (_webScraper as AffarsVarldenWebScraper).Index = AffarsVarldenIndexes.HelsinkiLargeCap;
                    break;

                default:
                    break;
            }

            // Run
            var stockData = _webScraper?.Scrape();
            var results = _analysis?.Analyze(stockData);

            if (!string.IsNullOrEmpty(exportPath))
            {
                _excelExporter.Export(results.ToList(), exportPath);
            }

            stopwatch.Stop();
            _logger.LogInformation($"Smidas avslutad. Körtid för utvald åtgärd: {stopwatch.Elapsed}");
        }
    }
}
