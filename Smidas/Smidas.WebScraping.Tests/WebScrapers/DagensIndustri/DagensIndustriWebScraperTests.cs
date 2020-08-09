using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Smidas.Common;
using Smidas.Core.Stocks;
using Smidas.WebScraping.WebScrapers.DagensIndustri;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Smidas.WebScraping.Tests.WebScrapers.DagensIndustri
{
    public class DagensIndustriWebScraperTests
    {
        [Fact]
        public async void Scrape_AnyCondition_ScrapingSuccessful()
        {
            var loggerFactory = A.Fake<ILoggerFactory>();
            var scraper = new DagensIndustriWebScraper(loggerFactory);
            var query = new AktieReaQuery
            {
                IndexUrl = "https://www.di.se/bors/aktier/?Countries=SE&Lists=4&Lists=&Lists=&Lists=&Lists=&RootSectors=&RootSectors=",
                Industries = new Dictionary<string, AktieReaQuery.IndustryData>()
            };

            IEnumerable<Stock> stocks = await scraper.Scrape(query);

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
    }
}
