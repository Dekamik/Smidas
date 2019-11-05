using FluentAssertions;
using static FluentAssertions.FluentActions;
using Smidas.Core.Analysis;
using Smidas.Core.Stocks;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Smidas.Core.Tests.Analysis
{
    public class AktieReaTests
    {
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
            };
            var blacklist = new[] { "AnyBlacklistedStock" };

            AktieRea.ExcludeDisqualifiedStocks(ref stocks, blacklist);

            stocks[0].Action.Should().Be(Action.Exclude);
            stocks[0].Comments.Should().Be("Blacklisted.");
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
            };
            var blacklist = new[] { "AnyBlacklistedStock" };

            AktieRea.ExcludeDisqualifiedStocks(ref stocks, blacklist);

            stocks[0].Action.Should().NotBe(Action.Exclude);
            stocks[0].Comments.Should().NotBe("Blacklisted.");
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
            };

            AktieRea.ExcludeDisqualifiedStocks(ref stocks, new[] { "" });

            stocks[0].Action.Should().Be(Action.Exclude);
            stocks[0].Comments.Should().Be("Negative profit per stock.");
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
            };

            AktieRea.ExcludeDisqualifiedStocks(ref stocks, new[] { "" });

            stocks[0].Action.Should().NotBe(Action.Exclude);
            stocks[0].Comments.Should().NotBe("Negative profit per stock.");
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
            };

            AktieRea.ExcludeDisqualifiedStocks(ref stocks, new[] { "" });

            stocks[0].Action.Should().Be(Action.Exclude);
            stocks[0].Comments.Should().Be("Zero direct yield.");
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
            };

            AktieRea.ExcludeDisqualifiedStocks(ref stocks, new[] { "" });

            stocks[0].Action.Should().NotBe(Action.Exclude);
            stocks[0].Comments.Should().NotBe("Zero direct yield.");
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
            };

            AktieRea.ExcludeDisqualifiedStocks(ref stocks, new[] { "" });

            stocks[0].Action.Should().Be(Action.Exclude);
            stocks[0].Comments.Should().Be("Preferred stock.");
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
            };

            AktieRea.ExcludeDisqualifiedStocks(ref stocks, new[] { "" });

            stocks[0].Action.Should().NotBe(Action.Exclude);
            stocks[0].Comments.Should().NotBe("Preferred stock.");
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
            };

            AktieRea.ExcludeDoubles(ref stocks);

            stocks.Where(s => s.Name == "AnyStock A" || s.Name == "AnyStock C")
                  .ToList()
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
            };

            AktieRea.ExcludeDoubles(ref stocks);

            stocks.ToList().ForEach(s => s.Action.Should().Be(Action.Undetermined));
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
            };

            AktieRea.ExcludeDoubles(ref stocks);

            stocks.Where(s => s.Name == "AnyStock A" || s.Name == "OtherStock B" || s.Name == "DifferentStock")
                  .ToList()
                  .ForEach(s => s.Action.Should().Be(Action.Undetermined));
            stocks.Single(s => s.Name == "OtherStock R").Action.Should().Be(Action.Exclude);
        }

        [Fact]
        public void GetCompanyName_AnyStockName_TrimsSeriesFromStockName()
        {
            var companyName = AktieRea.GetCompanyName("AnyStock A");
            companyName.Should().Be("AnyStock");
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
            };

            AktieRea.CalculateARank(ref stocks);

            stocks.Should().BeInDescendingOrder(s => s.Ep);
            stocks.Should().BeInAscendingOrder(s => s.ARank);
        }

        [Fact]
        public void CalculateBRank_AnyStockCollection_StocksRankedByJekPerStock()
        {
            var stocks = new List<Stock>
            {
                new Stock
                {
                    Name = "Third",
                    JekPerStock = 6m,
                },
                new Stock
                {
                    Name = "Second",
                    JekPerStock = 12m,
                },
                new Stock
                {
                    Name = "First",
                    JekPerStock = 24m,
                },
                new Stock
                {
                    Name = "Fourth",
                    JekPerStock = 3m,
                },
            };

            AktieRea.CalculateBRank(ref stocks);

            stocks.Should().BeInDescendingOrder(s => s.JekPerStock);
            stocks.Should().BeInAscendingOrder(s => s.BRank);
        }

        [Fact]
        public void DetermineActionByIndex_IndexOne_StockMarkedBuy()
        {
            var action = AktieRea.DetermineActionByIndex(1);
            action.Should().Be(Action.Buy);
        }

        [Fact]
        public void DetermineActionByIndex_IndexEleven_StockMarkedKeep()
        {
            var action = AktieRea.DetermineActionByIndex(11);
            action.Should().Be(Action.Keep);
        }

        [Fact]
        public void DetermineActionByIndex_IndexTwentyOne_StockMarkedSell()
        {
            var action = AktieRea.DetermineActionByIndex(21);
            action.Should().Be(Action.Sell);
        }

        [Fact]
        public void DetermineActionByIndex_InvalidIndexMinusOne_ThrowsArgumentOutOfRangeException()
        {
            Invoking(() => AktieRea.DetermineActionByIndex(-1)).Should().Throw<System.ArgumentOutOfRangeException>();
        }
    }
}
