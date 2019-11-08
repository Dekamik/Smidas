using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Smidas.Core.Analysis;
using Smidas.Core.Stocks;
using Smidas.WebScraping;
using Smidas.WebScraping.AffarsVarlden;
using System;
using System.Collections.Generic;
using System.Text;

namespace Smidas.CLI
{
    public class ConsoleApplication
    {
        private readonly ILoggerFactory _loggerFactory;

        private readonly AppSettings _config;

        public ConsoleApplication(ILoggerFactory loggerFactory, IOptions<AppSettings> config)
        {
            _loggerFactory = loggerFactory;
            _config = config.Value;
        }

        public void Run()
        {
            var menu = @"
   Smidas ver. 20191108

---------------------------------------------
   AktieREA-analyser

1: AktieREA - OMX Stockholm Large Cap
2: AktieREA - OMX Köpenhamn Large Cap (N/A)
3: AktieREA - OMX Helsingfors Large Cap (N/A)

---------------------------------------------
   Övriga åtgärder

0: Avsluta

---------------------------------------------";
            Console.WriteLine(menu);
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
                case "1":
                    webScraper = new AffarsVarldenWebScraper(webDriver, _loggerFactory, AffarsVarldenIndexes.StockholmLargeCap);
                    break;

                case "2":
                    webScraper = new AffarsVarldenWebScraper(webDriver, _loggerFactory, AffarsVarldenIndexes.CopenhagenLargeCap);
                    break;

                case "3":
                    webScraper = new AffarsVarldenWebScraper(webDriver, _loggerFactory, AffarsVarldenIndexes.HelsinkiLargeCap);
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
                    analysis = new AktieRea(_loggerFactory);
                    break;

                default:
                    break;
            }

            // Run
            var stockData = webScraper.Scrape();
            var results = analysis.Analyze(stockData, _config.Blacklist);
        }
    }
}
