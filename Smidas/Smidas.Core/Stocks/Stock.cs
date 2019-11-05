using Smidas.Common.Extensions;

namespace Smidas.Core.Stocks
{
    public class Stock
    {
        public string Name { get; set; }

        public Industry Industry { get; set; }

        public Action Action { get; set; }

        /// <summary>
        /// The latest stock price.
        /// </summary>
        public decimal Price { get; set; }

        public decimal Turnover { get; set; }

        public decimal JekPerStock { get; set; }

        public decimal DirectYield { get; set; }

        public decimal ProfitPerStock { get; set; }

        public decimal ProfitPerJek { get; set; }

        public decimal Ep => Price != 0m ? ProfitPerStock / Price
                                         : 0m;

        public int ARank { get; set; }

        public int BRank { get; set; }

        public int AbRank => ARank + BRank + (Action == Action.Exclude ? 10000 : 0);

        public string Comments { get; set; }

        public void Exclude(string reason)
        {
            Action = Action.Exclude;
            Comments = reason;
        }

        public override string ToString() => $"{Name}, {Ep}";

        public string ToFullString() => $"{Name}, {Industry.GetDisplayName()}, {Action.GetDisplayName()}, {Price}, {Turnover}, {JekPerStock}, " +
                                        $"{DirectYield}, {ProfitPerStock}, {ProfitPerJek}, {Ep}, {ARank}, {BRank}, {AbRank}, {Comments}";
    }
}
