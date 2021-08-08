using HtmlAgilityPack;

namespace Smidas.WebScraping.WebScrapers.Html
{
    public interface IHtmlWebFactory
    {
        HtmlWeb Create();
    }
}