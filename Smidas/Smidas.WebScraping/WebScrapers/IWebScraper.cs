using Smidas.Common;
using Smidas.Core.Stocks;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Smidas.WebScraping.WebScrapers
{
    public interface IWebScraper
    {
        IList<Stock> Scrape(AktieReaQuery query);
    }
}
