﻿using OfficeOpenXml;
using OfficeOpenXml.Style;
using Smidas.Common.Excel;
using Smidas.Core.Stocks;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Smidas.Common.Attributes;

namespace Smidas.Exporting.Excel
{
    public class ExcelExporter : IExcelExporter
    {
        [StandardLogging]
        public void ExportStocksToWorksheet(ref ExcelWorksheet worksheet, List<Stock> stocks, string currency, bool doStyling = true)
        {
            // Headers
            foreach (var prop in typeof(Stock).GetProperties())
            {
                var excelAttr = prop.GetCustomAttribute<ExcelAttribute>();
                
                if (excelAttr == null) 
                    continue;
                
                if (prop.Name == nameof(Stock.Price))
                {
                    worksheet.Cells[excelAttr.Column + "1"].Value = string.Format(excelAttr.ShortName ?? excelAttr.FullName ?? "{0}", currency);
                    continue;
                }
                worksheet.Cells[excelAttr.Column + "1"].Value = excelAttr.ShortName ?? excelAttr.FullName;
            }

            // Data
            for (var i = 0; i < stocks.Count; i++)
            {
                worksheet.InsertRowFrom(i + 2, stocks[i]);
            }

            // Styling
            if (doStyling)
            {
                var buyEndRow = 1 + stocks.Count(s => s.Action == Action.Buy);
                var keepEndRow = buyEndRow + stocks.Count(s => s.Action == Action.Hold);
                var sellEndRow = keepEndRow + stocks.Count(s => s.Action == Action.Sell);

                var green = System.Drawing.Color.FromArgb(226, 239, 218);
                var blue = System.Drawing.Color.FromArgb(221, 235, 247);
                var yellow = System.Drawing.Color.FromArgb(255, 242, 204);
                var red = System.Drawing.Color.FromArgb(248, 203, 173);

                worksheet.Cells[$"A2:M{worksheet.Dimension.Rows}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[$"A2:M{buyEndRow}"].Style.Fill.BackgroundColor.SetColor(green); // Buy
                worksheet.Cells[$"A{buyEndRow + 1}:M{keepEndRow}"].Style.Fill.BackgroundColor.SetColor(blue); // Keep
                worksheet.Cells[$"A{keepEndRow + 1}:M{sellEndRow}"].Style.Fill.BackgroundColor.SetColor(yellow); // Sell
                worksheet.Cells[$"A{sellEndRow + 1}:M{worksheet.Dimension.Rows}"].Style.Fill.BackgroundColor.SetColor(red); // Exclude
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            }

            worksheet.Cells["A1:M1"].Style.Font.Bold = true;
            worksheet.Cells["A1:M1"].AutoFilter = true;
            worksheet.View.FreezePanes(2, 1);
            
        }
    }
}
