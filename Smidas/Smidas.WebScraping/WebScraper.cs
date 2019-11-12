using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using Smidas.Common;
using Smidas.Core.Stocks;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Smidas.WebScraping
{
    public abstract class WebScraper : IWebScraper, IDisposable
    {
        private Random _random;

        private int _minWaitMillis;

        private int _maxWaitMillis;

        protected ILogger _logger;

        protected IWebDriver _webDriver;

        public WebScraper(IWebDriver webDriver, ILogger logger, AppSettings config)
        {
            _random = new Random();
            _minWaitMillis = config.MinWaitMillis;
            _maxWaitMillis = config.MaxWaitMillis;
            _webDriver = webDriver;
            _logger = logger;
        }

        public void Dispose()
        {
            _logger.LogTrace("Förstör webbskrapare");
            _webDriver.Dispose();
        }

        public abstract IEnumerable<Stock> Scrape();

        public void NavigateTo(string url)
        {
            _logger.LogInformation($"Surfar in på {url}");
            _webDriver.Navigate().GoToUrl(url);
        }

        public void Wait()
        {
            if (_minWaitMillis + _maxWaitMillis != 0)
            {
                var sleepMillis = _random.Next(_minWaitMillis, _maxWaitMillis);
                _logger.LogInformation($"Väntar i {sleepMillis} ms");
                Thread.Sleep(sleepMillis);
            }
        }
    }
}
