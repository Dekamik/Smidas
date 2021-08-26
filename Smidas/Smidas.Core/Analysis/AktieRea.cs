using Smidas.Core.Stocks;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Smidas.Common.Extensions;
using Microsoft.Extensions.Logging;
using Serilog.Events;
using Smidas.Common;
using Smidas.Common.Attributes;

namespace Smidas.Core.Analysis
{
    public class AktieRea : IAktieRea
    {
        private readonly ILogger<AktieRea> _logger;

        public AktieRea(ILogger<AktieRea> logger)
        {
            _logger = logger;
        }

        [StandardLogging]
        public IEnumerable<Stock> Analyze(AktieReaQuery query, IEnumerable<Stock> stocks)
        {
            ExcludeDisqualifiedStocks(ref stocks, query.AnalysisOptions);

            ExcludeDoubles(ref stocks);

            CalculateARank(ref stocks);

            CalculateBRank(ref stocks);

            DetermineActions(ref stocks, query);

            _logger.LogInformation($"Analysis complete\n" +
                                   $"\n" +
                                   $"Buy            : {stocks.Count(s => s.Action == Action.Buy)}\n" +
                                   $"Hold           : {stocks.Count(s => s.Action == Action.Hold)}\n" +
                                   $"Sell           : {stocks.Count(s => s.Action == Action.Sell)}\n" +
                                   $"Excluded       : {stocks.Count(s => s.Action == Action.Exclude)}\n" +
                                   $"\n" +
                                   $"Sum            : {stocks.Count()}\n");

            return stocks.OrderBy(s => s.AbRank)
                         .ThenByDescending(s => s.DirectDividend);
        }

        [StandardLogging(Level = LogEventLevel.Verbose)]
        public void ExcludeDisqualifiedStocks(ref IEnumerable<Stock> stocks, AktieReaQuery.AnalysisOptionsData options)
        {
            foreach (var stock in stocks)
            {
                if (options.ExcludeNegativeProfitStocks && stock.ProfitPerStock < 0m) // Stocks with negative profit per stock
                {
                    stock.Exclude(_logger, "Negativ vinst");
                }
                else if (options.ExcludeZeroDividendStocks && stock.DirectDividend == 0) // Stocks with zero direct dividend
                {
                    stock.Exclude(_logger, "Noll direktavkastning");
                }
                else if (options.ExcludePreferentialStocks && Regex.IsMatch(stock.Name, ".* Pref$")) // Preferential stocks
                {
                    stock.Exclude(_logger, "Preferensaktie");
                }
            }
        }

        [StandardLogging(Level = LogEventLevel.Verbose)]
        public void ExcludeDoubles(ref IEnumerable<Stock> stocks)
        {
            var doublesCount = new Dictionary<string, int>();
            var series = stocks.Where(s => Regex.IsMatch(s.Name, ".* [A-Z]$"));

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
                // Select the company's stocks, ordered by volume
                // Then keep the stock with the largest volume. Exclude the rest.
                doubleStocks.Where(s => s.Name.Contains(companyName))
                            .OrderByDescending(s => s.Volume)
                            .Skip(1)
                            .ForEach(c => stocksToExclude.Add(c.Name));
            }

            foreach (var stock in stocks)
            {
                if (stocksToExclude.Contains(stock.Name))
                {
                    stock.Exclude(_logger, "Dubblett");
                }
            }
        }

        [StandardLogging(Level = LogEventLevel.Verbose)]
        public void CalculateARank(ref IEnumerable<Stock> stocks)
        {
            var i = 1;
            stocks.OrderByDescending(s => s.Ep)
                  .ForEach(s => s.ARank = i++);
        }

        [StandardLogging(Level = LogEventLevel.Verbose)]
        public void CalculateBRank(ref IEnumerable<Stock> stocks)
        {
            var i = 1;
            stocks.OrderByDescending(s => s.AdjustedEquityPerStock)
                  .ForEach(s => s.BRank = i++);
        }

        [StandardLogging(Level = LogEventLevel.Verbose)]
        public void DetermineActions(ref IEnumerable<Stock> stocks, AktieReaQuery query)
        {
            var industryCap = new Dictionary<string, int> { { Stock.OtherIndustries, -1 } };
            foreach (var industry in query.Industries)
            {
                industryCap.Add(industry.Key, industry.Value.Cap);
            }

            var industryAmount = new Dictionary<string, int>();
            foreach (var industry in industryCap.Keys)
            {
                industryAmount.Add(industry, 0);
            }

            var i = 1;
            foreach (var stock in stocks.OrderBy(s => s.AbRank)
                                          .ThenByDescending(s => s.DirectDividend))
            {
                if (stock.Action == Action.Exclude)
                {
                    continue;
                }

                // Check if the stock belongs to a capped industry, and whether or not the cap has been reached.
                var cap = industryCap[stock.Industry];

                if (cap != -1 && industryAmount[stock.Industry] == cap)
                {
                    stock.Exclude(_logger, "Max antal bolag nådd inom branschen");
                    continue;
                }

                industryAmount[stock.Industry] += 1;

                // Determine action on stock
                stock.Action = i <= query.AmountToBuy ? Action.Buy :
                               i <= query.AmountToBuy + query.AmountToKeep ? Action.Hold :
                               Action.Sell;
                i++;
            }
        }
    }
}
