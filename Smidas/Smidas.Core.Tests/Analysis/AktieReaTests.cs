using FluentAssertions;
using static FluentAssertions.FluentActions;
using Smidas.Core.Analysis;
using Smidas.Core.Stocks;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Smidas.Common.Extensions;
using Microsoft.Extensions.Logging;
using FakeItEasy;
using Smidas.Common;

namespace Smidas.Core.Tests.Analysis
{
    public class AktieReaTests
    {
        private readonly AktieRea _aktieRea;

        public AktieReaTests()
        {
            var loggerFactory = A.Fake<ILoggerFactory>();
            var config = A.Fake<AppSettings>();

            _aktieRea = new AktieRea(loggerFactory, config);
        }

        [Fact]
        public void ExcludeDisqualifiedStocks_Blacklisted_StockIsExcluded()
        {
            var stocks = new List<Stock>
            {
                new Stock
                {
                    Name = "AnyBlacklistedStock",
                    ProfitPerStock = 1m,
                    DirectYield = 1m,
                }
            }.AsEnumerable();
            var blacklist = new[] { "AnyBlacklistedStock" };

            _aktieRea.ExcludeDisqualifiedStocks(ref stocks, blacklist);

            stocks.Single().Action.Should().Be(Action.Exclude);
            stocks.Single().Comments.Should().Be("Blacklisted.");
        }

        [Fact]
        public void ExcludeDisqualifiedStocks_BlacklistedWithSeries_StockIsExcluded()
        {
            var stocks = new List<Stock>
            {
                new Stock
                {
                    Name = "AnyBlacklistedStock A",
                    ProfitPerStock = 1m,
                    DirectYield = 1m,
                }
            }.AsEnumerable();
            var blacklist = new[] { "AnyBlacklistedStock" };

            _aktieRea.ExcludeDisqualifiedStocks(ref stocks, blacklist);

            stocks.Single().Action.Should().Be(Action.Exclude);
            stocks.Single().Comments.Should().Be("Blacklisted.");
        }

        [Fact]
        public void ExcludeDisqualifiedStocks_NotBlacklisted_StockIsntExcluded()
        {
            var stocks = new List<Stock>
            {
                new Stock
                {
                    Name = "AnyStock",
                    ProfitPerStock = 1m,
                    DirectYield = 1m,
                }
            }.AsEnumerable();
            var blacklist = new[] { "AnyBlacklistedStock" };

            _aktieRea.ExcludeDisqualifiedStocks(ref stocks, blacklist);

            stocks.Single().Action.Should().NotBe(Action.Exclude);
            stocks.Single().Comments.Should().NotBe("Blacklisted.");
        }

        [Fact]
        public void ExcludeDisqualifiedStocks_NegativeProfitPerStock_StockIsExcluded()
        {
            var stocks = new List<Stock>
            {
                new Stock
                {
                    Name = "AnyStock",
                    ProfitPerStock = -1m,
                    DirectYield = 1m,
                }
            }.AsEnumerable();

            _aktieRea.ExcludeDisqualifiedStocks(ref stocks, new List<string>());

            stocks.Single().Action.Should().Be(Action.Exclude);
            stocks.Single().Comments.Should().Be("Negative profit per stock.");
        }

        [Fact]
        public void ExcludeDisqualifiedStocks_PositiveProfitPerStock_StockIsntExcluded()
        {
            var stocks = new List<Stock>
            {
                new Stock
                {
                    Name = "AnyStock",
                    ProfitPerStock = 1m,
                    DirectYield = 1m,
                }
            }.AsEnumerable();

            _aktieRea.ExcludeDisqualifiedStocks(ref stocks, new List<string>());

            stocks.Single().Action.Should().NotBe(Action.Exclude);
            stocks.Single().Comments.Should().NotBe("Negative profit per stock.");
        }

        [Fact]
        public void ExcludeDisqualifiedStocks_ZeroDirectYield_StockIsExcluded()
        {
            var stocks = new List<Stock>
            {
                new Stock
                {
                    Name = "AnyStock",
                    ProfitPerStock = 1m,
                    DirectYield = 0m,
                }
            }.AsEnumerable();

            _aktieRea.ExcludeDisqualifiedStocks(ref stocks, new List<string>());

            stocks.Single().Action.Should().Be(Action.Exclude);
            stocks.Single().Comments.Should().Be("Zero direct yield.");
        }

        [Fact]
        public void ExcludeDisqualifiedStocks_PositiveDirectYield_StockIsntExcluded()
        {
            var stocks = new List<Stock>
            {
                new Stock
                {
                    Name = "AnyStock",
                    ProfitPerStock = 1m,
                    DirectYield = 1m,
                }
            }.AsEnumerable();

            _aktieRea.ExcludeDisqualifiedStocks(ref stocks, new List<string>());

            stocks.Single().Action.Should().NotBe(Action.Exclude);
            stocks.Single().Comments.Should().NotBe("Zero direct yield.");
        }

        [Fact]
        public void ExcludeDisqualifiedStocks_PreferredStock_StockIsExcluded()
        {
            var stocks = new List<Stock>
            {
                new Stock
                {
                    Name = "AnyStock Pref",
                    ProfitPerStock = 0m,
                    DirectYield = 1m,
                }
            }.AsEnumerable();

            _aktieRea.ExcludeDisqualifiedStocks(ref stocks, new List<string>());

            stocks.Single().Action.Should().Be(Action.Exclude);
            stocks.Single().Comments.Should().Be("Preferred stock.");
        }

