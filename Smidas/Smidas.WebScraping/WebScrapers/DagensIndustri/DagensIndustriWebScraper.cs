using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml.XPath;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Serilog.Events;
using Smidas.Common;
using Smidas.Common.Attributes;
using Smidas.Common.Extensions;
using Smidas.Core.Stocks;
using Smidas.WebScraping.WebScrapers.Html;
using Smidas.WebScraping.WebScrapers.Parsing;

namespace Smidas.WebScraping.WebScrapers.DagensIndustri
{
    public class DagensIndustriWebScraper : IDagensIndustriWebScraper
    {
        private readonly ILogger<DagensIndustriWebScraper> _logger;
        private readonly IHtmlWebFactory _htmlWebFactory;

        public DagensIndustriWebScraper(ILogger<DagensIndustriWebScraper> logger,
            IHtmlWebFactory htmlWebFactory)
        {
            _htmlWebFactory = htmlWebFactory;
            _logger = logger;
        }

        [StandardLogging]
        public IList<Stock> Scrape(AktieReaQuery query)
        {
            var htmlWeb = _htmlWebFactory.Create();
            _logger.LogDebug($"UserAgent: {htmlWeb.UserAgent}");
            
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
                    _logger.LogTrace($"Scraping {query.IndexUrls[i]} ({i + 1}/{query.IndexUrls.Length})");
                    var document = htmlWeb.Load(query.IndexUrls[i]);

                    var scrapedNames = ScrapeNodes(document, query.XPathExpressions.Names);
                    var scrapedPrices = ScrapeNodes(document, query.XPathExpressions.Prices).Parse();
                    var scrapedVolumes = ScrapeNodes(document, query.XPathExpressions.Volumes).Parse();
                    var scrapedProfitPerStock = ScrapeNodes(document, query.XPathExpressions.ProfitPerStock).Parse();
                    var scrapedAdjustedEquityPerStock =
                        ScrapeNodes(document, query.XPathExpressions.AdjustedEquityPerStock).Parse();
                    var scrapedDirectDividend = ScrapeNodes(document, query.XPathExpressions.DirectDividend)
                        .Parse(DecimalType.Percentage);

                    var nameList = scrapedNames.ToList();

                    // All lists must hold the same amount of elements
                    if (new[] { prices, volumes, profitPerStock, adjustedEquityPerStock, directDividend }
                        .Any(l => l.Count != names.Count))
                    {
                        var message = $"Expected same count in all lists, but found one or more discrepancies:\n" +
                                      $"Name:                   {nameList.Count}, " +
                                      $"Prices:                 {scrapedPrices.Count()}, " +
                                      $"Volumes:                {scrapedVolumes.Count()}, " +
                                      $"Profit/stock:           {scrapedProfitPerStock.Count()}, " +
                                      $"Adjusted equity/stock:  {scrapedAdjustedEquityPerStock.Count()}, " +
                                      $"Direct dividend:        {scrapedDirectDividend.Count()}";
                        _logger.LogError(message);
                        throw new ValidationException(message);
                    }
                    
                    _logger.LogTrace($"Scraped {nameList.Count} rows");

                    names.AddRange(nameList);
                    prices.AddRange(scrapedPrices);
                    volumes.AddRange(scrapedVolumes);
                    profitPerStock.AddRange(scrapedProfitPerStock);
                    adjustedEquityPerStock.AddRange(scrapedAdjustedEquityPerStock);
                    directDividend.AddRange(scrapedDirectDividend);
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

            _logger.LogTrace($"{names.Count} rows total - OK");

            var stocks = names.Select((t, i) => new Stock
                {
                    Name = t,
                    Price = prices[i],
                    Volume = volumes[i],
                    ProfitPerStock = profitPerStock[i],
                    AdjustedEquityPerStock = adjustedEquityPerStock[i],
                    DirectDividend = directDividend[i],
                })
                .ToList();

            SetIndustries(ref stocks, query);

            return stocks;
        }

        [StandardLogging(Level = LogEventLevel.Verbose)]
        private static IEnumerable<string> ScrapeNodes(HtmlDocument document, string xPath)
        {
            var node = document.DocumentNode.SelectNodes(xPath);

            if (node == null)
            {
                throw new XPathException($"Element not found on website. XPath: {xPath}");
            }
            
            return node.Select(n => WebUtility.HtmlDecode(n.InnerText));
        }

        [StandardLogging(Level = LogEventLevel.Verbose)]
        private static void SetIndustries(ref List<Stock> stockData, AktieReaQuery query)
        {
            foreach (var (industry, value) in query.Industries)
            {
                if (value.Companies == null) 
                    continue;

                foreach (var companyName in value.Companies)
                {
                    stockData.Where(s => s.Name.Contains(companyName))
                        .ForEach(s => s.Industry = industry);
                }
            }
        }
    }
}
