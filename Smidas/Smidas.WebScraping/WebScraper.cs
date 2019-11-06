using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Smidas.WebScraping
{
    public abstract class WebScraper<T> : IWebScraper<T>
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

        public abstract IEnumerable<T> Scrape();

        public void Wait()
        {
            var sleepMillis = _random.Next(MinWaitMillis, MaxWaitMillis);
            _logger.LogInformation($"Sleeping for {sleepMillis}ms");
            Thread.Sleep(sleepMillis);
        }
    }
}
