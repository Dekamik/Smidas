using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using Smidas.Common.Excel;
using Smidas.Core.Stocks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Smidas.Exporting.Excel
{
    public class ExcelExporter
    {
        private readonly ILogger _logger;

        public ExcelExporter(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ExcelExporter>();
        }

        public void Export(List<Stock> stocks, string exportFile)
        {
            _logger.LogDebug($"Exporting data to {exportFile}");

            using var excel = new ExcelPackage(new FileInfo(exportFile));
            var worksheet = excel.Workbook.Worksheets.Add($"AktieREA {DateTime.Today.ToString("yyyy-MM-dd")}");

            foreach (var prop in typeof(Stock).GetProperties())
            {
                var excelAttr = prop.GetCustomAttribute<ExcelAttribute>();
                if (excelAttr != null)
                {
                    worksheet.Cells[excelAttr.Column + "1"].Value = excelAttr.ShortName ?? excelAttr.FullName;
                }
            }

            for (int i = 0; i < stocks.Count; i++)
            {
                worksheet.InsertRowFrom(i + 2, stocks[i]);
            }

            excel.Save();

            _logger.LogDebug("Export complete");
        }
    }
}
