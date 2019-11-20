using System.Collections.Generic;

namespace Smidas.Common
{
    public class AppSettings
    {
        public class IndexSettings
        {
            public class IndustryData
            {
                public int Cap { get; set; }

                public string[] Companies { get; set; }
            }

            public int AmountToBuy { get; set; }

            public int AmountToKeep { get; set; }

            public string[] Blacklist { get; set; }

            public string CurrencyCode { get; set; }

            public IDictionary<string, IndustryData> Industries { get; set; }
        }

        public class WebScraperSettings
        {
            public string ChromeDriverDirectory { get; set; }

            public int MinWaitMillis { get; set; }

            public int MaxWaitMillis { get; set; }
        }

        public IDictionary<string, IndexSettings> AktieRea { get; set; }

        public string ExportDirectory { get; set; }

        public WebScraperSettings WebScraper { get; set; }
    }
}
