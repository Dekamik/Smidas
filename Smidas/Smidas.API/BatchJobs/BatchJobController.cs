using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Smidas.API.BatchJobs
{
    [ApiController]
    [Route("[controller]")]
    public class BatchJobController : ControllerBase
    {
        private readonly IBatchJobService _batchJobService;
        private readonly ILogger<BatchJobController> _logger;

        public BatchJobController(IBatchJobService batchJobService,
            ILogger<BatchJobController> logger)
        {
            _logger = logger;
            _batchJobService = batchJobService;
        }
        
        [HttpGet, Route("{index}")]
        public async Task<IActionResult> Index(string index)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            _logger.LogInformation($"ENDPOINT /BatchJob/OMXStockholmLargeCap");
            
            await _batchJobService.RunOnIndex(index);
            
            stopwatch.Stop();
            _logger.LogInformation($"ENDPOINT /BatchJob/OMXStockholmLargeCap - 200 OK ({stopwatch.ElapsedMilliseconds}ms)");
            return Ok();
        }
    }
}