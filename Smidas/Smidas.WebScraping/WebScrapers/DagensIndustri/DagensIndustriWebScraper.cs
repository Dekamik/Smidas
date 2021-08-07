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
    public class DagensIndustriWebScraper : IDagensIndustriWebScraper
    {
        private readonly ILogger<DagensIndustriWebScraper> _logger;

        public DagensIndustriWebScraper(ILogger<DagensIndustriWebScraper> logger)
        {
            _logger = logger;
        }

        public async Task<IList<Stock>> Scrape(AktieReaQuery query)
        {
            var htmlWeb = new HtmlWeb();
            _logger.LogInformation($"UserAgent: {htmlWeb.UserAgent}");
            
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
                    var document = await htmlWeb.LoadFromWebAsync(query.IndexUrls[i]);

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
                        var message = $"Förväntade samma antal skrapade objekt i samtliga listor, men hittade en eller flera avvikelser:\n" +
                                      $"Namn:        {nameList.Count} st, " +
                                      $"Priser:      {scrapedPrices.Count()} st, " +
                                      $"Volymer:     {scrapedVolumes.Count()} st, " +
                                      $"Vinst/aktie: {scrapedProfitPerStock.Count()} st, " +
                                      $"JEK/aktie:   {scrapedAdjustedEquityPerStock.Count()} st" +
                                      $"Dir.avk:     {scrapedDirectDividend.Count()} st";
                        _logger.LogError(message);
                        throw new ValidationException(message);
                    }
                    
                    _logger.LogInformation($"Skrapade {nameList.Count} rader");

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

            _logger.LogInformation($"{names.Count} rader skrapade totalt - OK");

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
