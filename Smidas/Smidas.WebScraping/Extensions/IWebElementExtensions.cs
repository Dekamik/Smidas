using OpenQA.Selenium;

namespace Smidas.WebScraping.Extensions
{
    public static class IWebElementExtensions
    {
        public static decimal TextAsDecimal(this IWebElement webElement) => decimal.Parse(!string.IsNullOrEmpty(webElement.Text) ? webElement.Text : "0");

        public static decimal TextAsNumber(this IWebElement webElement) => decimal.Parse(!string.IsNullOrEmpty(webElement.Text) ? webElement.Text.Replace("K", "000") : "0");
    }
}
