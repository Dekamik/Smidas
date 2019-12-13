using System;
using System.Collections.Generic;
using System.Text;

namespace Smidas.Common.Extensions
{
    public static class DecimalExtensions
    {
        public static decimal NextDecimal(this Random rand) => new decimal(rand.NextDouble());

        public static decimal ParseDecimal(this string str) => decimal.Parse(!string.IsNullOrEmpty(str) ? str : "0");

        public static decimal ParseDecimalWithSymbol(this string str) => decimal.Parse(str[0..^1] ?? "0");
    }
}
