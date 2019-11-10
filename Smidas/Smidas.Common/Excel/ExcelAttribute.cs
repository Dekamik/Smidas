using System;
using System.Collections.Generic;
using System.Text;

namespace Smidas.Common.Excel
{
    public class ExcelAttribute : Attribute
    {
        public string FullName { get; set; }

        public string ShortName { get; set; }

        public string Column { get; set; }
    }
}
