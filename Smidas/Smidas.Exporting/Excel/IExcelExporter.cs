using System.Collections.Generic;
using OfficeOpenXml;
using Smidas.Core.Stocks;

namespace Smidas.Exporting.Excel
{
    public interface IExcelExporter
    {
        void ExportStocksToWorksheet(ref ExcelWorksheet worksheet, List<Stock> stocks, string currency, bool doStyling = true);
    }
}