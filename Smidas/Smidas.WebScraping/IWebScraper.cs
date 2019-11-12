using Smidas.Core.Stocks;
using System.Collections.Generic;

namespace Smidas.WebScraping
{
    public interface IWebScraper
    {
        IEnumerable<Stock> Scrape();

        void NavigateTo(string url);

        void Wait();
    }
}
