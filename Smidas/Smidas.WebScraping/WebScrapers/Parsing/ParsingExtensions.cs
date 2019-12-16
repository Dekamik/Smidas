using System;
using System.Collections.Generic;

namespace Smidas.WebScraping.WebScrapers.Parsing
{
    public static class ParsingExtensions
    {
        public static decimal ParseDecimal(this string str)
        {
            try
            {
                return decimal.Parse(!string.IsNullOrEmpty(str) ? str.Replace(" ", "") : "0");
            }
            catch (FormatException)
            {
                throw new FormatException($"Decimal not in correct format: {str}");
            }
        }

        public static decimal ParseDecimalWithSymbol(this string str)
        {
            try
            {
                return decimal.Parse(str.Substring(0, str.Length - 1).Replace(" ", "") ?? "0");
            }
            catch
            {
                throw new FormatException($"Decimal not in correct format: {str}");
            }
        }

        public static IEnumerable<decimal> Parse(this IEnumerable<string> cells, bool hasSymbol = false)
        {
            foreach (string cell in cells)
            {
                yield return hasSymbol ? cell.ParseDecimalWithSymbol() : cell.ParseDecimal();
            }
        }
    }
}
