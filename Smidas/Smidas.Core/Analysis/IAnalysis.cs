using Smidas.Core.Stocks;
using System;
using System.Collections.Generic;
using System.Text;

namespace Smidas.Core.Analysis
{
    /// <summary>
    /// Interface for analysis classes
    /// </summary>
    /// <typeparam name="T">Data object type</typeparam>
    public interface IAnalysis
    {
        IEnumerable<Stock> Analyze(IEnumerable<Stock> stocks);
    }
}
