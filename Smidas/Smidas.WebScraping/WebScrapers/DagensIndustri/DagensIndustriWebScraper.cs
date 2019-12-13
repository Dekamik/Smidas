using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Smidas.Common;
using Smidas.Common.Extensions;
using Smidas.Common.StockIndices;
using Smidas.Core.Stocks;
using Smidas.WebScraping.Extensions;

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

            var htmlTask = new HtmlWeb().LoadFromWebAsync(url);
            htmlTask.Wait();
            html = htmlTask.Result;

            logger.LogDebug("Skrapar sidan...");

            IList<string> nameElements = null;
            IList<string> priceElements = null;
            IList<string> volumeElements = null;
            IList<string> profitPerStockElements = null;
            IList<string> adjustedEquityPerStockElements = null;
            IList<string> directYieldElements = null;

            Parallel.Invoke(
                () => nameElements = ScrapeNodes("Namn", "//div[contains(@class, 'js_Kurser')]/descendant::a[contains(@class, 'js_realtime__instrument-link')]"),
                () => priceElements = ScrapeNodes("Priser", "//tr[contains(@class, 'js_real-time-Kurser')]/descendant::span[contains(@class, 'di_stocks-table__last-price')]"),
                () => volumeElements = ScrapeNodes("Volymer", "//td[contains(@class, 'js_real-time__quantity')]"),
                () => profitPerStockElements = ScrapeNodes("Vinst/aktie", "//tr[contains(@class, 'js_real-time-Nyckeltal')]/td[4]"),
                () => adjustedEquityPerStockElements = ScrapeNodes("Justerat eget kapital/aktie", "//tr[contains(@class, 'js_real-time-Nyckeltal')]/td[5]"),
                () => directYieldElements = ScrapeNodes("Direktavkastning", "//tr[contains(@class, 'js_real-time-Nyckeltal')]/td[7]"));

            logger.LogDebug("Sida skrapad");

            logger.LogDebug("Verifierar hämtade element");

            // All lists must hold the same amount of elements
            if (new[] { nameElements, priceElements, volumeElements, profitPerStockElements, adjustedEquityPerStockElements, directYieldElements }
                .All(l => l.Count != nameElements.Count))
            {
                logger.LogError($"Elementlistorna har ej samma längd\n" +
                    $"Namn: {nameElements.Count} st, Priser: {priceElements.Count} st, Volymer: {volumeElements.Count} st, Vinst/aktie: {profitPerStockElements.Count} st, JEK/aktie: {adjustedEquityPerStockElements.Count}, Dir.avk: {directYieldElements.Count} st");

                throw new ValidationException($"Elementlistorna har ej samma längd\n" +
                    $"Namn: {nameElements.Count} st, Priser: {priceElements.Count} st, Volymer: {volumeElements.Count} st, Vinst/aktie: {profitPerStockElements.Count} st, JEK/aktie: {adjustedEquityPerStockElements.Count}, Dir.avk: {directYieldElements.Count} st");
            }

            logger.LogInformation("Element - OK");

            logger.LogDebug("Tolkar och organiserar data");

            List<string> names = nameElements.ToList();
            List<decimal> prices = null;
            List<decimal> volumes = null;
            List<decimal> profitPerStock = null;
            List<decimal> adjustedEquityPerStock = null;
            List<decimal> directYield = null;

            Parallel.Invoke(
                () => prices = Parse("Priser", priceElements).ToList(),
                () => volumes = Parse("Volymer", volumeElements).ToList(),
                () => profitPerStock = Parse("Vinst/aktie", profitPerStockElements).ToList(),
                () => adjustedEquityPerStock = Parse("Justerad eget kapital/aktie", adjustedEquityPerStockElements).ToList(),
                () => directYield = Parse("Direktavkastning", directYieldElements, hasSymbol: true).ToList());

            logger.LogDebug("Skapar modeller");

            var stocks = new List<Stock>();

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

        private IList<string> ScrapeNodes(string itemName, string xPath)
        {
            logger.LogDebug($"Skrapar {itemName.ToLower()}");

            var elements = html.DocumentNode.SelectNodes(xPath)
                                            .Select(n => WebUtility.HtmlDecode(n.InnerText))
                                            .ToList();

            logger.LogDebug($"{itemName} skrapade");

            return elements;
        }

        public IEnumerable<decimal> Parse(string itemName, IEnumerable<string> cells, bool hasSymbol = false)
        {
            logger.LogDebug($"Tar ut {itemName.ToLower()}");

            foreach (var cell in cells)
            {
                if (hasSymbol)
                {
                    yield return cell.ParseDecimalWithSymbol();
                }
                else
                {
                    yield return cell.ParseDecimal();
                }
            }

            logger.LogDebug($"{itemName} uttagna");
        }

        public void SetIndustries(ref List<Stock> stockData)
        {
            logger.LogDebug($"Tillsätter branscher");

            foreach (var industryData in industries)
            {
                if (industryData.Value.Companies != null)
                {
                    var industry = industryData.Key;
                    foreach (var companyName in industryData.Value.Companies)
                    {
                        stockData.Where(s => s.Name.Contains(companyName))
                                 .ForEach(s => s.Industry = industry);
                    }
                }
            }
        }
    }
}
