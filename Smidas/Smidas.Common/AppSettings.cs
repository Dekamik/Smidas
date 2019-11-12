namespace Smidas.Common
{
    public class AppSettings
    {
        public class IndustriesData
        {
            public int Cap { get; set; }

            public string Enum { get; set; }

            public string[] Stocks { get; set; }
        }

        public string[] Blacklist { get; set; }

        public string ChromeDriverDirectory { get; set; }

        public string ExportDirectory { get; set; }

        public IndustriesData[] Industries { get; set; }

        public int MinWaitMillis { get; set; }

        public int MaxWaitMillis { get; set; }
    }
}
