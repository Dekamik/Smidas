using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Smidas.Core.Stocks;
using Xunit;

namespace Smidas.Core.Tests.Stocks
{
    public class StockTests
    {

        [Fact]
        public void CompanyName_NameWithSeries_ReturnsNameWithoutSeries()
        {
            Stock stock = new Stock
            {
                Name = "AnyStock A",
            };

            stock.CompanyName.Should().Be("AnyStock");
        }


        [Fact]
        public void CompanyName_NameWithoutSeries_ReturnsName()
        {
            Stock stock = new Stock
            {
                Name = "AnyStock",
            };

            stock.CompanyName.Should().Be("AnyStock");
        }


        [Fact]
        public void CompanyName_NameWithAb_ReturnsName()
        {
            Stock stock = new Stock
            {
                Name = "AnyStock AB",
            };

            stock.CompanyName.Should().Be("AnyStock AB");
        }


        [Fact]
        public void CompanyName_NameWithAbAndSeries_ReturnsNameWithAbAndWithoutSeries()
        {
            Stock stock = new Stock
            {
                Name = "AnyStock AB A",
            };

            stock.CompanyName.Should().Be("AnyStock AB");
        }

        [Fact]
        public void Ep_StockPriceTwoProfitPerStockTen_EpEqualsFive()
        {
            Stock stock = new Stock
            {
                Name = "AnyStock",
                Price = 2m,
                ProfitPerStock = 10m,
            };

            stock.Ep.Should().Be(5m);
        }

        [Fact]
        public void Ep_StockPriceThreeProfitPerStockTwelve_EpEqualsFour()
        {
            Stock stock = new Stock
            {
                Name = "AnyStock",
                Price = 3m,
                ProfitPerStock = 12m,
            };

            stock.Ep.Should().Be(4m);
        }

        [Fact]
        public void Ep_StockPriceZeroProfitPerStockOne_EpEqualsZero()
        {
            Stock stock = new Stock
            {
                Name = "AnyStock",
                Price = 0m,
                ProfitPerStock = 1m,
            };

            stock.Ep.Should().Be(0m);
        }

        [Fact]
        public void AbRank_ARankOneBRankOne_AbRankTwo()
        {
            Stock stock = new Stock
            {
                Name = "AnyStock",
                ARank = 1,
                BRank = 1,
            };

            stock.AbRank.Should().Be(2);
        }

        [Fact]
        public void AbRank_ARankOneBRankTwo_AbRankThree()
        {
            Stock stock = new Stock
            {
                Name = "AnyStock",
                ARank = 1,
                BRank = 2,
            };

            stock.AbRank.Should().Be(3);
        }

        [Fact]
        public void AbRank_ARankOneBRankOneActionIsExclude_AbRankTenThousandAndTwo()
        {
            Stock stock = new Stock
            {
                Name = "AnyStock",
                Action = Action.Exclude,
                ARank = 1,
                BRank = 1,
            };

            stock.AbRank.Should().Be(10002);
        }

        [Fact]
        public void Exclude_AnyStock_StockIsExcludedWithComments()
        {
            Stock stock = new Stock
            {
                Name = "AnyStock"
            };

            stock.Exclude(A.Fake<ILogger>(), "AnyReason");

            stock.Action.Should().Be(Action.Exclude);
            stock.Comments.Should().Be("AnyReason");
        }
    }
}
