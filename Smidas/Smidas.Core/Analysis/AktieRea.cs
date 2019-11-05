using Smidas.Core.Stocks;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Smidas.Common.Extensions;

namespace Smidas.Core.Analysis
{
    public static class AktieRea
    {
        public static List<Stock> Analyze(
            List<Stock> stocks, 
            IEnumerable<string> blacklist,
            int investmentStocksCap = 2,
            int realEstateStocksCap = 2,
            int bankingStocksCap = 2)
        {
            ExcludeDisqualifiedStocks(ref stocks, blacklist);

            ExcludeDoubles(ref stocks);

            CalculateARank(ref stocks);

            CalculateBRank(ref stocks);

            DetermineActions(ref stocks, investmentStocksCap, realEstateStocksCap, bankingStocksCap);

            return stocks.OrderBy(s => s.AbRank)
                         .ToList();
        }

        public static void ExcludeDisqualifiedStocks(ref List<Stock> stocks, IEnumerable<string> blacklist)
        {
            foreach (var stock in stocks)
            {
                // Blacklisted stocks
                foreach (var name in blacklist)
                {
                    if (stock.Name.Contains(name))
                    {
                        stock.Exclude("Blacklisted.");
                    }
                }
                
                if (stock.ProfitPerStock < 0m) // Stocks with negative profit per stock
                {
                    stock.Exclude("Negative profit per stock.");
                }
                else if (stock.DirectYield == 0) // Stocks with zero direct yield
                {
                    stock.Exclude("Zero direct yield.");
                }
                else if (Regex.IsMatch(stock.Name, ".* Pref$")) // Preferential stocks
                {
                    stock.Exclude("Preferred stock.");
                }
            }
        }

        public static void ExcludeDoubles(ref List<Stock> stocks)
        {
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
                    stock.Exclude("Is a double");
                }
            }
        }

        public static void CalculateARank(ref List<Stock> stocks)
        {
            stocks = stocks.OrderByDescending(s => s.Ep).ToList();
            for (int i = 0; i < stocks.Count(); i++)
            {
                stocks[i].ARank = i + 1;
            }
        }

        public static void CalculateBRank(ref List<Stock> stocks)
        {
            stocks = stocks.OrderByDescending(s => s.AdjustedEquityPerStock).ToList();
            for (int i = 0; i < stocks.Count(); i++)
            {
                stocks[i].BRank = i + 1;
            }
        }

        public static void DetermineActions(
            ref List<Stock> stocks, 
            int investmentStocksCap, 
            int realEstateStocksCap, 
            int bankingStocksCap)
        {
            var index = 1;
            var investmentStocks = 0;
            var realEstateStocks = 0;
            var bankingStocks = 0;

            stocks = stocks.OrderBy(s => s.AbRank).ToList();
            foreach (var stock in stocks)
            {
                if (stock.Action == Action.Exclude)
                {
                    continue;
                }

                switch (stock.Industry)
                {
                    case Industry.Investment:
                        if (investmentStocks == investmentStocksCap)
                        {
                            stock.Exclude("Investment stocks cap reached.");
                            continue;
                        }
                        investmentStocks++;
                        break;

                    case Industry.RealEstate:
                        if (realEstateStocks == realEstateStocksCap)
                        {
                            stock.Exclude("Real estate stocks cap reached.");
                            continue;
                        }
                        realEstateStocks++;
                        break;

                    case Industry.Banking:
                        if (bankingStocks == bankingStocksCap)
                        {
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

        public static Action DetermineActionByIndex(int index)
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
