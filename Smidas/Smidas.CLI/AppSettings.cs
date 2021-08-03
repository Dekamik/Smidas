using Smidas.Common;
using System.Collections.Generic;

namespace Smidas.CLI
{
    public class AppSettings
    {
        public class AktieReaLocalQuery : AktieReaQuery
        {
            public string ExportDirectory { get; set; }
        }

        public IDictionary<string, AktieReaLocalQuery> AktieRea { get; set; }
    }
}
