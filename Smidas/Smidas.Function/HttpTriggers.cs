using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;
using System.Collections.Generic;
using Smidas.Core.Stocks;
using Newtonsoft.Json;
using Smidas.Common;
using Smidas.Batch;

namespace Smidas.Function
{
    public class HttpTriggers
    {
        private readonly AktieReaJob aktieReaJob;

        public HttpTriggers(
            AktieReaJob aktieReaJob)
        {
            this.aktieReaJob = aktieReaJob;
        }

        [FunctionName(nameof(GetExcel))]
        public async Task<HttpResponseMessage> GetExcel([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
        {
            AktieReaQuery query = JsonConvert.DeserializeObject<AktieReaQuery>(await req.ReadAsStringAsync());

            IEnumerable<Stock> results = aktieReaJob.Run(query);

            byte[] xlsxBytes = null;

            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new ByteArrayContent(xlsxBytes);
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") 
            { 
                FileName = "Book1.xlsx"
            };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

            return result;
        }
    }
}
