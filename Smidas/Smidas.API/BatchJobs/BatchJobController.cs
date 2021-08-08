using System;
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

        public BatchJobController(IBatchJobService batchJobService)
        {
            _batchJobService = batchJobService;
        }
        
        [StandardLogging(EntryMessage = "ENDPOINT /BatchJob called", ExitMessage = "ENDPOINT /BatchJob - 200 OK", Level = LogEventLevel.Information)]
        [HttpGet, Route("{index}")]
        public async Task<IActionResult> Index(string index)
        {
            await _batchJobService.RunOnIndex(index);
            return Ok();
        }
    }
}