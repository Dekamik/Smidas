using FluentAssertions;
using Smidas.Core.Analysis;
using Smidas.Core.Stocks;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Smidas.Common.Extensions;
using Microsoft.Extensions.Logging;
using FakeItEasy;
using Smidas.Common;
using Microsoft.Extensions.Options;
using Smidas.API;

namespace Smidas.Core.Tests.Analysis
{
    // TODO: Fixa tester
    public class AktieReaTests
    {
        private readonly AktieRea _aktieRea;

        public AktieReaTests()
        {
            var logger = A.Fake<ILogger<AktieRea>>();
            var config = A.Fake<IOptions<AppSettings>>();

            A.CallTo(() => config.Value).Returns(new AppSettings
            {
                ScrapingSets = new Dictionary<string, AppSettings.AktieReaLocalQuery>
                {
                    {
                        "TestIndex",
                        new AppSettings.AktieReaLocalQuery
                        {
                            AmountToBuy = 10,
                            AmountToKeep = 10,
                            CurrencyCode = "ANY",
                            Industries = new Dictionary<string, AppSettings.AktieReaLocalQuery.IndustryData>
                            {
                                {
                                    "AnyIndustry",
                                    new AppSettings.AktieReaLocalQuery.IndustryData
                                    {
                                        Cap = 2,
                                        Companies = new []
                                        {
                                            "AnyIndustryComp1",
                                            "AnyIndustryComp2",
                                            "AnyIndustryComp3",
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            });

            _aktieRea = new AktieRea(logger);
        }

        [Fact]
        public void ExcludeDisqualifiedStocks_NegativeProfitPerStock_StockIsExcluded()
        {
            IEnumerable<Stock> stocks = new List<Stock>
            {
                new Stock
                {
                    Name = "AnyStock",
                    ProfitPerStock = -1m,
                    DirectDividend = 1m,
                }
            }.AsEnumerable();

            var options = new AktieReaQuery.AnalysisOptionsData
            {
                ExcludeNegativeProfitStocks = true
            };

            _aktieRea.ExcludeDisqualifiedStocks(ref stocks, options);

            stocks.Single().Action.Should().Be(Action.Exclude);
            stocks.Single().Comments.Should().Be("Negativ vinst");
        }

        [Fact]
        public void ExcludeDisqualifiedStocks_PositiveProfitPerStock_StockIsntExcluded()
        {
            IEnumerable<Stock> stocks = new List<Stock>
            {
                new Stock
                {
                    Name = "AnyStock",
                    ProfitPerStock = 1m,
                    DirectDividend = 1m,
                }
            }.AsEnumerable();

            var options = new AktieReaQuery.AnalysisOptionsData
            {
                ExcludeNegativeProfitStocks = true
            };

            _aktieRea.ExcludeDisqualifiedStocks(ref stocks, options);

            stocks.Single().Action.Should().NotBe(Action.Exclude);
            stocks.Single().Comments.Should().NotBe("Negativ vinst");
        }

        [Fact]
        public void ExcludeDisqualifiedStocks_ZeroDirectDividend_StockIsExcluded()
        {
            IEnumerable<Stock> stocks = new List<Stock>
            {
                new Stock
                {
                    Name = "AnyStock",
                    ProfitPerStock = 1m,
                    DirectDividend = 0m,
                }
            }.AsEnumerable();

            var options = new AktieReaQuery.AnalysisOptionsData
            {
                ExcludeZeroDividendStocks = true
            };

            _aktieRea.ExcludeDisqualifiedStocks(ref stocks, options);

            stocks.Single().Action.Should().Be(Action.Exclude);
            stocks.Single().Comments.Should().Be("Noll direktavkastning");
        }

        [Fact]
        public void ExcludeDisqualifiedStocks_PositiveDirectDividend_StockIsntExcluded()
        {
            IEnumerable<Stock> stocks = new List<Stock>
            {
                new Stock
                {
                    Name = "AnyStock",
                    ProfitPerStock = 1m,
                    DirectDividend = 1m,
                }
            }.AsEnumerable();

            var options = new AktieReaQuery.AnalysisOptionsData
            {
                ExcludeZeroDividendStocks = true
            };

            _aktieRea.ExcludeDisqualifiedStocks(ref stocks, options);

            stocks.Single().Action.Should().NotBe(Action.Exclude);
            stocks.Single().Comments.Should().NotBe("Noll direktavkastning");
        }

        [Fact]
        public void ExcludeDisqualifiedStocks_PreferredStock_StockIsExcluded()
        {
            IEnumerable<Stock> stocks = new List<Stock>
            {
                new Stock
                {
                    Name = "AnyStock Pref",
                    ProfitPerStock = 0m,
                    DirectDividend = 1m,
                }
            }.AsEnumerable();

            var options = new AktieReaQuery.AnalysisOptionsData
            {
                ExcludePreferentialStocks = true
            };

            _aktieRea.ExcludeDisqualifiedStocks(ref stocks, options);

            stocks.Single().Action.Should().Be(Action.Exclude);
            stocks.Single().Comments.Should().Be("Preferensaktie");
        }

        [Fact]
        public void ExcludeDisqualifiedStocks_NonPreferredStock_StockIsntExcluded()
        {
            IEnumerable<Stock> stocks = new List<Stock>
            {
                new Stock
                {
                    Name = "AnyStock",
                    ProfitPerStock = 0m,
                    DirectDividend = 1m,
                }
            }.AsEnumerable();

            var options = new AktieReaQuery.AnalysisOptionsData
            {
                ExcludePreferentialStocks = true
            };

            _aktieRea.ExcludeDisqualifiedStocks(ref stocks, options);

            stocks.Single().Action.Should().NotBe(Action.Exclude);
            stocks.Single().Comments.Should().NotBe("Preferensaktie");
        }

        [Fact]
        public void ExcludeDoubles_CompanyWithDoubles_AllStocksExcludedExceptStockWithHighestTurnover()
        {
            IEnumerable<Stock> stocks = new List<Stock>
            {
                new Stock
                {
                    Name = "AnyStock A",
                    Volume = 2,
                },
                new Stock
                {
                    Name = "AnyStock B",
                    Volume = 3,
                },
                new Stock
                {
                    Name = "AnyStock C",
                    Volume = 1,
                }
            }.AsEnumerable();

            _aktieRea.ExcludeDoubles(ref stocks);

            stocks.Where(s => s.Name == "AnyStock A" || s.Name == "AnyStock C")
                  .ForEach(s => s.Action.Should().Be(Action.Exclude));
            stocks.Single(s => s.Name == "AnyStock B").Action.Should().Be(Action.Undetermined);
        }

        [Fact]
        public void ExcludeDoubles_NoDoubles_NoStocksExcluded()
        {
            IEnumerable<Stock> stocks = new List<Stock>
            {
                new Stock
                {
                    Name = "AnyStock A",
                    Volume = 2,
                },
                new Stock
                {
                    Name = "OtherStock B",
                    Volume = 3,
                },
            }.AsEnumerable();

            _aktieRea.ExcludeDoubles(ref stocks);

            stocks.ForEach(s => s.Action.Should().Be(Action.Undetermined));
        }

        [Fact]
        public void ExcludeDoubles_MixedCollection_OnlyDoublesExcluded()
        {
            IEnumerable<Stock> stocks = new List<Stock>
            {
                new Stock
                {
                    Name = "AnyStock A",
                    Volume = 2,
                },
                new Stock
                {
                    Name = "OtherStock B",
                    Volume = 3,
                },
                new Stock
                {
                    Name = "OtherStock R",
                    Volume = 1,
                },
                new Stock
                {
                    Name = "DifferentStock",
                    Volume = 1,
                }
            }.AsEnumerable();

            _aktieRea.ExcludeDoubles(ref stocks);

            stocks.Where(s => s.Name == "AnyStock A" || s.Name == "OtherStock B" || s.Name == "DifferentStock")
                  .ForEach(s => s.Action.Should().Be(Action.Undetermined));
            stocks.Single(s => s.Name == "OtherStock R").Action.Should().Be(Action.Exclude);
        }

        [Fact]
        public void CalculateARank_AnyStockCollection_StocksRankedByEp()
        {
            IEnumerable<Stock> stocks = new List<Stock>
            {
                new Stock
                {
                    Name = "Third",
                    Price = 3m,
                    ProfitPerStock = 10m,
                },
                new Stock
                {
                    Name = "Second",
                    Price = 2m,
                    ProfitPerStock = 10m,
                },
                new Stock
                {
                    Name = "First",
                    Price = 1m,
                    ProfitPerStock = 10m,
                },
                new Stock
                {
                    Name = "Fourth",
                    Price = 4m,
                    ProfitPerStock = 10m,
                },
            }.AsEnumerable();

            _aktieRea.CalculateARank(ref stocks);

            stocks.OrderBy(s => s.ARank).Should().BeInDescendingOrder(s => s.Ep);
            stocks.OrderByDescending(s => s.Ep).Should().BeInAscendingOrder(s => s.ARank);
        }

        [Fact]
        public void CalculateBRank_AnyStockCollection_StocksRankedByAdjustedEquityPerStock()
        {
            IEnumerable<Stock> stocks = new List<Stock>
            {
                new Stock
                {
                    Name = "Third",
                    AdjustedEquityPerStock = 6m,
                },
                new Stock
                {
                    Name = "Second",
                    AdjustedEquityPerStock = 12m,
                },
                new Stock
                {
                    Name = "First",
                    AdjustedEquityPerStock = 24m,
                },
                new Stock
                {
                    Name = "Fourth",
                    AdjustedEquityPerStock = 3m,
                },
            }.AsEnumerable();

            _aktieRea.CalculateBRank(ref stocks);

            stocks.OrderBy(s => s.BRank).Should().BeInDescendingOrder(s => s.AdjustedEquityPerStock);
            stocks.OrderByDescending(s => s.AdjustedEquityPerStock).Should().BeInAscendingOrder(s => s.BRank);
        }

        [Fact]
        public void DetermineActions_AllIncludedStocks_OrderedByAbRank()
        {
            List<Stock> stocks = new List<Stock>();

            for (int i = 1; i <= 30; i++)
            {
                stocks.Add(new Stock
                {
                    Name = i.ToString(),
                    ARank = i,
                    BRank = i,
                });
            }

            // Shuffle list
            var enumerable = stocks.OrderBy(s => System.Guid.NewGuid())
                                   .AsEnumerable();

            var query = new AktieReaQuery
            {
                AmountToBuy = 10,
                AmountToKeep = 10,
                Industries = new Dictionary<string, AktieReaQuery.IndustryData>()
            };

            _aktieRea.DetermineActions(ref enumerable, query);

            stocks.Should().BeInAscendingOrder(s => s.AbRank);
        }

        [Fact]
        public void DetermineActions_AllIncludedStocks_RankingHasProperActions()
        {
            List<Stock> stocks = new List<Stock>();

            for (int i = 1; i <= 30; i++)
            {
                stocks.Add(new Stock
                {
                    Name = i.ToString(),
                    ARank = i,
                    BRank = i,
                });
            }
            
            // Shuffle list
            var enumerable = stocks.OrderBy(s => System.Guid.NewGuid())
                                   .AsEnumerable();

            var query = new AktieReaQuery
            {
                AmountToBuy = 10,
                AmountToKeep = 10,
                Industries = new Dictionary<string, AktieReaQuery.IndustryData>()
            };

            _aktieRea.DetermineActions(ref enumerable, query);

            // Assert that 1 - 10 = Buy, 11 - 20 = Keep, 21> = Sell
            stocks.Take(10)
                  .ForEach(s => s.Action.Should().Be(Action.Buy));
            stocks.Skip(10)
                  .Take(10)
                  .ForEach(s => s.Action.Should().Be(Action.Hold));
            stocks.Skip(20)
                  .Take(10)
                  .ForEach(s => s.Action.Should().Be(Action.Sell));
        }

        [Fact]
        public void DetermineActions_ThreeStocksInAnyIndustryTwoAllowed_TheTwoBestAreMarkedBuyTheThirdIsExcluded()
        {
            List<Stock> stocks = new List<Stock>();

            for (int i = 1; i <= 3; i++)
            {
                stocks.Add(new Stock
                {
                    Name = $"AnyIndustryComp{i}",
                    Industry = "AnyIndustry",
                    ARank = i,
                    BRank = i,
                });
            }

            // Shuffle list
            var enumerable = stocks.OrderBy(s => System.Guid.NewGuid())
                                   .AsEnumerable();

            var query = new AktieReaQuery
            {
                AmountToBuy = 10,
                AmountToKeep = 10,
                Industries = new Dictionary<string, AktieReaQuery.IndustryData>
                {
                    { 
                        "AnyIndustry", 
                        new AktieReaQuery.IndustryData 
                        {
                            Cap = 2, 
                            Companies = new string[]
                            {
                                "AnyIndustryComp1",
                                "AnyIndustryComp2",
                                "AnyIndustryComp3"
                            }
                        } 
                    }
                }
            };

            _aktieRea.DetermineActions(ref enumerable, query);

            stocks.Should().BeInAscendingOrder(s => s.AbRank);
            stocks.Take(2)
                  .ForEach(s => s.Action.Should().Be(Action.Buy));
            stocks.Last().Action.Should().Be(Action.Exclude);
        }
    }
}
