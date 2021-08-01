using System;
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

        public DagensIndustriWebScraper(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<DagensIndustriWebScraper>();
        }

        public async Task<IList<Stock>> Scrape(AktieReaQuery query)
        {
            var names = new List<string>();
            var prices = new List<decimal>();
            var volumes = new List<decimal>();
            var profitPerStock = new List<decimal>();
            var adjustedEquityPerStock = new List<decimal>();
            var directDividend = new List<decimal>();

            for (var i = 0; i < query.IndexUrls.Length; i++)
            {
                try
                {
                    _logger.LogInformation($"Skrapar {query.IndexUrls[i]} ({i + 1}/{query.IndexUrls.Length})");
                    var document = await new HtmlWeb().LoadFromWebAsync(query.IndexUrls[i]);

                    names.AddRange(ScrapeNodes(document, query.XPathExpressions.Names));
                    prices.AddRange(ScrapeNodes(document, query.XPathExpressions.Prices).Parse());
                    volumes.AddRange(ScrapeNodes(document, query.XPathExpressions.Volumes).Parse());
                    profitPerStock.AddRange(ScrapeNodes(document, query.XPathExpressions.ProfitPerStock).Parse());
                    adjustedEquityPerStock.AddRange(ScrapeNodes(document, query.XPathExpressions.AdjustedEquityPerStock)
                        .Parse());
                    directDividend.AddRange(ScrapeNodes(document, query.XPathExpressions.DirectDividend)
                        .Parse(DecimalType.Percentage));
                }
                catch (AggregateException ex)
                {
                    if (ex.InnerExceptions.Any(innerEx => innerEx is XPathException))
                    {
                        throw new WebScrapingException("One or more XPaths failed. Check if website has been redesigned.", ex);
                    }

                    throw;
                }
            }

            // All lists must hold the same amount of elements
            if (new[] { prices, volumes, profitPerStock, adjustedEquityPerStock, directDividend }
                .Any(l => l.Count != names.Count))
            {
                var message = $"Elementlistorna har ej samma längd\n" +
                              $"Namn: {names.Count} st, " +
                              $"Priser: {prices.Count} st, " +
                              $"Volymer: {volumes.Count} st, " +
                              $"Vinst/aktie: {profitPerStock.Count} st, " +
                              $"JEK/aktie: {adjustedEquityPerStock.Count}, " +
                              $"Dir.avk: {directDividend.Count} st";
                _logger.LogError(message);
                throw new ValidationException(message);
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

        private IEnumerable<string> ScrapeNodes(HtmlDocument document, string xPath)
        {
            var node = document.DocumentNode.SelectNodes(xPath);

            if (node == null)
            {
                throw new XPathException($"Element not found on website. XPath: {xPath}");
            }
            
            return node.Select(n => WebUtility.HtmlDecode(n.InnerText));
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
