using Microsoft.Extensions.Logging;
using Smidas.Common;
using Smidas.Core.Analysis;
using Smidas.Core.Stocks;
using Smidas.WebScraping.WebScrapers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Smidas.Batch
{
    public class AktieReaJob
    {
        private readonly ILogger logger;
        private readonly IWebScraper webScraper;
        private readonly IAnalysis analysis;

        public AktieReaJob(
            ILoggerFactory loggerFactory,
            IWebScraper webScraper,
            IAnalysis aktieRea)
        {
            logger = loggerFactory.CreateLogger<AktieReaJob>();
            this.webScraper = webScraper;
            analysis = aktieRea;
        }

        /// <summary>
        /// Runs analysis based on the specified input.
        /// 
        /// 1 = OMX Stockholm
        /// 2 = OMX Copenhagen
        /// 3 = OMX Helsinki
        /// 4 = Oslo OBX
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public async Task<IEnumerable<Stock>> Run(AktieReaQuery query)
        {
            logger.LogInformation($"AktieReaJob påbörjad.");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // Run
            IList<Stock> stockData = await webScraper.Scrape(query);
            IEnumerable<Stock> results = analysis.Analyze(query, stockData);

            stopwatch.Stop();
            logger.LogInformation($"AktieReaJob slutförd. Körtid: {stopwatch.Elapsed}");

            return results;
        }
    }
}
