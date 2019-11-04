using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Smidas.Core.Stocks
{
    public enum Action
    {
        [Display(Name = "Undetermined")]
        Undetermined = 0,

        [Display(Name = "Buy")]
        Buy = 1,

        [Display(Name = "Keep")]
        Keep = 2,

        [Display(Name = "Sell")]
        Sell = 3,

        [Display(Name = "Exclude")]
        Exclude = 4,
    }
}
