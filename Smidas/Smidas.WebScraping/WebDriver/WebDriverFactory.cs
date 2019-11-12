using System;
using System.Collections.Generic;
using System.Text;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Smidas.WebScraping.WebDriver
{
    public class WebDriverFactory : IWebDriverFactory
    {
        public IWebDriver Create(string driverFolder)
        {
            return new ChromeDriver(driverFolder);
        }
    }
}
