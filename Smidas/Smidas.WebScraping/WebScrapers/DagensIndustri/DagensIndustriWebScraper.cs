﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml.XPath;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Smidas.Common;
using Smidas.Common.Extensions;
using Smidas.Core.Stocks;
using Smidas.WebScraping.WebScrapers.Parsing;

namespace Smidas.WebScraping.WebScrapers.DagensIndustri
{
    public class DagensIndustriWebScraper : IWebScraper
    {
        private readonly ILogger _logger;

        private HtmlDocument _html;

        public DagensIndustriWebScraper(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<DagensIndustriWebScraper>();
        }

        public async Task<IList<Stock>> Scrape(AktieReaQuery query)
        {
            _logger.LogInformation($"Skrapar {query.IndexUrl}");

            _html = await new HtmlWeb().LoadFromWebAsync(query.IndexUrl);

            List<string> names = null;
            List<decimal> prices = null;
            List<decimal> volumes = null;
            List<decimal> profitPerStock = null;
            List<decimal> adjustedEquityPerStock = null;
            List<decimal> directDividend = null;

            try
            {
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

                () => directDividend = ScrapeNodes("//tr[contains(@class, 'js_real-time-Nyckeltal')]/td[7]")
                                        .Parse(hasSymbol: true)
                                        .ToList());
            }
            catch (AggregateException ex)
            {
                if (ex.InnerExceptions.Any((innerEx) => innerEx is XPathException))
                {
                    throw new WebScrapingException("One or more XPaths failed. Check if website has been redesigned.", ex);
                }

                throw;
            }

            // All lists must hold the same amount of elements
            if (new[] { prices, volumes, profitPerStock, adjustedEquityPerStock, directDividend }
                .All(l => l.Count() != names.Count()))
            {
                _logger.LogError($"Elementlistorna har ej samma längd\n" +
                    $"Namn: {names.Count()} st, Priser: {prices.Count()} st, Volymer: {volumes.Count()} st, Vinst/aktie: {profitPerStock.Count()} st, JEK/aktie: {adjustedEquityPerStock.Count()}, Dir.avk: {directDividend.Count()} st");

                throw new ValidationException($"Elementlistorna har ej samma längd\n" +
                    $"Namn: {names.Count()} st, Priser: {prices.Count()} st, Volymer: {volumes.Count()} st, Vinst/aktie: {profitPerStock.Count()} st, JEK/aktie: {adjustedEquityPerStock.Count()}, Dir.avk: {directDividend.Count()} st");
            }

            _logger.LogInformation("Element - OK");

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
                    DirectDividend = directDividend[i],
                };
                stocks.Add(stock);
            }

            SetIndustries(ref stocks, query);

            _logger.LogInformation("Skrapning slutförd");
            return stocks;
        }

        private IEnumerable<string> ScrapeNodes(string xPath)
        {
            try
            {
                return _html.DocumentNode.SelectNodes(xPath).Select(n => WebUtility.HtmlDecode(n.InnerText));
            }
            catch (ArgumentNullException ex)
            {
                throw new XPathException("Element not found on website.", ex);
            }
        } 

        public void SetIndustries(ref List<Stock> stockData, AktieReaQuery query)
        {
            foreach (KeyValuePair<string, AktieReaQuery.IndustryData> industryData in query.Industries)
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
