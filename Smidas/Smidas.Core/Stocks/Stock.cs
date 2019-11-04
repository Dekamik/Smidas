using Smidas.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Smidas.Core.Stocks
{
    public class Stock : IEquatable<Stock>
    {
        public string Name { get; set; }

        public Industry Industry { get; set; }

        public Action Action { get; set; }

        /// <summary>
        /// The stock price.
        /// </summary>
        /// <value>
        /// The latest stock price.
        /// </value>
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

        public override string ToString() => $"{Name}, {Industry.GetDisplayName()}, {Action.GetDisplayName()}, {Price}, {Turnover}, {JekPerStock}, " +
                                             $"{DirectYield}, {ProfitPerStock}, {ProfitPerJek}, {Ep}, {ARank}, {BRank}, {AbRank}, {Comments}";

        public override bool Equals(object obj) => obj.GetType() == typeof(Stock) ? Equals(obj as Stock) 
                                                                                  : false;

        public bool Equals(Stock other) => Name == other.Name;

        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(Name);
            hash.Add(Industry);
            hash.Add(Action);
            hash.Add(Price);
            hash.Add(Turnover);
            hash.Add(JekPerStock);
            hash.Add(DirectYield);
            hash.Add(ProfitPerStock);
            hash.Add(ProfitPerJek);
            hash.Add(Ep);
            hash.Add(ARank);
            hash.Add(BRank);
            hash.Add(AbRank);
            hash.Add(Comments);
            return hash.ToHashCode();
        }
    }
}
