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

        public int AmountToBuy { get; set; }

        public int AmountToKeep { get; set; }

        public string CurrencyCode { get; set; }

        public string IndexUrl { get; set; }

        public IDictionary<string, IndustryData> Industries { get; set; }

        public AnalysisOptionsData AnalysisOptions { get; set; }
    }
}
