using System;
using System.Collections.Generic;

namespace Smidas.WebScraping
{
    public interface IWebScraper<T>
    {
        IEnumerable<T> Scrape();

        void Wait();
    }
}
