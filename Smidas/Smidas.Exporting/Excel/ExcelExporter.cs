using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Smidas.Common.Excel;
using Smidas.Core.Stocks;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Smidas.Exporting.Excel
{
    public class ExcelExporter
    {
        private readonly ILogger _logger;

        private readonly Color _green = Color.FromArgb(226, 239, 218);

        private readonly Color _blue = Color.FromArgb(221, 235, 247);

        private readonly Color _yellow = Color.FromArgb(255, 242, 204);

        private readonly Color _red = Color.FromArgb(248, 203, 173);

        public ExcelExporter(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ExcelExporter>();
        }

        public void Export(List<Stock> stocks, string exportFile)
        {
            _logger.LogInformation($"Exporterar analys om {stocks.Count()} aktier till {exportFile}");

            using var excel = new ExcelPackage(new FileInfo(exportFile));
            var worksheet = excel.Workbook.Worksheets.Add($"AktieREA {System.DateTime.Today.ToString("yyyy-MM-dd")}");

            // Headers
            foreach (var prop in typeof(Stock).GetProperties())
            {
                var excelAttr = prop.GetCustomAttribute<ExcelAttribute>();
                if (excelAttr != null)
                {
                    worksheet.Cells[excelAttr.Column + "1"].Value = excelAttr.ShortName ?? excelAttr.FullName;
                }
            }

            // Data
            for (int i = 0; i < stocks.Count; i++)
            {
                worksheet.InsertRowFrom(i + 2, stocks[i]);
            }

            // Styling
            var buyEndRow = 1 + stocks.Count(s => s.Action == Action.Buy);
            var keepEndRow = buyEndRow + stocks.Count(s => s.Action == Action.Keep);
            var sellEndRow = keepEndRow + stocks.Count(s => s.Action == Action.Sell);

            worksheet.Cells[$"A2:M{worksheet.Dimension.Rows}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[$"A2:M{buyEndRow}"].Style.Fill.BackgroundColor.SetColor(_green); // Buy
            worksheet.Cells[$"A{buyEndRow + 1}:M{keepEndRow}"].Style.Fill.BackgroundColor.SetColor(_blue); // Keep
            worksheet.Cells[$"A{keepEndRow + 1}:M{sellEndRow}"].Style.Fill.BackgroundColor.SetColor(_yellow); // Sell
            worksheet.Cells[$"A{sellEndRow + 1}:M{worksheet.Dimension.Rows}"].Style.Fill.BackgroundColor.SetColor(_red); // Exclude

            worksheet.Cells["A1:M1"].Style.Font.Bold = true;
            worksheet.Cells["A1:M1"].AutoFilter = true;
            worksheet.View.FreezePanes(2, 1);
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            excel.Save();

            _logger.LogDebug("Exportering slutförd");
        }
    }
}
