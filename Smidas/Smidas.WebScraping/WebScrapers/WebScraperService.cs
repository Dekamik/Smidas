using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using Smidas.Common;
using Smidas.Core.Stocks;
using Smidas.WebScraping.WebDriver;

namespace Smidas.WebScraping.WebScrapers
{
    public abstract class WebScraperService<T> : WebScraper
    {
        protected IOptions<AppSettings> _config;

        public WebScraperService(
            IWebDriverFactory webDriverFactory, 
            ILoggerFactory loggerFactory, 
            IOptions<AppSettings> config) 
            : base(
                  webDriverFactory.Create(config.Value.WebScraper.ChromeDriverDirectory),
                  loggerFactory.CreateLogger<T>(), 
                  config.Value)
        {
            _config = config;
        }
    }
}
