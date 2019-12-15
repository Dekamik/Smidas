using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Smidas.Common;
using Smidas.Common.Extensions;
using Smidas.Common.StockIndices;
using Smidas.Core.Stocks;
using Smidas.WebScraping.WebScrapers.Parsing;

namespace Smidas.WebScraping.WebScrapers.DagensIndustri
{
    public class DagensIndustriWebScraper : IWebScraper
    {
        private readonly ILogger logger;
        private readonly IOptions<AppSettings> options;

        private IDictionary<string, AppSettings.IndexSettings.IndustryData> industries;
        private string url;
        private StockIndex index;

        private HtmlDocument html;

        public StockIndex Index
        {
            get => index;
            set
            {
                index = value;

                url = index.GetDagensIndustriInfo().Url;
                industries = options.Value.AktieRea[index.ToString()].Industries;
            }
        }

        public DagensIndustriWebScraper(ILoggerFactory loggerFactory, IOptions<AppSettings> options)
        {
            logger = loggerFactory.CreateLogger<DagensIndustriWebScraper>();
            this.options = options;
        }

        public IList<Stock> Scrape()
        {
            logger.LogInformation($"Skrapar {url}");

            Task<HtmlDocument> htmlTask = new HtmlWeb().LoadFromWebAsync(url);
            htmlTask.Wait();
            html = htmlTask.Result;

            List<string> names = null;
            List<decimal> prices = null;
            List<decimal> volumes = null;
            List<decimal> profitPerStock = null;
            List<decimal> adjustedEquityPerStock = null;
            List<decimal> directYield = null;

            Parallel.Invoke(
                () => names = ScrapeNodes("//div[contains(@class, 'js_Kurser')]/descendant::a[contains(@class, 'js_realtime__instrument-link')]")
                                    .ToList(),

                () => prices = ScrapeNodes("//tr[contains(@class, 'js_real-time-Kurser')]/descendant::span[contains(@class, 'di_stocks-table__last-price')]")
                                    .Parse()
                                    .ToList(),

                () => volumes = ScrapeNodes("//td[contains(@class, 'js_real-time__quantity')]")
                                    .Parse()
                                    .ToList(),

                () => profitPerStock = ScrapeNodes("//tr[contains(@class, 'js_real-time-Nyckeltal')]/td[4]")
                                            .Parse()
                                            .ToList(),

                () => adjustedEquityPerStock = ScrapeNodes("//tr[contains(@class, 'js_real-time-Nyckeltal')]/td[5]")
                                                    .Parse()
                                                    .ToList(),

                () => directYield = ScrapeNodes("//tr[contains(@class, 'js_real-time-Nyckeltal')]/td[7]")
                                        .Parse(hasSymbol: true)
                                        .ToList());

            // All lists must hold the same amount of elements
            if (new[] { prices, volumes, profitPerStock, adjustedEquityPerStock, directYield }
                .All(l => l.Count() != names.Count()))
            {
                logger.LogError($"Elementlistorna har ej samma längd\n" +
                    $"Namn: {names.Count()} st, Priser: {prices.Count()} st, Volymer: {volumes.Count()} st, Vinst/aktie: {profitPerStock.Count()} st, JEK/aktie: {adjustedEquityPerStock.Count()}, Dir.avk: {directYield.Count()} st");

                throw new ValidationException($"Elementlistorna har ej samma längd\n" +
                    $"Namn: {names.Count()} st, Priser: {prices.Count()} st, Volymer: {volumes.Count()} st, Vinst/aktie: {profitPerStock.Count()} st, JEK/aktie: {adjustedEquityPerStock.Count()}, Dir.avk: {directYield.Count()} st");
            }

            logger.LogInformation("Element - OK");

            List<Stock> stocks = new List<Stock>();

            for (int i = 0; i < names.Count(); i++)
            {
                var stock = new Stock
                {
                    Name = names[i],
                    Price = prices[i],
                    Volume = volumes[i],
                    ProfitPerStock = profitPerStock[i],
                    AdjustedEquityPerStock = adjustedEquityPerStock[i],
                    DirectYield = directYield[i],
                };
                stocks.Add(stock);
            }

            SetIndustries(ref stocks);

            logger.LogInformation("Skrapning slutförd");
            return stocks;
        }

        private IEnumerable<string> ScrapeNodes(string xPath) => html.DocumentNode.SelectNodes(xPath)
                                                                                  .Select(n => WebUtility.HtmlDecode(n.InnerText));

        public void SetIndustries(ref List<Stock> stockData)
        {
            foreach (KeyValuePair<string, AppSettings.IndexSettings.IndustryData> industryData in industries)
            {
                if (industryData.Value.Companies != null)
                {
                    string industry = industryData.Key;
                    foreach (string companyName in industryData.Value.Companies)
                    {
                        stockData.Where(s => s.Name.Contains(companyName))
                                 .ForEach(s => s.Industry = industry);
                    }
                }
            }
        }
    }
}
