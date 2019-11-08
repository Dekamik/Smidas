using Smidas.Core.Stocks;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Smidas.Common.Extensions;
using Microsoft.Extensions.Logging;

namespace Smidas.Core.Analysis
{
    public class AktieRea : IAnalysis
    {
        private readonly ILogger _logger;

        public int InvestmentStocksCap { get; set; } = 2;

        public int RealEstateStocksCap { get; set; } = 2;

        public int BankingStocksCap { get; set; } = 2;

        public AktieRea(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<AktieRea>();
        }

        public IEnumerable<Stock> Analyze(IEnumerable<Stock> stocks, IEnumerable<string> blacklist)
        {
            ExcludeDisqualifiedStocks(ref stocks, blacklist);

            ExcludeDoubles(ref stocks);

            CalculateARank(ref stocks);

            CalculateBRank(ref stocks);

            DetermineActions(ref stocks, InvestmentStocksCap, RealEstateStocksCap, BankingStocksCap);

            return stocks.OrderBy(s => s.AbRank);
        }

        public void ExcludeDisqualifiedStocks(ref IEnumerable<Stock> stocks, IEnumerable<string> blacklist)
        {
            _logger.LogDebug($"Exluding disqualified stocks");

            foreach (var stock in stocks)
            {
                // Blacklisted stocks
                foreach (var name in blacklist)
                {
                    if (stock.Name.Contains(name))
                    {
                        _logger.LogTrace($"Excluding {stock.Name} - Blacklisted.");
                        stock.Exclude("Blacklisted.");
                    }
                }
                
                if (stock.ProfitPerStock < 0m) // Stocks with negative profit per stock
                {
                    _logger.LogTrace($"Excluding {stock.Name} - Negative profit per stock.");
                    stock.Exclude("Negative profit per stock.");
                }
                else if (stock.DirectYield == 0) // Stocks with zero direct yield
                {
                    _logger.LogTrace($"Excluding {stock.Name} - Zero direct yield.");
                    stock.Exclude("Zero direct yield.");
                }
                else if (Regex.IsMatch(stock.Name, ".* Pref$")) // Preferential stocks
                {
                    _logger.LogTrace($"Excluding {stock.Name} - Preferred stock.");
                    stock.Exclude("Preferred stock.");
                }
            }
        }

        public void ExcludeDoubles(ref IEnumerable<Stock> stocks)
        {
            _logger.LogDebug($"Exluding doubles");

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
                    _logger.LogTrace($"Excluding {stock.Name} - Is a double.");
                    stock.Exclude("Is a double");
                }
            }
        }

        public void CalculateARank(ref IEnumerable<Stock> stocks)
        {
            _logger.LogDebug($"Calculating A-rank");

            int i = 1;
            stocks.OrderByDescending(s => s.Ep)
                  .ForEach(s => s.ARank = i++);
        }

        public void CalculateBRank(ref IEnumerable<Stock> stocks)
        {
            _logger.LogDebug($"Calculating B-rank");

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
            _logger.LogDebug($"Determining actions");

            var index = 1;
            var investmentStocks = 0;
            var realEstateStocks = 0;
            var bankingStocks = 0;

            foreach (var stock in stocks.OrderBy(s => s.AbRank))
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
                            _logger.LogTrace($"Excluding {stock.Name} - Investment stocks cap reached.");
                            stock.Exclude("Investment stocks cap reached.");
                            continue;
                        }
                        investmentStocks++;
                        break;

                    case Industry.RealEstate:
                        if (realEstateStocks == realEstateStocksCap)
                        {
                            _logger.LogTrace($"Excluding {stock.Name} - Real estate stocks cap reached.");
                            stock.Exclude("Real estate stocks cap reached.");
                            continue;
                        }
                        realEstateStocks++;
                        break;

                    case Industry.Banking:
                        if (bankingStocks == bankingStocksCap)
                        {
                            _logger.LogTrace($"Excluding {stock.Name} - Banking stocks cap reached.");
                            stock.Exclude("Banking stocks cap reached.");
                            continue;
                        }
                        bankingStocks++;
                        break;

                    default:
                    case Industry.Other:
                        break;
                }

                stock.Action = DetermineActionByIndex(index);
                index++;
            }
        }

        public Action DetermineActionByIndex(int index)
        {
            if (index <= 0)
                throw new System.ArgumentOutOfRangeException(nameof(index));

            if (index <= 10)
            {
                return Action.Buy;
            }
            else if (index <= 20)
            {
                return Action.Keep;
            }
            else
            {
                return Action.Sell;
            }
        }
    }
}
