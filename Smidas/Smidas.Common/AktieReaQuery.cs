using System.Collections.Generic;

namespace Smidas.Common
{
    public class AktieReaQuery
    {
        public class IndustryData
        {
            public int Cap { get; set; }

            public string[] Companies { get; set; }
        }

        public class AnalysisOptionsData
        {
            public bool ExcludeNegativeProfitStocks { get; set; }
            public bool ExcludeZeroDividendStocks { get; set; }
            public bool ExcludePreferentialStocks { get; set; }
        }

        public class XPathExpressionsData
        {
            public string Names { get; set; }
            public string Prices { get; set; }
            public string Volumes { get; set; }
            public string ProfitPerStock { get; set; }
            public string AdjustedEquityPerStock { get; set; }
            public string DirectDividend { get; set; }
        }

        public int AmountToBuy { get; set; }

        public int AmountToKeep { get; set; }

        public string CurrencyCode { get; set; }

        public string[] IndexUrls { get; set; }

        public IDictionary<string, IndustryData> Industries { get; set; }

        public AnalysisOptionsData AnalysisOptions { get; set; }
        
        public XPathExpressionsData XPathExpressions { get; set; }
    }
}
