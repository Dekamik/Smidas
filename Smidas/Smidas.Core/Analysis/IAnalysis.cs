using Smidas.Common;
using Smidas.Core.Stocks;
using System.Collections.Generic;

namespace Smidas.Core.Analysis
{
    /// <summary>
    /// Interface for analysis classes
    /// </summary>
    /// <typeparam name="T">Data object type</typeparam>
    public interface IAnalysis
    {
        IEnumerable<Stock> Analyze(AktieReaQuery query, IEnumerable<Stock> stocks);
    }
}
