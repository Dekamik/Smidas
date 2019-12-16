using Smidas.Common;
using Smidas.Core.Stocks;
using System.Collections.Generic;

namespace Smidas.WebScraping.WebScrapers
{
    public interface IWebScraper
    {
        IList<Stock> Scrape(AktieReaQuery query);
    }
}
