using Smidas.Common.Extensions;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Smidas.Core.Stocks
{
    public class Stock
    {
        [Display(Name = "Namn")]
        public string Name { get; set; }

        /// <summary>
        /// Gets the Name without series letter if present
        /// </summary>
        public string CompanyName => Regex.Replace(Name, " [A-Z]$", "");

        [Display(Name = "Bransch")]
        public Industry Industry { get; set; }

        [Display(Name = "Åtgärd")]
        public Action Action { get; set; }

        [Display(Name = "Aktiekurs", ShortName = "Kurs")]
        public decimal Price { get; set; }

        [Display(Name = "Omsättning", ShortName = "Omsättn.")]
        public decimal Turnover { get; set; }

        [Display(Name = "Justerat eget kapital per aktie", ShortName = "JEK/aktie")]
        public decimal AdjustedEquityPerStock { get; set; }

        [Display(Name = "Direktavkastning", ShortName = "Dir.avk.")]
        public decimal DirectYield { get; set; }

        [Display(Name = "Vinst per aktie", ShortName = "Vinst/aktie")]
        public decimal ProfitPerStock { get; set; }

        /// <summary>
        /// E/P, i.e. the reverse of P/E
        /// The bigger the number, the more profitable the stock
        /// </summary>
        [Display(Name = "Earnings-to-Price ratio", ShortName = "E/P")]
        [Description("Vinst/aktie delat på aktiekurs.")]
        public decimal Ep => Price != 0m ? ProfitPerStock / Price
                                         : 0m;

        [Display(Name = "A-rang", ShortName = "A")]
        [Description("Rangordning efter E/P-tal.")]
        public int ARank { get; set; }

        [Display(Name = "B-rang", ShortName = "B")]
        [Description("Rangordning efter JEK/aktie.")]
        public int BRank { get; set; }

        [Display(Name = "A+B-rang", ShortName = "A+B")]
        [Description("A- och B-rangordningen kombinerad. Exkluderade aktier sätts till 10000-serien.")]
        public int AbRank => ARank + BRank + (Action == Action.Exclude ? 10000 : 0);

        [Display(Name = "Kommentarer")]
        [Description("Eventuella kommentarer. Anledning till exkludering ifylles automatiskt.")]
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
