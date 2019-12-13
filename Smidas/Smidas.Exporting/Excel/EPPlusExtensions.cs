using OfficeOpenXml;
using Smidas.Common.Excel;
using Smidas.Common.Extensions;
using System;
using System.Reflection;

namespace Smidas.Exporting.Excel
{
    public static class EPPlusExtensions
    {
        public static void InsertRowFrom<T>(this ExcelWorksheet worksheet, int row, T data)
        {
            foreach (PropertyInfo prop in typeof(T).GetProperties())
            {
                ExcelAttribute excelAttr = prop.GetCustomAttribute<ExcelAttribute>();
                if (excelAttr != null)
                {
                    if (prop.PropertyType.IsEnum)
                    {
                        worksheet.Cells[excelAttr.Column + row].Value = (prop.GetValue(data) as Enum).GetDisplayName() ?? prop.GetValue(data);
                    }
                    else
                    {
                        worksheet.Cells[excelAttr.Column + row].Value = prop.GetValue(data);
                    }
                }
            }
        }
    }
}
