using Microsoft.AspNetCore.Mvc;
using Smidas.Common.Attributes;

namespace Smidas.API.BatchJobs
{
    [ApiController]
    [Route("[controller]")]
    public class BatchJobController : ControllerBase
    {
        private readonly IBatchJobService _batchJobService;

        public BatchJobController(IBatchJobService batchJobService)
        {
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