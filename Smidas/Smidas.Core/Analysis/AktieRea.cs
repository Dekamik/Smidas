using Smidas.Core.Stocks;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Smidas.Common.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Smidas.Common;

namespace Smidas.Core.Analysis
{
    public class AktieRea : IAnalysis
    {
        private readonly ILogger _logger;

        private readonly int _investmentStocksCap;

        private readonly int _realEstateStocksCap;

        private readonly int _bankingStocksCap;

        public AktieRea(ILoggerFactory loggerFactory, AppSettings config)
        {
            _logger = loggerFactory.CreateLogger<AktieRea>();

            _investmentStocksCap = config.Industries.Single(i => i.Enum == Industry.Investment.ToString()).Cap;

            _realEstateStocksCap = config.Industries.Single(i => i.Enum == Industry.RealEstate.ToString()).Cap;

            _bankingStocksCap = config.Industries.Single(i => i.Enum == Industry.Banking.ToString()).Cap;
        }

        public IEnumerable<Stock> Analyze(IEnumerable<Stock> stocks, IEnumerable<string> blacklist)
        {
            _logger.LogInformation($"Analyserar {stocks.Count()} aktier enligt AktieREA-metoden");

            ExcludeDisqualifiedStocks(ref stocks, blacklist);

            ExcludeDoubles(ref stocks);

            CalculateARank(ref stocks);

            CalculateBRank(ref stocks);

            DetermineActions(ref stocks, _investmentStocksCap, _realEstateStocksCap, _bankingStocksCap);

            _logger.LogInformation($"Analys slutförd\n" +
                                   $"\n" +
                                   $"Köpes          : {stocks.Count(s => s.Action == Action.Buy)} \t(Inv: {stocks.Count(s => s.Action == Action.Buy && s.Industry == Industry.Investment)} \tFast: {stocks.Count(s => s.Action == Action.Buy && s.Industry == Industry.RealEstate)} \tBank: {stocks.Count(s => s.Action == Action.Buy && s.Industry == Industry.Banking)})\n" +
                                   $"Behålles       : {stocks.Count(s => s.Action == Action.Keep)} \t(Inv: {stocks.Count(s => s.Action == Action.Keep && s.Industry == Industry.Investment)} \tFast: {stocks.Count(s => s.Action == Action.Keep && s.Industry == Industry.RealEstate)} \tBank: {stocks.Count(s => s.Action == Action.Keep && s.Industry == Industry.Banking)})\n" +
                                   $"Säljes         : {stocks.Count(s => s.Action == Action.Sell)} \t(Inv: {stocks.Count(s => s.Action == Action.Sell && s.Industry == Industry.Investment)} \tFast: {stocks.Count(s => s.Action == Action.Sell && s.Industry == Industry.RealEstate)} \tBank: {stocks.Count(s => s.Action == Action.Sell && s.Industry == Industry.Banking)})\n" +
                                   $"Bortsållade    : {stocks.Count(s => s.Action == Action.Exclude)} \t(Inv: {stocks.Count(s => s.Action == Action.Exclude && s.Industry == Industry.Investment)} \tFast: {stocks.Count(s => s.Action == Action.Exclude && s.Industry == Industry.RealEstate)} \tBank: {stocks.Count(s => s.Action == Action.Exclude && s.Industry == Industry.Banking)})\n" +
                                   $"\n" +
                                   $"S:a            : {stocks.Count()} \t(Inv: {stocks.Count(s => s.Industry == Industry.Investment)} \tFast: {stocks.Count(s => s.Industry == Industry.RealEstate)} \tBank: {stocks.Count(s => s.Industry == Industry.Banking)})\n");

            return stocks.OrderBy(s => s.AbRank)
                         .ThenByDescending(s => s.DirectYield);
        }

        public void ExcludeDisqualifiedStocks(ref IEnumerable<Stock> stocks, IEnumerable<string> blacklist)
        {
            _logger.LogDebug($"Sållar diskvalivicerade aktier");

            foreach (var stock in stocks)
            {
                // Blacklisted stocks
                foreach (var name in blacklist)
                {
                    if (stock.Name.Contains(name))
                    {
                        _logger.LogTrace($"Sållade {stock.Name} - Svartlistad");
                        stock.Exclude("Svartlistad");
                    }
                }
                
                if (stock.ProfitPerStock < 0m) // Stocks with negative profit per stock
                {
                    _logger.LogTrace($"Sållade {stock.Name} - Negativ vinst");
                    stock.Exclude("Negativ vinst");
                }
                else if (stock.DirectYield == 0) // Stocks with zero direct yield
                {
                    _logger.LogTrace($"Sållade {stock.Name} - Ingen direktavkastning");
                    stock.Exclude("Ingen direktavkastning");
                }
                else if (Regex.IsMatch(stock.Name, ".* Pref$")) // Preferential stocks
                {
                    _logger.LogTrace($"Sållade {stock.Name} - Preferensaktie");
                    stock.Exclude("Preferensaktie");
                }
            }
        }

