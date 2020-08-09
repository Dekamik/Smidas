using Microsoft.Extensions.Logging;

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
