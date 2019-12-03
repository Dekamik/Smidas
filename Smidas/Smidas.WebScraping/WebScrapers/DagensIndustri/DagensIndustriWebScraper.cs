using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using Smidas.Common;
using Smidas.Common.Extensions;
using Smidas.Common.StockIndices;
using Smidas.Core.Stocks;
using Smidas.WebScraping.Extensions;
using Smidas.WebScraping.WebDriver;

namespace Smidas.WebScraping.WebScrapers.DagensIndustri
{
    public class DagensIndustriWebScraper : WebScraperService<DagensIndustriWebScraper>
    {
        private IDictionary<string, AppSettings.IndexSettings.IndustryData> _industries;

        private string _url;

        private StockIndex _index;

        public StockIndex Index
        {
            get => _index;
            set
            {
                _index = value;

                _url = _index.GetDagensIndustriInfo().Url;
                _industries = _config.Value.AktieRea[_index.ToString()].Industries;
            }
        }

        public DagensIndustriWebScraper(IWebDriverFactory webDriverFactory, ILoggerFactory loggerFactory, IOptions<AppSettings> config) : base(webDriverFactory, loggerFactory, config)
        {
        }

        public override IList<Stock> Scrape()
        {
            _logger.LogInformation($"Skrapar di.se");

            NavigateTo(_url);

            // Get stock prices
            _logger.LogInformation("Hämtar kurser");

            IList<string> nameElements = null;
            IList<string> priceElements = null;
            IList<string> volumeElements = null;

            Parallel.Invoke(
                () => nameElements = ScrapeNames(),
                () => priceElements = ScrapePrices(),
                () => volumeElements = ScrapeVolumes());

            _logger.LogInformation("Kurser hämtade");

            Wait();

            // Get KPIs
            ClickKpiButton();

            _logger.LogInformation("Hämtar nyckeltal");

            IList<string> profitPerStockElements = null;
            IList<string> adjustedEquityPerStockElement = null;
            IList<string> directYieldElements = null;

            Parallel.Invoke(
                () => profitPerStockElements = ScrapeProfitPerStock(),
                () => adjustedEquityPerStockElement = ScrapeAdjustedEquityPerStock(),
                () => directYieldElements = ScrapeDirectYield());

            _logger.LogInformation("Nyckeltal hämtade");

            _logger.LogInformation("Verifierar hämtade element");

            // All lists must hold the same amount of elements
            if (new[] { nameElements, priceElements, volumeElements, profitPerStockElements, adjustedEquityPerStockElement, directYieldElements }
                .All(l => l.Count != nameElements.Count))
            {
                _logger.LogError($"Elementlistorna har ej samma längd\n" +
                    $"Namn: {nameElements.Count} st, Priser: {priceElements.Count} st, Volymer: {volumeElements.Count} st, Vinst/aktie: {profitPerStockElements.Count} st, JEK/aktie: {adjustedEquityPerStockElement.Count}, Dir.avk: {directYieldElements.Count} st");

                throw new ValidationException($"Elementlistorna har ej samma längd\n" +
                    $"Namn: {nameElements.Count} st, Priser: {priceElements.Count} st, Volymer: {volumeElements.Count} st, Vinst/aktie: {profitPerStockElements.Count} st, JEK/aktie: {adjustedEquityPerStockElement.Count}, Dir.avk: {directYieldElements.Count} st");
            }

            _logger.LogInformation("Element - OK");

            _logger.LogDebug("Stänger ned Selenium");

            // Close web driver to free up resources
            _webDriver.Close();

            _logger.LogInformation("Tolkar och organiserar data");

            // Parse all values from the raw strings
            _logger.LogDebug("Tar ut alla värden");

            List<string> names = null;
            List<decimal> prices = null;
            List<decimal> volumes = null;
            List<decimal> profitPerStock = null;
            List<decimal> adjustedEquityPerStock = null;
            List<decimal> directYield = null;

            Parallel.Invoke(
                () => names = ParseNames(nameElements).ToList(),
                () => prices = ParsePrices(priceElements).ToList(),
                () => volumes = ParseVolumes(volumeElements).ToList(),
                () => profitPerStock = ParseProfitPerStock(profitPerStockElements).ToList(),
                () => adjustedEquityPerStock = ParseAdjustedEquityPerStock(adjustedEquityPerStockElement).ToList(),
                () => directYield = ParseDirectYield(directYieldElements).ToList());

            _logger.LogDebug("Alla värden uttagna");

            // Build all stock models from the scraped data
            _logger.LogDebug("Skapar modeller");

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

            _logger.LogDebug("Modeller skapade");

            SetIndustries(ref stocks);

            _logger.LogInformation($"Skrapning slutförd");
            return stocks;
        }

        public void ClickKpiButton()
        {
            _logger.LogInformation("Klickar in på nyckeltalsfliken");
            _webDriver.ExecuteScript("arguments[0].click();", _webDriver.FindElement(By.XPath("//a[text()='Nyckeltal']")));
        }

