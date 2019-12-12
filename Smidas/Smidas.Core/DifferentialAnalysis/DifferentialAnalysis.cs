using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Smidas.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Smidas.Core.DifferentialAnalysis
{
    public class DifferentialAnalysis
    {
        private readonly ILogger _logger;

        private readonly IOptions<AppSettings> _options;

        public DifferentialAnalysis(ILoggerFactory loggerFactory, IOptions<AppSettings> options)
        {
            _logger = loggerFactory.CreateLogger<DifferentialAnalysis>();
            _options = options;
        }
    }
}
