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

            var number = _webElement.TextAsDecimal();

            number.Should().Be(1m);
        }

        [Fact]
        public void TextAsDecimal_NullText_ReturnsZero()
        {
            A.CallTo(() => _webElement.Text).Returns(null);

            var number = _webElement.TextAsDecimal();

            number.Should().Be(0m);
        }

        [Fact]
        public void TextAsDecimal_EmptyText_ReturnsZero()
        {
            A.CallTo(() => _webElement.Text).Returns(string.Empty);

            var number = _webElement.TextAsDecimal();

            number.Should().Be(0m);
        }

        [Fact]
        public void TextAsNumber_AnyKNumberText_ReturnsDecimal()
        {
            A.CallTo(() => _webElement.Text).Returns("1K");

            var number = _webElement.TextAsNumber();

            number.Should().Be(1000m);
        }

        [Fact]
        public void TextAsNumber_AnyNumberText_ReturnsDecimal()
        {
            A.CallTo(() => _webElement.Text).Returns("1");

            var number = _webElement.TextAsNumber();

            number.Should().Be(1m);
        }

        [Fact]
        public void TextAsNumber_AnyDecimalText_ReturnsDecimal()
        {
            A.CallTo(() => _webElement.Text).Returns("1,10");

            var number = _webElement.TextAsNumber();

            number.Should().Be(1.1m);
        }

        [Fact]
        public void TextAsNumber_NullText_ReturnsZero()
        {
            A.CallTo(() => _webElement.Text).Returns(null);

            var number = _webElement.TextAsNumber();

            number.Should().Be(0m);
        }

        [Fact]
        public void TextAsNumber_EmptyText_ReturnsZero()
        {
            A.CallTo(() => _webElement.Text).Returns(string.Empty);

            var number = _webElement.TextAsNumber();

            number.Should().Be(0m);
        }
    }
}
