using System.ComponentModel.DataAnnotations;

namespace Smidas.Core.Stocks
{
    public enum Action
    {
        [Display(Name = "Obestämd")]
        Undetermined = 0,

        [Display(Name = "Köp")]
        Buy = 1,

        [Display(Name = "Behåll")]
        Hold = 2,

        [Display(Name = "Sälj")]
        Sell = 3,

        [Display(Name = "Bortsållad")]
        Exclude = 4,
    }
}
