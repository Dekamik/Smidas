using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Smidas.Common;
using Smidas.Core.Stocks;
using Smidas.WebScraping.WebScrapers;
using Smidas.WebScraping.WebScrapers.DagensIndustri;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Smidas.WebScraping.Tests.WebScrapers.DagensIndustri
{
    // TODO: Fix tests
    public class DagensIndustriWebScraperTests
    {
        private readonly IDagensIndustriWebScraper _scraper;

        public DagensIndustriWebScraperTests()
        {
            var logger = A.Fake<ILogger<DagensIndustriWebScraper>>();
            _scraper = new DagensIndustriWebScraper(logger);
        }
        
        [Fact]
        public async void Scrape_ValidWebsite_ScrapingSuccessful()
        {
            var query = new AktieReaQuery
            {
                IndexUrls = new [] { "https://www.di.se/bors/aktier/?Countries=SE&Lists=4&Lists=&Lists=&Lists=&Lists=&RootSectors=&RootSectors=" },
                Industries = new Dictionary<string, AktieReaQuery.IndustryData>()
            };

            IEnumerable<Stock> stocks = await _scraper.Scrape(query);

            stocks.Should().NotBeNull();
            stocks.Should().NotBeEmpty();

            Stock example = stocks.First();
            example.Name.Should().NotBeNullOrEmpty();
            example.Price.Should().NotBe(0m);
            example.Volume.Should().NotBe(0m);
            example.ProfitPerStock.Should().NotBe(0m);
            example.Industry.Should().NotBeNullOrEmpty();
            example.AdjustedEquityPerStock.Should().NotBe(0m);
        }

        [Fact]
        public async void Scrape_InvalidWebsite_ScrapingUnsuccessful()
        {
            var query = new AktieReaQuery
            {
                IndexUrls = new [] { "https://www.affarsvarlden.se/bors/kurslistor/stockholm-large/kurs/" },
                Industries = new Dictionary<string, AktieReaQuery.IndustryData>()
            };

            await Assert.ThrowsAsync<WebScrapingException>(() => _scraper.Scrape(query));
        }
    }
}
