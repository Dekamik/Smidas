using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        // TODO: Ändra kolumner
        // Share prices page
        private const int _nameCol = 0;

        private const int _priceCol = 0;
        private const int _volumeCol = 5;

        // Stock indicators page
        private const int _profitPerStockCol = 3;
        private const int _adjustedEquityPerStockCol = 4;
        private const int _directYieldCol = 6;

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

        public override IEnumerable<Stock> Scrape()
        {
            _logger.LogInformation($"Skrapar di.se");

            var stockData = new List<Stock>();

            NavigateTo(_url);

            ScrapeNames(ref stockData);

            ScrapeSharePrices(ref stockData);

            Wait();

            ClickKpiButton();

            ScrapeKpis(ref stockData);

            Wait();

            SetIndustries(ref stockData);

            _logger.LogInformation($"Skrapning slutförd");
            return stockData;
        }

        public void ClickKpiButton()
        {
            _logger.LogInformation("Klickar in på nyckeltalsfliken");
            _webDriver.ExecuteScript("arguments[0].click();", _webDriver.FindElement(By.XPath("//a[text()='Nyckeltal']")));
        }

        public void ScrapeNames(ref List<Stock> stockData)
        {
            _logger.LogInformation("Skrapar namn");

            var names = _webDriver.FindElements(By.XPath("//table[contains(@class, 'fixed-table')]/descendant::td/a")).Select(e => e.Text);

            foreach (var name in names)
            {
                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }

                _logger.LogTrace($"{name}");
                stockData.Add(new Stock { Name = name });
            }
        }

        public void ScrapeSharePrices(ref List<Stock> stockData)
        {
            _logger.LogInformation($"Skrapar aktiekurser");

            var rows = _webDriver.FindElements(By.XPath("//table[contains(@class, 'scrolling-table')]/descendant::tbody/tr"));
            var i = 0;
            
            foreach (var row in rows)
            {
                if (string.IsNullOrEmpty(row.Text))
                {
                    continue;
                }

                var cells = row.FindElements(By.TagName("td"));
                var price = cells[_priceCol].DecimalTextAsDecimal();
                var volume = cells[_volumeCol].DecimalTextAsDecimal();

                _logger.LogTrace($"Pris = {price}, Volym = {volume}");

                stockData[i].Price = price;
                stockData[i].Volume = volume;

                i++;
            }
        }

        public void ScrapeKpis(ref List<Stock> stockData)
        {
            _logger.LogInformation($"Skrapar nyckeltal");

            var rows = _webDriver.FindElements(By.XPath("//table[contains(@class, 'scrolling-table')]/descendant::tbody/tr"));
            var i = 0;

            foreach (var row in rows)
            {
                if (string.IsNullOrEmpty(row.Text))
                {
                    continue;
                }

                var cells = row.FindElements(By.TagName("td"));
                var profitPerStock = cells[_profitPerStockCol].DecimalTextAsDecimal();
                var adjustedEquityPerStock = cells[_adjustedEquityPerStockCol].DecimalTextAsDecimal();
                var directYield = cells[_directYieldCol].PercentageTextAsDecimal();

                _logger.LogTrace($"Vinst/aktie = {profitPerStock}, JEK/aktie = {adjustedEquityPerStock}, Dir.avk. = {directYield}");

                stockData[i].ProfitPerStock = profitPerStock;
                stockData[i].AdjustedEquityPerStock = adjustedEquityPerStock;
                stockData[i].DirectYield = directYield;

                i++;
            }
        }

        public void SetIndustries(ref List<Stock> stockData)
        {
            _logger.LogDebug($"Tillsätter branscher");

            foreach (var industryData in _industries)
            {
                if (industryData.Value.Companies != null)
                {
                    var industry = Enum.Parse<Industry>(industryData.Key);
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
