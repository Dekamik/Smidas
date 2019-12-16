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
using OfficeOpenXml;
using System;
using System.Linq;
using Smidas.Exporting.Excel;
using Microsoft.Extensions.Logging;
using Smidas.WebScraping.WebScrapers.DagensIndustri;
using Smidas.Core.Analysis;
using System.Threading;
using System.Globalization;

namespace Smidas.Function
{
    public class HttpTriggers
    {
        private readonly AktieReaJob aktieReaJob;
        private readonly ExcelExporter excelExporter;

        public HttpTriggers()
        {
            LoggerFactory loggerFactory = new LoggerFactory();
            aktieReaJob = new AktieReaJob(loggerFactory,
                new DagensIndustriWebScraper(loggerFactory),
                new AktieRea(loggerFactory));
            excelExporter = new ExcelExporter();
        }

        [FunctionName(nameof(GetExcel))]
        public async Task<HttpResponseMessage> GetExcel(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("sv-SE");

            DateTime callTime = DateTime.Now;

            FunctionQuery query = JsonConvert.DeserializeObject<FunctionQuery>(await req.ReadAsStringAsync());
            IEnumerable<Stock> results = aktieReaJob.Run(query);

            byte[] xlsxBytes = null;

            using (ExcelPackage excel = new ExcelPackage())
            {
                ExcelWorksheet worksheet = excel.Workbook.Worksheets.Add($"AktieREA {callTime.ToString("yyyy-MM-dd")}");

                excelExporter.ExportStocksToWorksheet(ref worksheet, results.ToList(), query.CurrencyCode, doStyling: false);

                xlsxBytes = excel.GetAsByteArray();
            }

            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new ByteArrayContent(xlsxBytes);
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = $"AktieREA_{query.IndexName}_{callTime.ToString("yyyy-MM-dd_HHmm")}.xlsx"
            };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

            return result;
        }
    }
}
