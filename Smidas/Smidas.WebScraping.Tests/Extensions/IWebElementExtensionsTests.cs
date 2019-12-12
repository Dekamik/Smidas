using FakeItEasy;
using FluentAssertions;
using OpenQA.Selenium;
using Smidas.WebScraping.Extensions;
using Xunit;

namespace Smidas.WebScraping.Tests.Extensions
{
    public class IWebElementExtensionsTests
    {
        private IWebElement _webElement;

        public IWebElementExtensionsTests()
        {
            _webElement = A.Fake<IWebElement>();
        }

        [Fact]
        public void TextAsDecimal_AnyDecimalText_ReturnsDecimal()
        {
            A.CallTo(() => _webElement.Text).Returns("1,00");

            decimal number = _webElement.DecimalTextAsDecimal();

            number.Should().Be(1m);
        }

        [Fact]
        public void TextAsDecimal_NullText_ReturnsZero()
        {
            A.CallTo(() => _webElement.Text).Returns(null);

            decimal number = _webElement.DecimalTextAsDecimal();

            number.Should().Be(0m);
        }

        [Fact]
        public void TextAsDecimal_EmptyText_ReturnsZero()
        {
            A.CallTo(() => _webElement.Text).Returns(string.Empty);

            decimal number = _webElement.DecimalTextAsDecimal();

            number.Should().Be(0m);
        }

        [Fact]
        public void TextAsNumber_AnyKNumberText_ReturnsDecimal()
        {
            A.CallTo(() => _webElement.Text).Returns("1K");

            decimal number = _webElement.NumberTextAsDecimal();

            number.Should().Be(1000m);
        }

        [Fact]
        public void TextAsNumber_AnyNumberText_ReturnsDecimal()
        {
            A.CallTo(() => _webElement.Text).Returns("1");

            decimal number = _webElement.NumberTextAsDecimal();

            number.Should().Be(1m);
        }

        [Fact]
        public void TextAsNumber_AnyDecimalText_ReturnsDecimal()
        {
            A.CallTo(() => _webElement.Text).Returns("1,10");

            decimal number = _webElement.NumberTextAsDecimal();

            number.Should().Be(1.1m);
        }

        [Fact]
        public void TextAsNumber_NullText_ReturnsZero()
        {
            A.CallTo(() => _webElement.Text).Returns(null);

            decimal number = _webElement.NumberTextAsDecimal();

            number.Should().Be(0m);
        }

        [Fact]
        public void TextAsNumber_EmptyText_ReturnsZero()
        {
            A.CallTo(() => _webElement.Text).Returns(string.Empty);

            decimal number = _webElement.NumberTextAsDecimal();

            number.Should().Be(0m);
        }
    }
}
