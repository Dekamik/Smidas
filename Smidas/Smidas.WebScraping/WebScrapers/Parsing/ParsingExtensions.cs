using System.Collections.Generic;

namespace Smidas.WebScraping.WebScrapers.Parsing
{
    public static class ParsingExtensions
    {
        public static decimal ParseDecimal(this string str) => decimal.Parse(!string.IsNullOrEmpty(str) ? str : "0");

        public static decimal ParseDecimalWithSymbol(this string str) => decimal.Parse(str.Substring(0, str.Length - 1) ?? "0");

        public static IEnumerable<decimal> Parse(this IEnumerable<string> cells, bool hasSymbol = false)
        {
            foreach (string cell in cells)
            {
                yield return hasSymbol ? cell.ParseDecimalWithSymbol() : cell.ParseDecimal();
            }
        }
    }
}
