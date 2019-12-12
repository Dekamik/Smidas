using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Smidas.Core.DifferentialAnalysis
{
    public enum RankDifference
    {
        [Display(Name = "~")]
        NoChange = 0,

        [Display(Name = "▲")]
        Up = 1,

        [Display(Name = "▼")]
        Down = 2,
    }
}
