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
        private readonly ILogger _logger;
        private readonly IWebScraper _webScraper;
        private readonly IAnalysis _analysis;

        public AktieReaJob(
            ILoggerFactory loggerFactory,
            IWebScraper webScraper,
            IAnalysis aktieRea)
        {
            _logger = loggerFactory.CreateLogger<AktieReaJob>();
            this._webScraper = webScraper;
            _analysis = aktieRea;
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
            _logger.LogInformation($"AktieReaJob påbörjad.");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // Run
            IList<Stock> stockData = await _webScraper.Scrape(query);
            IEnumerable<Stock> results = _analysis.Analyze(query, stockData);

            stopwatch.Stop();
            _logger.LogInformation($"AktieReaJob slutförd. Körtid: {stopwatch.Elapsed}");

            return results;
        }
    }
}
