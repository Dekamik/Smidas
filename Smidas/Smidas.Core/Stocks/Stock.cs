using Microsoft.Extensions.Logging;
using Smidas.Common.Excel;
using Smidas.Common.Extensions;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace Smidas.Core.Stocks
{
    public class Stock
    {
        public static readonly string OtherIndustries = "Övrig";

        [Excel(FullName = "Namn", Column = "A")]
        public string Name { get; set; }

        /// <summary>
        /// Gets the Name without series
        /// </summary>
        public string CompanyName => Regex.Replace(Name, " (Pref|[A-Z])$", "");

        [Excel(FullName = "Bransch", Column = "B")]
        public string Industry { get; set; } = OtherIndustries;

        [Excel(FullName = "Åtgärd", Column = "C")]
        public Action Action { get; set; }

        [Excel(FullName = "Aktiekurs i {0}", ShortName = "Kurs ({0})", Column = "D")]
        public decimal Price { get; set; }

        [Excel(FullName = "Volym", Column = "E")]
        public decimal Volume { get; set; }

        [Excel(FullName = "Justerat eget kapital per aktie", ShortName = "JEK/aktie", Column = "F")]
        public decimal AdjustedEquityPerStock { get; set; }

        [Excel(FullName = "Direktavkastning", ShortName = "Dir.avk.", Column = "G")]
        public decimal DirectDividend { get; set; }

        [Excel(FullName = "Vinst per aktie", ShortName = "Vinst/aktie", Column = "H")]
        public decimal ProfitPerStock { get; set; }

        /// <summary>
        /// E/P, i.e. the reverse of P/E
        /// The bigger the number, the more profitable the stock
        /// </summary>
        [Excel(FullName = "Earnings-to-Price ratio", ShortName = "E/P", Column = "I")]
        [Description("Vinst/aktie delat på aktiekurs.")]
        public decimal Ep => Price != 0m ? ProfitPerStock / Price : 0m;

        [Excel(FullName = "A-rang", ShortName = "A", Column = "J")]
        [Description("Rangordning efter E/P-tal.")]
        public int ARank { get; set; }

        [Excel(FullName = "B-rang", ShortName = "B", Column = "K")]
        [Description("Rangordning efter JEK/aktie.")]
        public int BRank { get; set; }

        [Excel(FullName = "A+B-rang", ShortName = "A+B", Column = "L")]
        [Description("A- och B-rangordningen kombinerad. Uteslutna aktier sätts till 10000-serien.")]
        public int AbRank => ARank + BRank + (Action == Action.Exclude ? 10000 : 0);

        [Excel(FullName = "Kommentarer", Column = "M")]
        [Description("Eventuella kommentarer. Anledning till bortsållning ifylles automatiskt.")]
        public string Comments { get; set; }

        public void Exclude(ILogger logger, string reason)
        {
            Action = Action.Exclude;
            Comments = reason;

            logger.LogTrace($"Sållade {Name} - {reason}");
        }

        public override string ToString() => $"{Name}, {CompanyName}, {Industry}, {Action.GetDisplayName()}, {Price}, {Volume}, " +
                                             $"{AdjustedEquityPerStock}, {DirectDividend}, {ProfitPerStock}, {Ep}, {ARank}, {BRank}, {AbRank}, {Comments}";
    }
}
