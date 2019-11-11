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

1a: AktieREA - OMX Stockholm Large Cap
1b: AktieREA - OMX Köpenhamn Large Cap (N/A)
1c: AktieREA - OMX Helsingfors Large Cap (N/A)

-----------------------------------------------------
    Testkörningar

2a: AffarsVarldenWebScraper (OMX Stockholm Large Cap)

-----------------------------------------------------
    Övriga åtgärder

0:  Avsluta

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
                case "1a":
                case "2a":
                    exportPath = _config.ExportDirectory + $"\\AktieREA_{DateTime.Now.ToString("yyyy-MM-dd_HHmm")}.xlsx";
                    break;

                default:
                    break;
            }

            // Select webscraper
            switch (input)
            {
                case "1a":
                case "2a":
                    webScraper = new AffarsVarldenWebScraper(webDriver, _loggerFactory, _config, AffarsVarldenIndexes.StockholmLargeCap);
                    break;

                case "1b":
                    webScraper = new AffarsVarldenWebScraper(webDriver, _loggerFactory, _config, AffarsVarldenIndexes.CopenhagenLargeCap);
                    break;

                case "1c":
                    webScraper = new AffarsVarldenWebScraper(webDriver, _loggerFactory, _config, AffarsVarldenIndexes.HelsinkiLargeCap);
                    break;

                default:
                    break;
            }

            // Select analysis
            switch (input)
            {
                case "1a":
                case "1b":
                case "1c":
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
            _logger.LogInformation($"Smidas avslutad. Körtid för åtgärd: {stopwatch.Elapsed}");
        }
    }
}
