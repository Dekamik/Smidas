using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Text;

namespace Smidas.WebScraping.Extensions
{
    public static class IWebElementExtensions
    {
        public static decimal TextAsDecimal(this IWebElement webElement) => decimal.Parse(webElement.Text.Replace(',', '.'));

        public static decimal TextAsKNumber(this IWebElement webElement) => decimal.Parse(webElement.Text.Split(',')[0].Replace("K", "000"));
    }
}
