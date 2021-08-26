using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Serilog.Events;
using Smidas.Common.Attributes;

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
        
        [ControllerLogging(EndpointName = "BatchJob")]
        [HttpGet, Route("{index}")]
        public IActionResult Index(string index)
        {
            _batchJobService.RunOnIndex(index);
            return Ok();
        }
    }
}