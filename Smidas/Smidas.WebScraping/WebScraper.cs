using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using Smidas.Common;
using Smidas.Core.Stocks;
using Smidas.WebScraping.WebDriver;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Smidas.WebScraping
{
    public abstract class WebScraper : IWebScraper, IDisposable
    {
        private Random _random;

        protected ILogger _logger;

        public IWebDriver WebDriver { get; set; }

        public int MinWaitMillis { get; set; }

        public int MaxWaitMillis { get; set; }

        public WebScraper(IWebDriverFactory webDriverFactory, ILogger logger, IOptions<AppSettings> config)
        {
            _random = new Random();
            WebDriver = webDriverFactory.Create(config.Value.ChromeDriverDirectory);
            _logger = logger;
        }

        public void Dispose()
        {
            _logger.LogTrace("Förstör webbskrapare");
            WebDriver.Dispose();
        }

        public abstract IEnumerable<Stock> Scrape();

        public void Wait()
        {
            if (MinWaitMillis + MaxWaitMillis != 0)
            {
                var sleepMillis = _random.Next(MinWaitMillis, MaxWaitMillis);
                _logger.LogInformation($"Sleeping for {sleepMillis}ms");
                Thread.Sleep(sleepMillis);
            }
        }
    }
}
