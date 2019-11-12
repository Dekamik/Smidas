using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Text;

namespace Smidas.WebScraping.WebDriver
{
    public interface IWebDriverFactory
    {
        IWebDriver Create(string driverPath);
    }
}
