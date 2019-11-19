using System;
using System.Collections.Generic;
using System.Text;

namespace Smidas.Common.StockIndices
{
    public class StockInfoAttribute : Attribute
    {
        public string Currency { get; set; }
    }
}
