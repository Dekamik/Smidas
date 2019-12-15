using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Smidas.Common.StockIndices;
using Smidas.Core.Analysis;
using Smidas.Core.Stocks;
using Smidas.WebScraping.WebScrapers;
using Smidas.WebScraping.WebScrapers.DagensIndustri;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Smidas.Batch
{
    public class AktieReaJob
    {
        private readonly ILogger logger;
        private readonly IServiceProvider serviceProvider;
        private readonly IAnalysis analysis;

        public AktieReaJob(
            ILoggerFactory loggerFactory,
            IServiceProvider serviceProvider,
            AktieRea aktieRea)
        {
            logger = loggerFactory.CreateLogger<AktieReaJob>();
            this.serviceProvider = serviceProvider;
            analysis = aktieRea;
        }

        /// <summary>
        /// Runs analysis based on the specified input.
        /// 
        /// 1 = OMX Stockholm
        /// 2 = OMX Copenhagen
        /// 3 = OMX Helsinki
        /// 4 = Oslo OBX
        /// 5 = N/A
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public IEnumerable<Stock> Run(string input)
        {
            logger.LogInformation($"AktieReaJob påbörjad.");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            IWebScraper webScraper = null;

            // Select action
            switch (input)
            {
                case "1":
                    webScraper = serviceProvider.GetService<DagensIndustriWebScraper>();
                    (webScraper as DagensIndustriWebScraper).Index = StockIndex.OMXStockholmLargeCap;
                    (analysis as AktieRea).Index = StockIndex.OMXStockholmLargeCap;
                    break;

                case "2":
                    webScraper = serviceProvider.GetService<DagensIndustriWebScraper>();
                    (webScraper as DagensIndustriWebScraper).Index = StockIndex.OMXCopenhagenLargeCap;
                    (analysis as AktieRea).Index = StockIndex.OMXCopenhagenLargeCap;
                    break;

                case "3":
                    webScraper = serviceProvider.GetService<DagensIndustriWebScraper>();
                    (webScraper as DagensIndustriWebScraper).Index = StockIndex.OMXHelsinkiLargeCap;
                    (analysis as AktieRea).Index = StockIndex.OMXHelsinkiLargeCap;
                    break;

                case "4":
                    webScraper = serviceProvider.GetService<DagensIndustriWebScraper>();
                    (webScraper as DagensIndustriWebScraper).Index = StockIndex.OsloOBX;
                    (analysis as AktieRea).Index = StockIndex.OsloOBX;
                    break;

                case "5":
                    webScraper = serviceProvider.GetService<DagensIndustriWebScraper>();
                    (webScraper as DagensIndustriWebScraper).Index = StockIndex.Nasdaq100AndSnP100;
                    (analysis as AktieRea).Index = StockIndex.Nasdaq100AndSnP100;
                    break;

                default:
                    break;
            }

            // Run
            IList<Stock> stockData = webScraper?.Scrape();
            IEnumerable<Stock> results = analysis?.Analyze(stockData);

            stopwatch.Stop();
            logger.LogInformation($"AktieReaJob slutförd. Körtid: {stopwatch.Elapsed}");

            return results;
        }
    }
}
