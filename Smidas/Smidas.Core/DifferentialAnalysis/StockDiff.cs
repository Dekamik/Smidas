using Smidas.Core.Stocks;
using System;
using System.Collections.Generic;
using System.Text;

namespace Smidas.Core.DifferentialAnalysis
{
    public class StockDiff
    {
        public Stock Before { get; set; }

        public Stock After { get; set; }

        public RankDifference RankPosDiff { get; set; }

        public int RankPosDiffAmount { get; set; }

        public int ABRankDiffAmount { get; set; }

        public RankDifference DirectYieldDiff { get; set; }

        public decimal DirectYieldDiffAmount { get; set; }
    }
}
