using OpenQA.Selenium;

namespace Smidas.WebScraping.WebDriver
{
    public interface IWebDriverFactory
    {
        IWebDriver Create(string driverPath);
    }
}
