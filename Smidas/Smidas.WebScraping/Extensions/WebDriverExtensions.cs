using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Text;

namespace Smidas.WebScraping.Extensions
{
    public static class WebDriverExtensions
    {
        public static object ExecuteScript(this IWebDriver driver, string script, params object[] args) => (driver as IJavaScriptExecutor).ExecuteScript(script, args);

        public static decimal DecimalTextAsDecimal(this IWebElement webElement) => decimal.Parse(!string.IsNullOrEmpty(webElement.Text) ? webElement.Text : "0");

        public static decimal NumberTextAsDecimal(this IWebElement webElement) => decimal.Parse(!string.IsNullOrEmpty(webElement.Text) ? webElement.Text.Replace("K", "000") : "0");

        public static decimal PercentageTextAsDecimal(this IWebElement webElement) => decimal.Parse(!string.IsNullOrEmpty(webElement.Text) ? webElement.Text.Replace("%", "") : "0");

        public static decimal ParseDecimal(this string str) => decimal.Parse(!string.IsNullOrEmpty(str) ? str : "0");

        public static decimal ParseDecimalWithSymbol(this string str) => decimal.Parse(str[0..^1] ?? "0");
    }
}
