using System.ComponentModel.DataAnnotations;

namespace Smidas.Core.Stocks
{
    public enum Industry
    {
        [Display(Name = "Övrig")]
        Other = 0,

        [Display(Name = "Investering")]
        Investment = 1,

        [Display(Name = "Fastigheter")]
        RealEstate = 2,

        [Display(Name = "Bank")]
        Banking = 3,
    }
}
