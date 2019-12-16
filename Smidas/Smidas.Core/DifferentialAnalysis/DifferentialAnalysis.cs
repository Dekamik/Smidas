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

        public DifferentialAnalysis(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<DifferentialAnalysis>();
        }
    }
}
