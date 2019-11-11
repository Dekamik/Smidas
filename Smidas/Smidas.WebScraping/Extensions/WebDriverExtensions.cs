using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Text;

namespace Smidas.WebScraping.Extensions
{
    public static class WebDriverExtensions
    {
        public static object ExecuteScript(this IWebDriver driver, string script, params object[] args) => (driver as IJavaScriptExecutor).ExecuteScript(script, args);

        public static decimal TextAsDecimal(this IWebElement webElement) => decimal.Parse(!string.IsNullOrEmpty(webElement.Text) ? webElement.Text : "0");

        public static decimal TextAsNumber(this IWebElement webElement) => decimal.Parse(!string.IsNullOrEmpty(webElement.Text) ? webElement.Text.Replace("K", "000") : "0");
    }
}
