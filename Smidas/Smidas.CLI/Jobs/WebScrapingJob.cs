using Microsoft.Extensions.Logging;
using Smidas.Core.Stocks;
using System;
using System.Collections.Generic;
using System.Text;

namespace Smidas.CLI.Jobs
{
    public class WebScrapingJob : IJob<List<Stock>>
    {
        private readonly ILogger<WebScrapingJob> _logger;

        public WebScrapingJob(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<WebScrapingJob>();
        }

        public List<Stock> Run()
        {
            throw new NotImplementedException();
        }
    }
}
