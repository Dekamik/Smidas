using Smidas.Common.Extensions;
using System.Text.RegularExpressions;

namespace Smidas.Core.Stocks
{
    public class Stock
    {
        public string Name { get; set; }

        public string CompanyName => Regex.Replace(Name, " [A-Z]$", "");

        public Industry Industry { get; set; }

        public Action Action { get; set; }

        /// <summary>
        /// The latest stock price.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Amount of stocks in circulation. (SE: Omsättning)
        /// </summary>
        public decimal Turnover { get; set; }

        /// <summary>
        /// SE: Justerat Eget Kapital per aktie
        /// </summary>
        public decimal AdjustedEquityPerStock { get; set; }

        public decimal DirectYield { get; set; }

        public decimal ProfitPerStock { get; set; }

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

        public string ToFullString() => $"{Name}, {Industry.GetDisplayName()}, {Action.GetDisplayName()}, {Price}, {Turnover}, {AdjustedEquityPerStock}, " +
                                        $"{DirectYield}, {ProfitPerStock}, {Ep}, {ARank}, {BRank}, {AbRank}, {Comments}";
    }
}
