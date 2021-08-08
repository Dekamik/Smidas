using HtmlAgilityPack;

namespace Smidas.WebScraping.WebScrapers.Html
{
    public class HtmlWebFactory : IHtmlWebFactory
    {
        public HtmlWeb Create() => new HtmlWeb();
    }
}