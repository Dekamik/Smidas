using System.Collections.Generic;
using Smidas.Common;
using Smidas.Core.Stocks;

namespace Smidas.Core.Analysis
{
    public interface IAktieRea : IAnalysis
    {
        void ExcludeDisqualifiedStocks(ref IEnumerable<Stock> stocks, AktieReaQuery.AnalysisOptionsData options);
        void ExcludeDoubles(ref IEnumerable<Stock> stocks);
        void CalculateARank(ref IEnumerable<Stock> stocks);
        void CalculateBRank(ref IEnumerable<Stock> stocks);
        void DetermineActions(ref IEnumerable<Stock> stocks, AktieReaQuery query);
    }
}