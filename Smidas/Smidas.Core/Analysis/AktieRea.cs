using Smidas.Core.Stocks;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Smidas.Common.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Smidas.Common;
using Smidas.Common.StockIndices;

namespace Smidas.Core.Analysis
{
    public class AktieRea : IAnalysis
    {
        private readonly ILogger logger;
        private readonly IOptions<AppSettings> options;

        private int amountToBuy;
        private int amountToKeep;

        private IDictionary<string, int> industryCap;
        private StockIndex index;

        public StockIndex Index
        {
            get => index;
            set
            {
                index = value;

                amountToBuy = options.Value.AktieRea[index.ToString()].AmountToBuy;
                amountToKeep = options.Value.AktieRea[index.ToString()].AmountToKeep;

                industryCap = new Dictionary<string, int>();
                industryCap.Add(Stock.OtherIndustries, -1);
                foreach (KeyValuePair<string, AppSettings.IndexSettings.IndustryData> industry in options.Value.AktieRea[index.ToString()].Industries)
                {
                    industryCap.Add(industry.Key, industry.Value.Cap);
                }
            }
        }

        public AktieRea(ILoggerFactory loggerFactory, IOptions<AppSettings> options)
        {
            logger = loggerFactory.CreateLogger<AktieRea>();
            this.options = options;
        }

        public IEnumerable<Stock> Analyze(IEnumerable<Stock> stocks)
        {
            logger.LogInformation($"Analyserar {stocks.Count()} aktier enligt AktieREA-metoden");

            ExcludeDisqualifiedStocks(ref stocks);

            ExcludeDoubles(ref stocks);

            CalculateARank(ref stocks);

            CalculateBRank(ref stocks);

            DetermineActions(ref stocks);

            logger.LogInformation($"Analys slutförd\n" +
                                   $"\n" +
                                   $"Köpes          : {stocks.Count(s => s.Action == Action.Buy)}\n" +
                                   $"Behålles       : {stocks.Count(s => s.Action == Action.Keep)}\n" +
                                   $"Säljes         : {stocks.Count(s => s.Action == Action.Sell)}\n" +
                                   $"Bortsållade    : {stocks.Count(s => s.Action == Action.Exclude)}\n" +
                                   $"\n" +
                                   $"S:a            : {stocks.Count()}\n");

            return stocks.OrderBy(s => s.AbRank)
                         .ThenByDescending(s => s.DirectYield);
        }

        public void ExcludeDisqualifiedStocks(ref IEnumerable<Stock> stocks)
        {
            logger.LogDebug($"Sållar diskvalivicerade aktier");

            foreach (Stock stock in stocks)
            {
                if (stock.ProfitPerStock < 0m) // Stocks with negative profit per stock
                {
                    stock.Exclude(logger, "Negativ vinst");
                }
                else if (stock.DirectYield == 0) // Stocks with zero direct yield
                {
                    stock.Exclude(logger, "Noll direktavkastning");
                }
                else if (Regex.IsMatch(stock.Name, ".* Pref$")) // Preferential stocks
                {
                    stock.Exclude(logger, "Preferensaktie");
                }
            }
        }

        public void ExcludeDoubles(ref IEnumerable<Stock> stocks)
        {
            logger.LogDebug($"Sållar dubbletter");

            Dictionary<string, int> doublesCount = new Dictionary<string, int>();
            var series = stocks.Where(s => Regex.IsMatch(s.Name, ".* [A-Z]$"));

            foreach (Stock stock in series) // Count amount of doubles per series
            {
                doublesCount[stock.CompanyName] = doublesCount.ContainsKey(stock.CompanyName) ? doublesCount[stock.CompanyName] + 1 : 1;
            }

            // Select all series that have at least two stocks
            var doubleStocks = series.Where(s => doublesCount[s.CompanyName] > 1);
            var doubleCompanies = doubleStocks.Select(s => s.CompanyName)
                                              .Distinct();
            HashSet<string> stocksToExclude = new HashSet<string>();

            foreach (string companyName in doubleCompanies)
            {
                // Select the company's stocks, ordered by turnover
                // Then keep the stock with the largest turnover (the first one). Exclude the rest.
                doubleStocks.Where(s => s.Name.Contains(companyName))
                            .OrderByDescending(s => s.Volume)
                            .Skip(1)
                            .ForEach(c => stocksToExclude.Add(c.Name));
            }

            foreach (Stock stock in stocks)
            {
                if (stocksToExclude.Contains(stock.Name))
                {
                    stock.Exclude(logger, "Dubblett");
                }
            }
        }

        public void CalculateARank(ref IEnumerable<Stock> stocks)
        {
            logger.LogDebug($"Räknar ut A-rang");

            int i = 1;
            stocks.OrderByDescending(s => s.Ep)
                  .ForEach(s => s.ARank = i++);
        }

        public void CalculateBRank(ref IEnumerable<Stock> stocks)
        {
            logger.LogDebug($"Räknar ut B-rang");

            int i = 1;
            stocks.OrderByDescending(s => s.AdjustedEquityPerStock)
                  .ForEach(s => s.BRank = i++);
        }

        public void DetermineActions(ref IEnumerable<Stock> stocks)
        {
            logger.LogDebug($"Beslutar åtgärder");

            int index = 1;
            Dictionary<string, int> industryAmount = new Dictionary<string, int>();

            foreach (string industry in industryCap.Keys)
            {
                industryAmount.Add(industry, 0);
            }

            foreach (Stock stock in stocks.OrderBy(s => s.AbRank)
                                        .ThenByDescending(s => s.DirectYield))
            {
                if (stock.Action == Action.Exclude)
                {
                    continue;
                }

                // Check if the stock belongs to a capped industry, and whether or not the cap has been reached.
                int cap = industryCap[stock.Industry];

                if (cap != -1 && industryAmount[stock.Industry] == cap)
                {
                    stock.Exclude(logger, "Max antal bolag nådd inom branschen");
                    continue;
                }

                industryAmount[stock.Industry.ToString()] += 1;

                // Determine action on stock
                stock.Action = index <= amountToBuy ? Action.Buy :
                               index <= amountToBuy + amountToKeep ? Action.Keep :
                               Action.Sell;
                index++;
            }
        }
    }
}
