using System;
using System.Collections.Generic;
using System.Text;

namespace Smidas.Common.StockIndices
{
    [AttributeUsage(AttributeTargets.Field)]
    public class AffarsVarldenInfoAttribute : Attribute
    {
        public string StockIndexUrl { get; set; }

        public string StockIndicatorsUrl { get; set; }
    }
}
