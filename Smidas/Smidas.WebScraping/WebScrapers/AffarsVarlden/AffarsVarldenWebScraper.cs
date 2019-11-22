using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using Smidas.Common;
using Smidas.Common.Extensions;
using Smidas.Common.StockIndices;
using Smidas.Core.Stocks;
using Smidas.WebScraping.Extensions;
using Smidas.WebScraping.WebDriver;
using System.Collections.Generic;
using System.Linq;

namespace Smidas.WebScraping.WebScrapers.AffarsVarlden
{
    public class AffarsVarldenWebScraper : WebScraperService<AffarsVarldenWebScraper>
    {
        // Share prices page
        private const int _nameCol = 0;
        private const int _priceCol = 6;
        private const int _turnoverCol = 10;

        // Stock indicators page
        private const int _adjustedEquityPerStockCol = 3;
        private const int _directYieldCol = 6;
        private const int _profitPerStockCol = 7;

        private IDictionary<string, AppSettings.IndexSettings.IndustryData> _industries;

        private string _stockIndexUrl;

        private string _stockIndicatorsUrl;

        private StockIndex _index;

        public StockIndex Index
        {
            get => _index;
            set
            {
                _index = value;

                var info = _index.GetAffarsVarldenInfo();
                _stockIndexUrl = info.StockIndexUrl;
                _stockIndicatorsUrl = info.StockIndicatorsUrl;

                _industries = _config.Value.AktieRea[_index.ToString()].Industries;
            }
        }

        public AffarsVarldenWebScraper(IWebDriverFactory webDriverFactory, ILoggerFactory loggerFactory, IOptions<AppSettings> config) : base(webDriverFactory, loggerFactory, config)
        {
        }

        public override IEnumerable<Stock> Scrape()
        {
            _logger.LogInformation($"Skrapar affärsvärlden.se");
            var stockData = new List<Stock>();

            NavigateTo(_stockIndexUrl);

            Wait();

            ScrapeSharePrices(ref stockData);

            // Pretty hacky, should support pagination instead
            if (Index == StockIndex.OMXStockholmLargeCap)
            {
                ClickNextButton();

                Wait();

                ScrapeSharePrices(ref stockData);
            }

            NavigateTo(_stockIndicatorsUrl);

            Wait();

            ScrapeStockIndicators(ref stockData);

            if (Index == StockIndex.OMXStockholmLargeCap)
            {
                ClickNextButton();

                Wait();

                ScrapeStockIndicators(ref stockData);
            }

            SetIndustries(ref stockData);

            _logger.LogInformation($"Skrapning slutförd");
            return stockData.AsEnumerable();
        }

        public void ClickNextButton()
        {
            _logger.LogInformation($"Klickar till nästa sida");
            _webDriver.ExecuteScript("arguments[0].click();", _webDriver.FindElement(By.XPath("//a[text()='>']")));
        }

        public void ScrapeSharePrices(ref List<Stock> stockData)
        {
            _logger.LogInformation($"Skrapar aktiekurser");

            var table = _webDriver.FindElements(By.XPath("//table[contains(@class, 'afv-table-body-list')]/tbody/tr"));

            foreach (var row in table)
            {
                var cells = row.FindElements(By.TagName("td"));
                var name = cells[_nameCol].Text;
                var price = cells[_priceCol].DecimalTextAsDecimal();
                var turnover = cells[_turnoverCol].NumberTextAsDecimal();

                _logger.LogTrace($"Namn = {name}, Kurs = {price}, Omsättn. = {turnover}");

                stockData.Add(new Stock
                {
                    Name = name,
                    Price = price,
                    Volume = turnover,
                });
            }
        }

        public void ScrapeStockIndicators(ref List<Stock> stockData)
        {
            _logger.LogInformation($"Skrapar aktieindikatorer");

            var stockDictionary = new Dictionary<string, Stock>();
            stockData.ForEach(s => stockDictionary.Add(s.Name, s));

            var table = _webDriver.FindElements(By.XPath("//table[contains(@class, 'afv-table-body-list')]/tbody/tr"));

            foreach (var row in table)
            {
                var cells = row.FindElements(By.TagName("td"));
                var stock = stockDictionary[cells[_nameCol].Text];

                var adjustedEquityPerStock = cells[_adjustedEquityPerStockCol].DecimalTextAsDecimal();
                var directYield = cells[_directYieldCol].DecimalTextAsDecimal();
                var profitPerStock = cells[_profitPerStockCol].DecimalTextAsDecimal();

                _logger.LogTrace($"JEK/aktie = {adjustedEquityPerStock}, Dir.avk. = {directYield}, Vinst/aktie = {profitPerStock}");

                stock.AdjustedEquityPerStock = adjustedEquityPerStock;
                stock.DirectYield = directYield;
                stock.ProfitPerStock = profitPerStock;
            }

            stockData = stockDictionary.Values.ToList();
        }

        public void SetIndustries(ref List<Stock> stockData)
        {
            _logger.LogDebug($"Tillsätter branscher");

            foreach (var industryData in _industries)
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
