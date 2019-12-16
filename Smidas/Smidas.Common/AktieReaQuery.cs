using System;
using System.Collections.Generic;
using System.Text;

namespace Smidas.Common
{
    public class AktieReaQuery
    {
        public class IndustryData
        {
            public int Cap { get; set; }

            public string[] Companies { get; set; }
        }

        public int AmountToBuy { get; set; }

        public int AmountToKeep { get; set; }

        public string CurrencyCode { get; set; }

        public string IndexUrl { get; set; }

        public IDictionary<string, IndustryData> Industries { get; set; }
    }
}
