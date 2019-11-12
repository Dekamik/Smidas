﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using Smidas.Common;
using Smidas.Common.Extensions;
using Smidas.Core.Stocks;
using Smidas.WebScraping.Extensions;
using Smidas.WebScraping.WebDriver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Smidas.WebScraping.AffarsVarlden
{
    public class AffarsVarldenWebScraper : WebScraper
    {
        // Share prices page
        private const int _nameIndex = 0;
        private const int _priceIndex = 6;
        private const int _turnoverIndex = 10;

        // Stock indicators page
        private const int _adjustedEquityPerStock = 3;
        private const int _directYieldIndex = 6;
        private const int _profitPerStock = 7;

        private readonly IOptions<AppSettings> _config;

        private IDictionary<string, AppSettings.IndexSettings.IndustryData> _industries;

        private StockIndex _index;

        public StockIndex Index 
        {
            get 
            {
                return _index;
            }
            set 
            {
                _index = value;
                _industries = _config.Value.AktieRea[_index.ToString()].Industries;
            } 
        }

        public AffarsVarldenWebScraper(
            IWebDriverFactory webDriverFactory, 
            ILoggerFactory loggerFactory, 
            IOptions<AppSettings> config) 
            : base(
                  webDriverFactory.Create(config.Value.WebScraper.ChromeDriverDirectory), 
                  loggerFactory.CreateLogger<AffarsVarldenWebScraper>(),
                  config.Value)
        {
            _config = config;
        }

        public override IEnumerable<Stock> Scrape()
        {
            _logger.LogInformation($"Skrapar affärsvärlden.se");
            var stockData = new List<Stock>();

            NavigateTo($"https://www.affarsvarlden.se/bors/kurslistor/{Index.GetDescription()}/kurs/");

            Wait();

            ScrapeSharePrices(ref stockData);

            // Pretty hacky, should support pagination instead
            if (Index == StockIndex.OmxStockholmLargeCap)
            {
                ClickNextButton();

                Wait();

                ScrapeSharePrices(ref stockData);
            }

            NavigateTo($"https://www.affarsvarlden.se/bors/kurslistor/{Index.GetDescription()}/aktieindikatorn/");

            Wait();

            ScrapeStockIndicators(ref stockData);

            if (Index == StockIndex.OmxStockholmLargeCap)
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
                var name = cells[_nameIndex].Text;
                var price = cells[_priceIndex].TextAsDecimal();
                var turnover = cells[_turnoverIndex].TextAsNumber();

                _logger.LogTrace($"Namn = {name}\tKurs = {price}\tOmsättn. = {turnover}");

                stockData.Add(new Stock
                {
                    Name = name,
                    Price = price,
                    Turnover = turnover,
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
                var stock = stockDictionary[cells[_nameIndex].Text];

                var adjustedEquityPerStock = cells[_adjustedEquityPerStock].TextAsDecimal();
                var directYield = cells[_directYieldIndex].TextAsDecimal();
                var profitPerStock = cells[_profitPerStock].TextAsDecimal();

                _logger.LogTrace($"Namn = {stock.Name}\tJEK/aktie = {adjustedEquityPerStock}\tDir.avk. = {directYield}\tVinst/aktie = {profitPerStock}");

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
