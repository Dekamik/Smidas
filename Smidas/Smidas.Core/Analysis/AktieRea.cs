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
        private readonly ILogger _logger;

        private readonly IOptions<AppSettings> _options;

        private int _amountToBuy;

        private int _amountToKeep;

        private IDictionary<string, int> _industryCap;

        private IEnumerable<string> _blacklist;

        private StockIndex _index;

        public StockIndex Index
        {
            get => _index;
            set
            {
                _index = value;

                _amountToBuy = _options.Value.AktieRea[_index.ToString()].AmountToBuy;
                _amountToKeep = _options.Value.AktieRea[_index.ToString()].AmountToKeep;

                _industryCap = new Dictionary<string, int>();
                _industryCap.Add(Stock.OtherIndustries, -1);
                foreach (var industry in _options.Value.AktieRea[_index.ToString()].Industries)
                {
                    _industryCap.Add(industry.Key, industry.Value.Cap);
                }

                _blacklist = _options.Value.AktieRea[_index.ToString()].Blacklist;
            }
        }

        public AktieRea(ILoggerFactory loggerFactory, IOptions<AppSettings> options)
        {
            _logger = loggerFactory.CreateLogger<AktieRea>();
            _options = options;
        }

        public IEnumerable<Stock> Analyze(IEnumerable<Stock> stocks)
        {
            _logger.LogInformation($"Analyserar {stocks.Count()} aktier enligt AktieREA-metoden");

            ExcludeDisqualifiedStocks(ref stocks);

            ExcludeDoubles(ref stocks);

            CalculateARank(ref stocks);

            CalculateBRank(ref stocks);

            DetermineActions(ref stocks);

            _logger.LogInformation($"Analys slutförd\n" +
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
            _logger.LogDebug($"Sållar diskvalivicerade aktier");

            foreach (var stock in stocks)
            {
                // Blacklisted stocks
                if (_blacklist != null)
                {
                    foreach (var name in _blacklist)
                    {
                        if (stock.Name.Contains(name))
                        {
                            _logger.LogTrace($"Sållade {stock.Name} - Svartlistad");
                            stock.Exclude("Svartlistad");
                        }
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
                                          .OrderByDescending(s => s.Volume);

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

        public void DetermineActions(ref IEnumerable<Stock> stocks)
        {
            _logger.LogDebug($"Beslutar åtgärder");

            var index = 1;
            var industryAmount = new Dictionary<string, int>();

            foreach (var industry in _industryCap.Keys)
            {
                industryAmount.Add(industry, 0);
            }

            foreach (var stock in stocks.OrderBy(s => s.AbRank)
                                        .ThenByDescending(s => s.DirectYield))
            {
                if (stock.Action == Action.Exclude)
                {
                    continue;
                }

                // Check if the stock belongs to a capped industry, and whether or not the cap has been reached.
                var cap = _industryCap[stock.Industry];

                if (cap != -1 && industryAmount[stock.Industry] == cap)
                {
                    _logger.LogTrace($"Sållade {stock.Name} - Max antal bolag nådd inom branschen");
                    stock.Exclude("Max antal bolag nådd inom branschen");
                    continue;
                }

                industryAmount[stock.Industry.ToString()]++;

                // Determine action on stock
                stock.Action = index <= _amountToBuy ? Action.Buy :
                               index <= _amountToBuy + _amountToKeep ? Action.Keep :
                               Action.Sell;
                index++;
            }
        }
    }
}