        private IList<string> ScrapeNames()
        {
            _logger.LogDebug("Skrapar namn");

            var elements = _webDriver.FindElements(By.XPath("//div[contains(@class, 'js_Kurser')]/descendant::a[contains(@class, 'js_realtime__instrument-link')]"))
                                     .Select(e => e.Text)
                                     .ToList();

            _logger.LogDebug("Namn skrapade");

            return elements;
        }

        private IList<string> ScrapePrices()
        {
            _logger.LogDebug("Skrapar priser");

            var elements = _webDriver.FindElements(By.XPath("//tr[contains(@class, 'js_real-time-Kurser')]/descendant::span[contains(@class, 'di_stocks-table__last-price')]"))
                                     .Select(e => e.Text)
                                     .ToList();

            _logger.LogDebug("Priser skrapade");

            return elements;
        }

        private IList<string> ScrapeVolumes()
        {
            _logger.LogDebug("Skrapar volymer");

            var elements = _webDriver.FindElements(By.XPath("//td[contains(@class, 'js_real-time__quantity')]"))
                                     .Select(e => e.Text)
                                     .ToList();

            _logger.LogDebug("Volymer skrapade");

            return elements;
        }

        private IList<string> ScrapeProfitPerStock()
        {
            _logger.LogDebug("Skrapar vinst/aktie");

            var elements = _webDriver.FindElements(By.XPath("//tr[contains(@class, 'js_real-time-Nyckeltal')]/td[4]"))
                                     .Select(e => e.Text)
                                     .ToList();

            _logger.LogDebug("Vinst/aktie skrapad");

            return elements;
        }

        private IList<string> ScrapeAdjustedEquityPerStock()
        {
            _logger.LogDebug("Skrapar JEK/aktie");

            var elements = _webDriver.FindElements(By.XPath("//tr[contains(@class, 'js_real-time-Nyckeltal')]/td[5]"))
                                     .Select(e => e.Text)
                                     .ToList();

            _logger.LogDebug("JEK/aktie skrapad");

            return elements;
        }

        private IList<string> ScrapeDirectYield()
        {
            _logger.LogDebug("Skrapar direktavkastningar");

            var elements = _webDriver.FindElements(By.XPath("//tr[contains(@class, 'js_real-time-Nyckeltal')]/td[7]"))
                                     .Select(e => e.Text)
                                     .ToList();

            _logger.LogDebug("Direktavkastningar skrapade");

            return elements;
        }

        public IEnumerable<string> ParseNames(IEnumerable<string> cells)
        {
            _logger.LogDebug("Tar ut namn");

            foreach (var cell in cells)
            {
                yield return cell;
                _logger.LogTrace($"Namn = {cell}");
            }

            _logger.LogDebug("Namn uttagna");
        }

        public IEnumerable<decimal> ParsePrices(IEnumerable<string> cells)
        {
            _logger.LogDebug("Tar ut priser");

            foreach (var cell in cells)
            {
                yield return cell.ParseDecimal();
                _logger.LogTrace($"Pris = {cell}");
            }

            _logger.LogDebug("Priser uttagna");
        }

        public IEnumerable<decimal> ParseVolumes(IEnumerable<string> cells)
        {
            _logger.LogDebug("Tar ut volymer");

            foreach (var cell in cells)
            {
                yield return cell.ParseDecimal();
                _logger.LogTrace($"Volym = {cell}");
            }

            _logger.LogDebug("Volymer uttagna");
        }

        private IEnumerable<decimal> ParseProfitPerStock(IEnumerable<string> cells)
        {
            _logger.LogDebug("Tar ut Vinst/aktie");

            foreach (var cell in cells)
            {
                yield return cell.ParseDecimal();
                _logger.LogTrace($"Vinst/aktie = {cell}");
            }

            _logger.LogDebug("Vinst/aktie uttagen");
        }

        private IEnumerable<decimal> ParseAdjustedEquityPerStock(IEnumerable<string> cells)
        {
            _logger.LogDebug("Tar ut JEK/aktie");

            foreach (var cell in cells)
            {
                yield return cell.ParseDecimal();
                _logger.LogTrace($"JEK/aktie = {cell}");
            }

            _logger.LogDebug("JEK/aktie uttagen");
        }

        private IEnumerable<decimal> ParseDirectYield(IEnumerable<string> cells)
        {
            _logger.LogDebug("Tar ut direktavkastning");

            foreach (var cell in cells)
            {
                yield return cell.ParseDecimalWithSymbol();
                _logger.LogTrace($"JEK/aktie = {cell}");
            }

            _logger.LogDebug("Direktavkastningar uttagna");
        }

        public void SetIndustries(ref List<Stock> stockData)
        {
            _logger.LogDebug($"Tillsätter branscher");

            foreach (var industryData in _industries)
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
