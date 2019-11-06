using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using Smidas.Core.Stocks;
using Smidas.WebScraping.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Smidas.WebScraping.AffarsVarlden
{
    public class AffarsVarldenWebScraper : WebScraper<Stock>
    {
        // Share prices page
        private const int _nameIndex = 0;
        private const int _priceIndex = 6;
        private const int _turnoverIndex = 10;

        // Stock indicators page
        private const int _adjustedEquityPerStock = 3;
        private const int _directYieldIndex = 6;
        private const int _profitPerStock = 7;

        public string SharePricesUrl { get; set; } = "https://www.affarsvarlden.se/bors/kurslistor/stockholm-large/kurs/";

        public string StockIndicatorsUrl { get; set; } = "https://www.affarsvarlden.se/bors/kurslistor/stockholm-large/aktieindikatorn/";

        public AffarsVarldenWebScraper(IWebDriver webDriver, ILogger logger) : base(webDriver, logger)
        {
        }

        public override IEnumerable<Stock> Scrape()
        {
            _logger.LogInformation($"Scraping started");
            var stockData = new List<Stock>();

            _logger.LogInformation($"Navigating to {SharePricesUrl}");
            WebDriver.Navigate().GoToUrl(SharePricesUrl);

            Wait();

            ScrapeSharePrices(ref stockData);

            ClickNextButton();

            Wait();

            ScrapeSharePrices(ref stockData);

            _logger.LogInformation($"Navigating to {SharePricesUrl}");
            WebDriver.Navigate().GoToUrl(StockIndicatorsUrl);

            Wait();

            ScrapeStockIndicators(ref stockData);

            ClickNextButton();

            Wait();

            ScrapeStockIndicators(ref stockData);

            _logger.LogInformation($"Scraping completed");
            return stockData.AsEnumerable();
        }

        public void ClickNextButton()
        {
            _logger.LogInformation($"Clicking to page 2");
            WebDriver.ExecuteScript("arguments[0].click();", WebDriver.FindElement(By.XPath("//a[text()='>']")));
        }

        public void ScrapeSharePrices(ref List<Stock> stockData)
        {
            _logger.LogInformation($"Scraping share prices");
            var table = WebDriver.FindElements(By.XPath("//table[contains(@class, 'afv-table-body-list')]/tbody/tr"));
            foreach (var row in table)
            {
                var cells = WebDriver.FindElements(By.TagName("td"));
                stockData.Add(new Stock
                {
                    Name = cells[_nameIndex].Text,
                    Price = cells[_priceIndex].TextAsDecimal(),
                    Turnover = cells[_turnoverIndex].TextAsKNumber(),
                });
            }
        }

        public void ScrapeStockIndicators(ref List<Stock> stockData)
        {
            _logger.LogInformation($"Scraping stock indicators");
            var table = WebDriver.FindElements(By.XPath("//table[contains(@class, 'afv-table-body-list')]/tbody/tr"));
            foreach (var row in table)
            {
                var cells = row.FindElements(By.TagName("td"));
                var stock = stockData.Single(d => d.Name == cells[_nameIndex].Text);
                stock.AdjustedEquityPerStock = cells[_adjustedEquityPerStock].TextAsDecimal();
                stock.DirectYield = cells[_directYieldIndex].TextAsDecimal();
                stock.ProfitPerStock = cells[_profitPerStock].TextAsDecimal();
            }
        }
    }
}
