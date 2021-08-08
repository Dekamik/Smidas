using System;
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
        private readonly Stopwatch _stopwatch = new();

        public BatchJobController(IBatchJobService batchJobService,
            ILogger<BatchJobController> logger)
        {
            _logger = logger;
            _batchJobService = batchJobService;
        }
        
        [HttpGet, Route("{index}")]
        public async Task<IActionResult> Index(string index)
        {
            _logger.LogInformation($"/BatchJob/{index} called");
            _stopwatch.Start();
            
            await _batchJobService.RunOnIndex(index);
            
            _stopwatch.Stop();
            _logger.LogInformation($"/BatchJob/{index} - 200 OK ({_stopwatch.ElapsedMilliseconds}ms)");
            return Ok();
        }
    }
}