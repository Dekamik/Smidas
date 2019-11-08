using Smidas.Core.Stocks;
using System;
using System.Collections.Generic;

namespace Smidas.WebScraping
{
    public interface IWebScraper
    {
        IEnumerable<Stock> Scrape();

        void Wait();
    }
}
