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
                return decimal.Parse(str[..^1].Replace(" ", ""));
            }
            catch
            {
                throw new FormatException($"Decimal not in correct format: {str}");
            }
        }

        public static decimal ParsePercentage(this string str)
        {
            try
            {
                return decimal.Parse(str[..^1].Replace(",", "."));
            }
            catch
            {
                throw new FormatException($"Decimal not in correct format: {str}");
            }
        }

        public static IEnumerable<decimal> Parse(this IEnumerable<string> cells, DecimalType type = DecimalType.Normal)
        {
            switch (type)
            {
                case DecimalType.Normal:
                default:
                    foreach (var cell in cells)
                    {
                        yield return cell == "-" ? 0 : cell.ParseDecimal();
                    }
                    break;
                
                case DecimalType.WithSymbol:
                    foreach (var cell in cells)
                    {
                        yield return cell == "-" ? 0 : cell.ParseDecimalWithSymbol();
                    }
                    break;
                
                case DecimalType.Percentage:
                    foreach (var cell in cells)
                    {
                        yield return cell == "-" ? 0 : cell.ParsePercentage();
                    }
                    break;
            }
            
        }
    }
}
