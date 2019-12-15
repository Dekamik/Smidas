using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Smidas.Function
{
    public static class HttpTriggers
    {
        [FunctionName("GetExcel")]
        public static async Task<IActionResult> GetExcel(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger logger)
        {
            logger.LogInformation("C# HTTP trigger function processed a request.");

            string index = req.Query["index"];

            return index != null
                ? (ActionResult)new OkObjectResult($"{index} selected.")
                : new BadRequestObjectResult("Please pass a number on the index string");
        }
    }
}
