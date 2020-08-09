using System.ComponentModel.DataAnnotations;

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
