using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Smidas.Core.Stocks
{
    public enum Industry
    {
        [Display(Name = "Other")]
        Other = 0,

        [Display(Name = "Investment")]
        Investment = 1,

        [Display(Name = "Real estate")]
        RealEstate = 2,

        [Display(Name = "Banking")]
        Banking = 3,
    }
}
