using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using Smidas.Core.Stocks;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Smidas.WebScraping
{
    public abstract class WebScraper : IWebScraper
    {
        private Random _random;

        protected ILogger _logger;

        public IWebDriver WebDriver { get; set; }

        public int MinWaitMillis { get; set; }

        public int MaxWaitMillis { get; set; }

        public WebScraper(IWebDriver webDriver, ILogger logger)
        {
            _random = new Random();
            _logger = logger;
            WebDriver = webDriver;
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
