using System;
using System.ComponentModel;

namespace Smidas.Common
{
    public enum StockIndex
    {
        [Description("stockholm-large")]
        OmxStockholmLargeCap = 0,

        [Description("danmark-large")]
        OmxCopenhagenLargeCap = 1,

        [Description("finland-large")]
        OmxHelsinkiLargeCap = 2,
    }
}
