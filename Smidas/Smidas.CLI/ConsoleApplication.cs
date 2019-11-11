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

        private readonly AppSettings _config;

        private readonly ExcelExporter _excelExporter;

        public ConsoleApplication(ILoggerFactory loggerFactory, IOptions<AppSettings> config, ExcelExporter excelExporter)
        {
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger<ConsoleApplication>();
            _config = config.Value;
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

            using var webDriver = new ChromeDriver(_config.ChromeDriverDirectory);
            string exportPath = null;
            IWebScraper webScraper = null;
            IAnalysis analysis = null;

            switch (input)
            {
                case "1":
                    exportPath = _config.ExportDirectory + $"\\AktieREA_{DateTime.Now.ToString("yyyy-MM-dd_HHmm")}.xlsx";
                    break;

                case "2":
                    exportPath = _config.ExportDirectory + $"\\AktieREA_OMX_Köpenhamn_Large_Cap_{DateTime.Now.ToString("yyyy-MM-dd_HHmm")}.xlsx";
                    break;

                case "3":
                    exportPath = _config.ExportDirectory + $"\\AktieREA_OMX_Helsingfors_Large_Cap_{DateTime.Now.ToString("yyyy-MM-dd_HHmm")}.xlsx";
                    break;

                default:
                    break;
            }

            // Select webscraper
            switch (input)
            {
                case "1":
                    webScraper = new AffarsVarldenWebScraper(webDriver, _loggerFactory, _config, AffarsVarldenIndexes.StockholmLargeCap);
                    break;

                case "2":
                    webScraper = new AffarsVarldenWebScraper(webDriver, _loggerFactory, _config, AffarsVarldenIndexes.CopenhagenLargeCap);
                    break;

                case "3":
                    webScraper = new AffarsVarldenWebScraper(webDriver, _loggerFactory, _config, AffarsVarldenIndexes.HelsinkiLargeCap);
                    break;

                default:
                    break;
            }

            // Select analysis
            switch (input)
            {
                case "1":
                case "2":
                case "3":
                    analysis = new AktieRea(_loggerFactory, _config);
                    break;

                default:
                    break;
            }

            // Run
            var stockData = webScraper?.Scrape();
            var results = analysis?.Analyze(stockData, _config.Blacklist);

            if (!string.IsNullOrEmpty(exportPath))
            {
                _excelExporter.Export(results.ToList(), exportPath);
            }

            stopwatch.Stop();
            _logger.LogInformation($"Smidas avslutad. Körtid för utvald åtgärd: {stopwatch.Elapsed}");
        }
    }
}
