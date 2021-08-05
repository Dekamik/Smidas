using System;
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
        private ILogger<BatchJobController> _logger;

        public BatchJobController(IBatchJobService batchJobService,
            ILogger<BatchJobController> logger)
        {
            _logger = logger;
            _batchJobService = batchJobService;
        }
        
        [HttpGet, Route("{index}")]
        public async Task<IActionResult> Index(string index)
        {
            await _batchJobService.RunOnIndex(index);
            return Ok();
        }
    }
}