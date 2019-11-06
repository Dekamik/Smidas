using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Text;

namespace Smidas.WebScraping.Extensions
{
    public static class IWebDriverExtensions
    {
        public static object ExecuteScript(this IWebDriver driver, string script, params object[] args) => (driver as IJavaScriptExecutor).ExecuteScript(script, args);
    }
}
