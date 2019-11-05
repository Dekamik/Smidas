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
    public interface IAnalysis<T>
    {
        IEnumerable<T> Analyze(IEnumerable<T> stocks, IEnumerable<string> blacklist);
    }
}
