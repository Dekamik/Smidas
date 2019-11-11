using Smidas.Common.Excel;
using Smidas.Common.Extensions;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Smidas.Core.Stocks
{
    public class Stock
    {
        [Excel(FullName = "Namn", Column = "A")]
        public string Name { get; set; }

        /// <summary>
        /// Gets the Name without series letter if present
        /// </summary>
        public string CompanyName => Regex.Replace(Name, " [A-Z]$", "");

        [Excel(FullName = "Bransch", Column = "B")]
        public Industry Industry { get; set; }

        [Excel(FullName = "Åtgärd", Column = "C")]
        public Action Action { get; set; }

        [Excel(FullName = "Aktiekurs", ShortName = "Kurs", Column = "D")]
        public decimal Price { get; set; }

        [Excel(FullName = "Omsättning", ShortName = "Omsättn.", Column = "E")]
        public decimal Turnover { get; set; }

        [Excel(FullName = "Justerat eget kapital per aktie", ShortName = "JEK/aktie", Column = "F")]
        public decimal AdjustedEquityPerStock { get; set; }

        [Excel(FullName = "Direktavkastning", ShortName = "Dir.avk.", Column = "G")]
        public decimal DirectYield { get; set; }

        [Excel(FullName = "Vinst per aktie", ShortName = "Vinst/aktie", Column = "H")]
        public decimal ProfitPerStock { get; set; }

        /// <summary>
        /// E/P, i.e. the reverse of P/E
        /// The bigger the number, the more profitable the stock
        /// </summary>
        [Excel(FullName = "Earnings-to-Price ratio", ShortName = "E/P", Column = "I")]
        [Description("Vinst/aktie delat på aktiekurs.")]
        public decimal Ep => Price != 0m ? ProfitPerStock / Price
                                         : 0m;

        [Excel(FullName = "A-rang", ShortName = "A", Column = "J")]
        [Description("Rangordning efter E/P-tal.")]
        public int ARank { get; set; }

        [Excel(FullName = "B-rang", ShortName = "B", Column = "K")]
        [Description("Rangordning efter JEK/aktie.")]
        public int BRank { get; set; }

        [Excel(FullName = "A+B-rang", ShortName = "A+B", Column = "L")]
        [Description("A- och B-rangordningen kombinerad. Uteslutet aktier sätts till 10000-serien.")]
        public int AbRank => ARank + BRank + (Action == Action.Exclude ? 10000 : 0);

        [Excel(FullName = "Kommentarer", Column = "M")]
        [Description("Eventuella kommentarer. Anledning till bortsållning ifylles automatiskt.")]
        public string Comments { get; set; }

        public void Exclude(string reason)
        {
            Action = Action.Exclude;
            Comments = reason;
        }

        public override string ToString() => $"{Name}";

        public string ToFullString() => $"{Name}, {CompanyName}, {Industry.GetDisplayName()}, {Action.GetDisplayName()}, {Price}, {Turnover}, " +
                                        $"{AdjustedEquityPerStock}, {DirectYield}, {ProfitPerStock}, {Ep}, {ARank}, {BRank}, {AbRank}, {Comments}";
    }
}
