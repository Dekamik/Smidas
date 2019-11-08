using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Smidas.CLI.Jobs;
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
        private readonly ILoggerFactory loggerFactory;

        private readonly ILogger _logger;

        public ConsoleApplication(ILoggerFactory loggerFactory, WebScrapingJob webScrapingJob)
        {
            _logger = loggerFactory.CreateLogger<ConsoleApplication>();
        }

        public void Run()
        {
            Console.WriteLine("   Smidas ver. 20191108");
            Console.WriteLine("-----------------------------------------");
            Console.WriteLine("1. AktieREA - Stockholm Large Cap");
            Console.WriteLine("2. AktieREA - Köpenhamn Large Cap (N/A)");
            Console.WriteLine("3. AktieREA - Helsingfors Large Cap (N/A)");
            Console.WriteLine();
            Console.WriteLine("0. Exit");
            Console.WriteLine("-----------------------------------------");
            Console.Write(">> ");
            var input = Console.ReadLine();

            Console.WriteLine();

            if (input == "0")
            {
                Console.WriteLine("Exiting");
                Environment.Exit(0);
            }

            IWebScraper webScraper = null;
            IAnalysis analysis = null;

            // Select webscraper
            switch (input)
            {
                case "1":
                    webScraper = new AffarsVarldenWebScraper(new ChromeDriver(), loggerFactory, AffarsVarldenIndexes.StockholmLargeCap);
                    break;

                case "2":
                    webScraper = new AffarsVarldenWebScraper(new ChromeDriver(), loggerFactory, AffarsVarldenIndexes.CopenhagenLargeCap);
                    break;

                case "3":
                    webScraper = new AffarsVarldenWebScraper(new ChromeDriver(), loggerFactory, AffarsVarldenIndexes.HelsinkiLargeCap);
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
                    analysis = new AktieRea(loggerFactory);
                    break;

                default:
                    break;
            }

            // Run
            var stockData = webScraper.Scrape();
            var results = analysis.Analyze(stockData, null);
        }
    }
}
