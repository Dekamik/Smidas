using System;
using System.Collections.Generic;
using System.Text;

namespace Smidas.Common.StockIndices
{
    [AttributeUsage(AttributeTargets.Field)]
    public class DagensIndustriInfoAttribute : Attribute
    {
        public string Url { get; set; }
    }
}
