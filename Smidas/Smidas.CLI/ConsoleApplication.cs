using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenQA.Selenium.Chrome;
using Smidas.Core.Analysis;
using Smidas.WebScraping;
using Smidas.WebScraping.AffarsVarlden;
using System;

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

        private readonly AppSettings _config;

        public ConsoleApplication(ILoggerFactory loggerFactory, IOptions<AppSettings> config)
        {
            _loggerFactory = loggerFactory;
            _config = config.Value;
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

            using var webDriver = new ChromeDriver(_config.ChromeDriverPath);
            IWebScraper webScraper = null;
            IAnalysis analysis = null;

            // Select webscraper
            switch (input)
            {
                case "1a":
                case "2a":
                    webScraper = new AffarsVarldenWebScraper(webDriver, _loggerFactory, AffarsVarldenIndexes.StockholmLargeCap);
                    break;

                case "1b":
                    webScraper = new AffarsVarldenWebScraper(webDriver, _loggerFactory, AffarsVarldenIndexes.CopenhagenLargeCap);
                    break;

                case "1c":
                    webScraper = new AffarsVarldenWebScraper(webDriver, _loggerFactory, AffarsVarldenIndexes.HelsinkiLargeCap);
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
                    analysis = new AktieRea(_loggerFactory);
                    break;

                default:
                    break;
            }

            // Run
            var stockData = webScraper?.Scrape();
            var results = analysis?.Analyze(stockData, _config.Blacklist);
        }
    }
}
