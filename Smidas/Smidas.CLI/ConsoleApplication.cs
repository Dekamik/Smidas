using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OfficeOpenXml;
using Smidas.Batch;
using Smidas.Core.Stocks;
using Smidas.Exporting.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Smidas.CLI.AppSettings;

namespace Smidas.CLI
{
    public class ConsoleApplication
    {
        private readonly string _menu = @"
   Smidas

-----------------------------------------------------
   AktieREA-analyser (Dagens Industri)

   Nordeuropa
1: OMX Stockholm Large Cap
2: OMX Köpenhamn Large Cap
3: OMX Helsingfors Large Cap
4: Oslo OBX

   Nordamerika
5: S&P 500 (Nasdaq & NYSE)

-----------------------------------------------------
   Övriga åtgärder

0: Avsluta

-----------------------------------------------------";

        private readonly ILogger _logger;
        
        private readonly IOptions<AppSettings> _options;
        
        private readonly AktieReaJob _aktieReaJob;

        private readonly ExcelExporter _excelExporter;

        public ConsoleApplication(
            ILoggerFactory loggerFactory,
            IOptions<AppSettings> options,
            AktieReaJob aktieReaJob,
            ExcelExporter excelExporter)
        {
            _logger = loggerFactory.CreateLogger<ConsoleApplication>();
            _options = options;
            _aktieReaJob = aktieReaJob;
            _excelExporter = excelExporter;
        }

        public void Run()
        {
            string input;
            string exportPath = null;
            AktieReaLocalQuery query = null;

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

            switch (input)
            {
                case "1":
                    query = _options.Value.AktieRea["OMXStockholmLargeCap"];
                    exportPath = Path.Combine(query.ExportDirectory ?? _options.Value.DefaultExportDirectory, $"AktieREA_OMX_Stockholm_Large_Cap_{DateTime.Now.ToString("yyyy-MM-dd_HHmm")}.xlsx");
                    break;

                case "2":
                    query = _options.Value.AktieRea["OMXCopenhagenLargeCap"];
                    exportPath = Path.Combine(query.ExportDirectory ?? _options.Value.DefaultExportDirectory, $"AktieREA_OMX_Köpenhamn_Large_Cap_{DateTime.Now.ToString("yyyy-MM-dd_HHmm")}.xlsx");
                    break;

                case "3":
                    query = _options.Value.AktieRea["OMXHelsinkiLargeCap"];
                    exportPath = Path.Combine(query.ExportDirectory ?? _options.Value.DefaultExportDirectory, $"AktieREA_OMX_Helsingfors_Large_Cap_{DateTime.Now.ToString("yyyy-MM-dd_HHmm")}.xlsx");
                    break;

                case "4":
                    query = _options.Value.AktieRea["OsloOBX"];
                    exportPath = Path.Combine(query.ExportDirectory ?? _options.Value.DefaultExportDirectory, $"AktieREA_Oslo_OBX_{DateTime.Now.ToString("yyyy-MM-dd_HHmm")}.xlsx");
                    break;

                default:
                    break;
            }

            Task<IEnumerable<Stock>> task = _aktieReaJob.Run(query);
            task.Wait();
            IEnumerable<Stock> results = task.Result;

            if (!string.IsNullOrEmpty(exportPath))
            {
                _logger.LogInformation($"Exporterar analys om {results.Count()} aktier till {exportPath}");

                using ExcelPackage excel = new ExcelPackage(new FileInfo(exportPath));
                ExcelWorksheet worksheet = excel.Workbook.Worksheets.Add($"AktieREA {DateTime.Today.ToString("yyyy-MM-dd")}");

                _excelExporter.ExportStocksToWorksheet(ref worksheet, results.ToList(), query.CurrencyCode);

                excel.Save();

                _logger.LogDebug("Exportering slutförd");
            }
        }
    }
}