        [Fact]
        public void ExcludeDisqualifiedStocks_NonPreferredStock_StockIsntExcluded()
        {
            var stocks = new List<Stock>
            {
                new Stock
                {
                    Name = "AnyStock",
                    ProfitPerStock = 0m,
                    DirectYield = 1m,
                }
            }.AsEnumerable();

            _aktieRea.ExcludeDisqualifiedStocks(ref stocks, new List<string>());

            stocks.Single().Action.Should().NotBe(Action.Exclude);
            stocks.Single().Comments.Should().NotBe("Preferred stock.");
        }

        [Fact]
        public void ExcludeDoubles_CompanyWithDoubles_AllStocksExcludedExceptStockWithHighestTurnover()
        {
            var stocks = new List<Stock>
            {
                new Stock
                {
                    Name = "AnyStock A",
                    Turnover = 2,
                },
                new Stock
                {
                    Name = "AnyStock B",
                    Turnover = 3,
                },
                new Stock
                {
                    Name = "AnyStock C",
                    Turnover = 1,
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
            var stocks = new List<Stock>
            {
                new Stock
                {
                    Name = "AnyStock A",
                    Turnover = 2,
                },
                new Stock
                {
                    Name = "OtherStock B",
                    Turnover = 3,
                },
            }.AsEnumerable();

            _aktieRea.ExcludeDoubles(ref stocks);

            stocks.ForEach(s => s.Action.Should().Be(Action.Undetermined));
        }

        [Fact]
        public void ExcludeDoubles_MixedCollection_OnlyDoublesExcluded()
        {
            var stocks = new List<Stock>
            {
                new Stock
                {
                    Name = "AnyStock A",
                    Turnover = 2,
                },
                new Stock
                {
                    Name = "OtherStock B",
                    Turnover = 3,
                },
                new Stock
                {
                    Name = "OtherStock R",
                    Turnover = 1,
                },
                new Stock
                {
                    Name = "DifferentStock",
                    Turnover = 1,
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
            var stocks = new List<Stock>
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
            var stocks = new List<Stock>
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
            var stocks = new List<Stock>();
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

            _aktieRea.DetermineActions(ref enumerable, 2, 2, 2);

            stocks.Should().BeInAscendingOrder(s => s.AbRank);
        }

        [Fact]
        public void DetermineActions_AllIncludedStocks_RankingHasProperActions()
        {
            var stocks = new List<Stock>();
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

            _aktieRea.DetermineActions(ref enumerable, 2, 2, 2);

            // Assert that 1 - 10 = Buy, 11 - 20 = Keep, 21> = Sell
            stocks.Take(10)
                  .ForEach(s => s.Action.Should().Be(Action.Buy));
            stocks.Skip(10)
                  .Take(10)
                  .ForEach(s => s.Action.Should().Be(Action.Keep));
            stocks.Skip(20)
                  .Take(10)
                  .ForEach(s => s.Action.Should().Be(Action.Sell));
        }

        [Fact]
        public void DetermineActions_ThreeInvestmentStocksTwoAllowed_TheTwoBestAreMarkedBuyTheThirdIsExcluded()
        {
            var stocks = new List<Stock>();
            for (int i = 1; i <= 30; i++)
            {
                stocks.Add(new Stock
                {
                    Name = i.ToString(),
                    Industry = Industry.Investment,
                    ARank = i,
                    BRank = i,
                });
            }

            // Shuffle list
            var enumerable = stocks.OrderBy(s => System.Guid.NewGuid())
                                   .AsEnumerable();

            _aktieRea.DetermineActions(ref enumerable, 2, 2, 2);

            stocks.Should().BeInAscendingOrder(s => s.AbRank);
            stocks.Take(2)
                  .ForEach(s => s.Action.Should().Be(Action.Buy));
            stocks.Last().Action.Should().Be(Action.Exclude);
        }

        [Fact]
        public void DetermineActions_ThreeRealEstateStocksTwoAllowed_TheTwoBestAreMarkedBuyTheThirdIsExcluded()
        {
            var stocks = new List<Stock>();
            for (int i = 1; i <= 30; i++)
            {
                stocks.Add(new Stock
                {
                    Name = i.ToString(),
                    Industry = Industry.RealEstate,
                    ARank = i,
                    BRank = i,
                });
            }

            // Shuffle list
            var enumerable = stocks.OrderBy(s => System.Guid.NewGuid())
                                   .AsEnumerable();

            _aktieRea.DetermineActions(ref enumerable, 2, 2, 2);

            stocks.Should().BeInAscendingOrder(s => s.AbRank);
            stocks.Take(2)
                  .ForEach(s => s.Action.Should().Be(Action.Buy));
            stocks.Last().Action.Should().Be(Action.Exclude);
        }

        [Fact]
        public void DetermineActions_ThreeBankingStocksTwoAllowed_TheTwoBestAreMarkedBuyTheThirdIsExcluded()
        {
            var stocks = new List<Stock>();
            for (int i = 1; i <= 30; i++)
            {
                stocks.Add(new Stock
                {
                    Name = i.ToString(),
                    Industry = Industry.Banking,
                    ARank = i,
                    BRank = i,
                });
            }

            // Shuffle list
            var enumerable = stocks.OrderBy(s => System.Guid.NewGuid())
                                   .AsEnumerable();

            _aktieRea.DetermineActions(ref enumerable, 2, 2, 2);

            stocks.Should().BeInAscendingOrder(s => s.AbRank);
            stocks.Take(2)
                  .ForEach(s => s.Action.Should().Be(Action.Buy));
            stocks.Last().Action.Should().Be(Action.Exclude);
        }
    }
}
