using Smidas.Core.Stocks;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Smidas.Core.Analysis
{
    public static class AktieRea
    {
        private static readonly string PreferentialStocksPattern = ".* Pref$";

        private static readonly string SeriesPattern = ".* [A-Z]$";

        public static IEnumerable<Stock> Analyze(
            List<Stock> stocks, 
            IEnumerable<string> blacklist,
            int investmentStocksCap = 2,
            int realEstateStocksCap = 2,
            int bankingStocksCap = 2)
        {
            // Exclusions
            foreach (var stock in stocks)
            {
                if (blacklist.Contains(stock.Name)) // Blacklisted stocks
                {
                    stock.Exclude("Blacklisted.");
                }
                else if (stock.ProfitPerStock < 0m) // Stocks with negative profit per stock
                {
                    stock.Exclude("Negative profit per stock.");
                }
                else if (stock.DirectYield == 0) // Stocks with zero direct yield
                {
                    stock.Exclude("Zero direct yield.");
                }
                else if (Regex.IsMatch(stock.Name, PreferentialStocksPattern)) // Preferential stocks
                {
                    stock.Exclude("Preferred stock");
                }
            }

            ExcludeDoubles(ref stocks);

            // Calculate A-rank
            stocks.OrderByDescending(s => s.Ep);
            for (int i = 0; i < stocks.Count(); i++)
            {
                stocks[i].ARank = i + 1;
            }

            // Calculate B-rank
            stocks.OrderByDescending(s => s.JekPerStock);
            for (int i = 0; i < stocks.Count(); i++)
            {
                stocks[i].BRank = i + 1;
            }

            // Determine actions
            var index = 0;
            var investmentStocks = 0;
            var realEstateStocks = 0;
            var bankingStocks = 0;

            stocks.OrderBy(s => s.AbRank);
            foreach (var stock in stocks)
            {
                if (stock.Action == Stocks.Action.Exclude)
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
                        else
                        {
                            investmentStocks++;
                        }
                        break;

                    case Industry.RealEstate:
                        if (realEstateStocks == realEstateStocksCap)
                        {
                            stock.Exclude("Real estate stocks cap reached.");
                            continue;
                        }
                        else
                        {
                            realEstateStocks++;
                        }
                        break;

                    case Industry.Banking:
                        if (bankingStocks == bankingStocksCap)
                        {
                            stock.Exclude("Banking stocks cap reached.");
                            continue;
                        }
                        else
                        {
                            bankingStocks++;
                        }
                        break;

                    default:
                    case Industry.Other:
                        break;
                }

                stock.Action = DetermineAction(index);
                index++;
            }

            // Return stocks
            return stocks.OrderBy(s => s.AbRank);
        }

        private static void ExcludeDoubles(ref List<Stock> stocks)
        {
            var series = stocks.Where(s => Regex.IsMatch(s.Name, SeriesPattern));
            var doublesCount = new Dictionary<string, int>();

            foreach (var stock in series) // Count amount of doubles per series
            {
                var seriesName = GetSeriesName(stock.Name);
                if (doublesCount.ContainsKey(seriesName))
                {
                    doublesCount[seriesName]++;
                }
                else
                {
                    doublesCount[seriesName] = 1;
                }
            }

            // Select all series that have at least two stocks
            var doubleStocks = series.Where(s => doublesCount[GetSeriesName(s.Name)] > 1);
            var doubleSeries = doubleStocks.Select(s => s.Name)
                                           .Distinct();
            var stocksToExclude = new HashSet<string>();

            foreach (var seriesName in doubleSeries)
            {
                // Select series
                var company = doubleStocks.Where(s => s.Name.Contains(seriesName))
                                          .ToList();

                // Only include the one with the largest turnover. Exclude the rest.
                company.OrderByDescending(s => s.Turnover);
                for (int i = 0; i < company.Count(); i++)
                {
                    if (i == 0)
                    {
                        continue;
                    }
                    stocksToExclude.Add(company[i].Name);
                }
            }

            foreach (var stock in stocks)
            {
                if (stocksToExclude.Contains(stock.Name))
                {
                    stock.Exclude("Is a double");
                }
            }
        }

        private static string GetSeriesName(string fullName) => fullName.Substring(0, fullName.Length - 2);

        private static Stocks.Action DetermineAction(int index)
        {
            if (index <= 10)
            {
                return Stocks.Action.Buy;
            }
            else if (index <= 20)
            {
                return Stocks.Action.Keep;
            }
            else
            {
                return Stocks.Action.Sell;
            }
        }
    }
}
