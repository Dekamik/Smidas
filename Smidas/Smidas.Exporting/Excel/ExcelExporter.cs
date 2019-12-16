using OfficeOpenXml;
using OfficeOpenXml.Style;
using Smidas.Common.Excel;
using Smidas.Core.Stocks;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Smidas.Exporting.Excel
{
    public class ExcelExporter
    {
        public void ExportStocksToWorksheet(ref ExcelWorksheet worksheet, List<Stock> stocks, string currency, bool doStyling = true)
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
            if (doStyling)
            {
                int buyEndRow = 1 + stocks.Count(s => s.Action == Action.Buy);
                int keepEndRow = buyEndRow + stocks.Count(s => s.Action == Action.Keep);
                int sellEndRow = keepEndRow + stocks.Count(s => s.Action == Action.Sell);

                worksheet.Cells[$"A2:M{worksheet.Dimension.Rows}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[$"A2:M{buyEndRow}"].Style.Fill.BackgroundColor.SetColor(0, 226, 239, 218); // Buy - Green
                worksheet.Cells[$"A{buyEndRow + 1}:M{keepEndRow}"].Style.Fill.BackgroundColor.SetColor(0, 221, 235, 247); // Keep - Blue
                worksheet.Cells[$"A{keepEndRow + 1}:M{sellEndRow}"].Style.Fill.BackgroundColor.SetColor(0, 255, 242, 204); // Sell - Yellow
                worksheet.Cells[$"A{sellEndRow + 1}:M{worksheet.Dimension.Rows}"].Style.Fill.BackgroundColor.SetColor(0, 248, 203, 173); // Exclude - Red
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            }

            worksheet.Cells["A1:M1"].Style.Font.Bold = true;
            worksheet.Cells["A1:M1"].AutoFilter = true;
            worksheet.View.FreezePanes(2, 1);
            
        }
    }
}
