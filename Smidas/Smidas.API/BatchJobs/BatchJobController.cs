using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Smidas.API.BatchJobs
{
    [ApiController]
    [Route("[controller]")]
    public class BatchJobController : ControllerBase
    {
        private readonly BatchJobService _batchJobService;

        public BatchJobController(BatchJobService batchJobService)
        {
            _batchJobService = batchJobService;
        }
        
        [HttpGet, Route("{index}")]
        public async Task<IActionResult> Index(string index)
        {
            try
            {
                await _batchJobService.RunOnIndex(index);
                return Ok();
            }
            catch (Exception ex)
            {
                return Problem(ex.StackTrace, null, 500, ex.Message, ex.GetType().Name);
            }
        }
    }
}