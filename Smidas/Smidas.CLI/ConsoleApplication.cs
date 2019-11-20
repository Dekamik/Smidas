using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Smidas.Common;
using Smidas.Common.StockIndices;
using Smidas.Core.Analysis;
using Smidas.Exporting.Excel;
using Smidas.WebScraping.WebScrapers;
using Smidas.WebScraping.WebScrapers.AffarsVarlden;
using Smidas.WebScraping.WebScrapers.DagensIndustri;
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
   AktieREA-analyser (Affärsvärlden)

1: OMX Stockholm Large Cap

-----------------------------------------------------
   AktieREA-analyser (Dagens Industri)

   Nordeuropa
2: OMX Stockholm Large Cap
3: OMX Köpenhamn Large Cap
4: OMX Helsingfors Large Cap
5: Oslo OBX

   Nordamerika
6: S&P 500 (Nasdaq & NYSE)

-----------------------------------------------------
   Övriga åtgärder

0: Avsluta

-----------------------------------------------------";

        private readonly ILoggerFactory _loggerFactory;

        private readonly ILogger _logger;

        private readonly IOptions<AppSettings> _options;

        private readonly IServiceProvider _serviceProvider;

        private readonly IAnalysis _analysis;

        private readonly ExcelExporter _excelExporter;

        public ConsoleApplication(
            ILoggerFactory loggerFactory, 
            IOptions<AppSettings> options, 
            IServiceProvider serviceProvider,
            AktieRea aktieRea, 
            ExcelExporter excelExporter)
        {
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger<ConsoleApplication>();
            _options = options;
            _serviceProvider = serviceProvider;
            _analysis = aktieRea;
            _excelExporter = excelExporter;
        }

        public void Run()
        {
            string input = null;

            if (Environment.UserInteractive)
            {
                Console.WriteLine(_menu);
                Console.Write(">> ");
                input = Console.ReadLine();

                Console.WriteLine();

                if (input == "0")
                {
                    Console.WriteLine("Avslutar");
                    Environment.Exit(0);
                }
            }
            else
            {
                input = Environment.GetCommandLineArgs()[1];

                if (input == "h" || input == "help")
                {
                    Console.WriteLine(_menu);
                    Environment.Exit(0);
                }
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            
            string exportPath = null;
            IWebScraper webScraper = null;
            string currency = null;

            // Select action
            switch (input)
            {
                case "1":
                    webScraper = _serviceProvider.GetService<AffarsVarldenWebScraper>();
                    (webScraper as AffarsVarldenWebScraper).Index = StockIndex.OmxStockholmLargeCap;
                    (_analysis as AktieRea).Index = StockIndex.OmxStockholmLargeCap;
                    currency = _options.Value.AktieRea[StockIndex.OmxStockholmLargeCap.ToString()].CurrencyCode;
                    exportPath = (_options.Value.AktieRea[StockIndex.OmxStockholmLargeCap.ToString()].ExportDirectory ?? _options.Value.DefaultExportDirectory) + $"\\AktieREA_OMX_Stockholm_Large_Cap_{DateTime.Now.ToString("yyyy-MM-dd_HHmm")}.xlsx";
                    break;

                case "2":
                    webScraper = _serviceProvider.GetService<DagensIndustriWebScraper>();
                    (webScraper as DagensIndustriWebScraper).Index = StockIndex.OmxStockholmLargeCap;
                    (_analysis as AktieRea).Index = StockIndex.OmxStockholmLargeCap;
                    currency = _options.Value.AktieRea[StockIndex.OmxStockholmLargeCap.ToString()].CurrencyCode;
                    exportPath = (_options.Value.AktieRea[StockIndex.OmxStockholmLargeCap.ToString()].ExportDirectory ?? _options.Value.DefaultExportDirectory) + $"\\AktieREA_OMX_Stockholm_Large_Cap_{DateTime.Now.ToString("yyyy-MM-dd_HHmm")}.xlsx";
                    break;

                case "3":
                    webScraper = _serviceProvider.GetService<DagensIndustriWebScraper>();
                    (webScraper as DagensIndustriWebScraper).Index = StockIndex.OmxCopenhagenLargeCap;
                    (_analysis as AktieRea).Index = StockIndex.OmxCopenhagenLargeCap;
                    currency = _options.Value.AktieRea[StockIndex.OmxCopenhagenLargeCap.ToString()].CurrencyCode;
                    exportPath = (_options.Value.AktieRea[StockIndex.OmxCopenhagenLargeCap.ToString()].ExportDirectory ?? _options.Value.DefaultExportDirectory) + $"\\AktieREA_OMX_Köpenhamn_Large_Cap_{DateTime.Now.ToString("yyyy-MM-dd_HHmm")}.xlsx";
                    break;

                case "4":
                    webScraper = _serviceProvider.GetService<DagensIndustriWebScraper>();
                    (webScraper as DagensIndustriWebScraper).Index = StockIndex.OmxHelsinkiLargeCap;
                    (_analysis as AktieRea).Index = StockIndex.OmxHelsinkiLargeCap;
                    currency = _options.Value.AktieRea[StockIndex.OmxHelsinkiLargeCap.ToString()].CurrencyCode;
                    exportPath = (_options.Value.AktieRea[StockIndex.OmxHelsinkiLargeCap.ToString()].ExportDirectory ?? _options.Value.DefaultExportDirectory) + $"\\AktieREA_OMX_Helsingfors_Large_Cap_{DateTime.Now.ToString("yyyy-MM-dd_HHmm")}.xlsx";
                    break;

                case "5":
                    webScraper = _serviceProvider.GetService<DagensIndustriWebScraper>();
                    (webScraper as DagensIndustriWebScraper).Index = StockIndex.OsloObx;
                    (_analysis as AktieRea).Index = StockIndex.OsloObx;
                    currency = _options.Value.AktieRea[StockIndex.OsloObx.ToString()].CurrencyCode;
                    exportPath = (_options.Value.AktieRea[StockIndex.OsloObx.ToString()].ExportDirectory ?? _options.Value.DefaultExportDirectory) + $"\\AktieREA_Oslo_OBX_{DateTime.Now.ToString("yyyy-MM-dd_HHmm")}.xlsx";
                    break;

                case "6":
                    webScraper = _serviceProvider.GetService<DagensIndustriWebScraper>();
                    (webScraper as DagensIndustriWebScraper).Index = StockIndex.SNP500;
                    (_analysis as AktieRea).Index = StockIndex.SNP500;
                    currency = _options.Value.AktieRea[StockIndex.SNP500.ToString()].CurrencyCode;
                    exportPath = (_options.Value.AktieRea[StockIndex.SNP500.ToString()].ExportDirectory ?? _options.Value.DefaultExportDirectory) + $"\\AktieREA_S&P_500_{DateTime.Now.ToString("yyyy-MM-dd_HHmm")}.xlsx";
                    break;

                default:
                    break;
            }

            // Run
            var stockData = webScraper?.Scrape();
            var results = _analysis?.Analyze(stockData);

            if (!string.IsNullOrEmpty(exportPath))
            {
                _excelExporter.Export(results.ToList(), exportPath, currency);
            }

            stopwatch.Stop();
            _logger.LogInformation($"Smidas avslutad. Körtid för utvald åtgärd: {stopwatch.Elapsed}");
        }
    }
}