        public void ExcludeDoubles(ref IEnumerable<Stock> stocks)
        {
            _logger.LogDebug($"Sållar dubbletter");

            var series = stocks.Where(s => Regex.IsMatch(s.Name, ".* [A-Z]$"));
            var doublesCount = new Dictionary<string, int>();

            foreach (var stock in series) // Count amount of doubles per series
            {
                doublesCount[stock.CompanyName] = doublesCount.ContainsKey(stock.CompanyName) ? doublesCount[stock.CompanyName] + 1 : 1;
            }

            // Select all series that have at least two stocks
            var doubleStocks = series.Where(s => doublesCount[s.CompanyName] > 1);
            var doubleCompanies = doubleStocks.Select(s => s.CompanyName)
                                              .Distinct();
            var stocksToExclude = new HashSet<string>();

            foreach (var companyName in doubleCompanies)
            {
                // Select the company's stocks, ordered with largest turnover first
                var company = doubleStocks.Where(s => s.Name.Contains(companyName))
                                          .OrderByDescending(s => s.Turnover);

                // Keep the stock with the largest turnover (the first one). Exclude the rest.
                company.Skip(1)
                       .Take(company.Count() - 1)
                       .ForEach(c => stocksToExclude.Add(c.Name));
            }

            foreach (var stock in stocks)
            {
                if (stocksToExclude.Contains(stock.Name))
                {
                    _logger.LogTrace($"Sållade {stock.Name} - Dubblett");
                    stock.Exclude("Dubblett");
                }
            }
        }

        public void CalculateARank(ref IEnumerable<Stock> stocks)
        {
            _logger.LogDebug($"Räknar ut A-rang");

            int i = 1;
            stocks.OrderByDescending(s => s.Ep)
                  .ForEach(s => s.ARank = i++);
        }

        public void CalculateBRank(ref IEnumerable<Stock> stocks)
        {
            _logger.LogDebug($"Räknar ut B-rang");

            int i = 1;
            stocks.OrderByDescending(s => s.AdjustedEquityPerStock)
                  .ForEach(s => s.BRank = i++);
        }

        public void DetermineActions(
            ref IEnumerable<Stock> stocks, 
            int investmentStocksCap, 
            int realEstateStocksCap, 
            int bankingStocksCap)
        {
            _logger.LogDebug($"Beslutar åtgärder");

            var index = 1;
            var investmentStocks = 0;
            var realEstateStocks = 0;
            var bankingStocks = 0;

            foreach (var stock in stocks.OrderBy(s => s.AbRank)
                                        .ThenByDescending(s => s.DirectYield))
            {
                if (stock.Action == Action.Exclude)
                {
                    continue;
                }

                // Check if the stock belongs to a capped industry, and whether or not the cap has been reached.
                switch (stock.Industry)
                {
                    case Industry.Investment:
                        if (investmentStocks == investmentStocksCap)
                        {
                            _logger.LogTrace($"Sållade {stock.Name} - Max antal investeringsbolag nådd");
                            stock.Exclude("Max antal investeringsbolag nådd");
                            continue;
                        }
                        investmentStocks++;
                        break;

                    case Industry.RealEstate:
                        if (realEstateStocks == realEstateStocksCap)
                        {
                            _logger.LogTrace($"Sållade {stock.Name} - Max antal fastighetsbolag nådd");
                            stock.Exclude("Max antal fastighetsbolag nådd");
                            continue;
                        }
                        realEstateStocks++;
                        break;

                    case Industry.Banking:
                        if (bankingStocks == bankingStocksCap)
                        {
                            _logger.LogTrace($"Sållade {stock.Name} - Max antal bankbolag nådd");
                            stock.Exclude("Max antal bankbolag nådd");
                            continue;
                        }
                        bankingStocks++;
                        break;

                    default:
                    case Industry.Other:
                        break;
                }

                stock.Action = index <= 10 ? Action.Buy :
                               index <= 20 ? Action.Keep :
                                             Action.Sell;
                index++;
            }
        }
    }
}
