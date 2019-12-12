using System;
using System.Collections.Generic;
using System.Text;

namespace Smidas.Common.Extensions
{
    public static class DecimalExtensions
    {
        public static decimal NextDecimal(this Random rand)
        {
            return new decimal(rand.NextDouble());
        }
    }
}
