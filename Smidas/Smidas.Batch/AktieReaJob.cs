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
    public class AktieReaJob : IAktieReaJob
    {
        private readonly ILogger _logger;
        private readonly IWebScraper _webScraper;
        private readonly IAktieRea _aktieRea;

        public AktieReaJob(
            ILoggerFactory loggerFactory,
            IWebScraper webScraper,
            IAktieRea aktieRea)
        {
            _logger = loggerFactory.CreateLogger<AktieReaJob>();
            _webScraper = webScraper;
            _aktieRea = aktieRea;
        }
        
        public async Task<IEnumerable<Stock>> Run(AktieReaQuery query)
        {
            _logger.LogInformation($"AktieReaJob start.");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // Run
            IList<Stock> stockData = await _webScraper.Scrape(query);
            IEnumerable<Stock> results = _aktieRea.Analyze(query, stockData);

            stopwatch.Stop();
            _logger.LogInformation($"AktieReaJob end. Runtime: {stopwatch.ElapsedMilliseconds}ms");

            return results;
        }
    }
}
