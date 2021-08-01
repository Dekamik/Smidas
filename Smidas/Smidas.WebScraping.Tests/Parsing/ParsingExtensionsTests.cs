using FluentAssertions;
using Smidas.WebScraping.WebScrapers.Parsing;
using Xunit;

namespace Smidas.WebScraping.Tests.Parsing
{
    public class ParsingExtensionsTests
    {
        [Fact]
        public void ParsePercentage_PositivePercentage_ParsedAsDecimal()
        {
            const string str = "+1,00%";

            var result = str.ParsePercentage();

            result.Should().Be(1);
        }

        [Fact]
        public void ParsePercentage_NegativePercentage_ParsedAsDecimal()
        {
            const string str = "-1,00%";

            var result = str.ParsePercentage();

            result.Should().Be(-1);
        }

        [Fact]
        public void ParsePercentage_ZeroedPercentage_ParsedAsDecimal()
        {
            const string str = "0,00%";

            var result = str.ParsePercentage();

            result.Should().Be(0);
        }
    }
}