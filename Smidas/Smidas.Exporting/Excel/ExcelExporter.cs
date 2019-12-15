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
        private readonly Color green = Color.FromArgb(226, 239, 218);
        private readonly Color blue = Color.FromArgb(221, 235, 247);
        private readonly Color yellow = Color.FromArgb(255, 242, 204);
        private readonly Color red = Color.FromArgb(248, 203, 173);

        public void ExportStocksToWorksheet(ref ExcelWorksheet worksheet, List<Stock> stocks, string currency)
        {
            // Headers
            foreach (PropertyInfo prop in typeof(Stock).GetProperties())
            {
                ExcelAttribute excelAttr = prop.GetCustomAttribute<ExcelAttribute>();
                if (excelAttr != null)
                {
                    if (prop.Name == nameof(Stock.Price))
                    {
                        worksheet.Cells[excelAttr.Column + "1"].Value = string.Format(excelAttr.ShortName ?? excelAttr.FullName ?? "{0}", currency);
                        continue;
                    }
                    worksheet.Cells[excelAttr.Column + "1"].Value = excelAttr.ShortName ?? excelAttr.FullName;
                }
            }

            // Data
            for (int i = 0; i < stocks.Count; i++)
            {
                worksheet.InsertRowFrom(i + 2, stocks[i]);
            }

            // Styling
            int buyEndRow = 1 + stocks.Count(s => s.Action == Action.Buy);
            int keepEndRow = buyEndRow + stocks.Count(s => s.Action == Action.Keep);
            int sellEndRow = keepEndRow + stocks.Count(s => s.Action == Action.Sell);

            worksheet.Cells[$"A2:M{worksheet.Dimension.Rows}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[$"A2:M{buyEndRow}"].Style.Fill.BackgroundColor.SetColor(green); // Buy
            worksheet.Cells[$"A{buyEndRow + 1}:M{keepEndRow}"].Style.Fill.BackgroundColor.SetColor(blue); // Keep
            worksheet.Cells[$"A{keepEndRow + 1}:M{sellEndRow}"].Style.Fill.BackgroundColor.SetColor(yellow); // Sell
            worksheet.Cells[$"A{sellEndRow + 1}:M{worksheet.Dimension.Rows}"].Style.Fill.BackgroundColor.SetColor(red); // Exclude

            worksheet.Cells["A1:M1"].Style.Font.Bold = true;
            worksheet.Cells["A1:M1"].AutoFilter = true;
            worksheet.View.FreezePanes(2, 1);
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        }
    }
}
