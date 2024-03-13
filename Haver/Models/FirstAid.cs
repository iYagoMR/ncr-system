using System.ComponentModel.DataAnnotations;

namespace Haver.Models
{
    public enum FirstAid
    {
        None,
        [Display(Name = "c Level 1")]
        Level1,
        [Display(Name = "OFA Level 2")]
        Level2,
        [Display(Name = "OFA Level 3")]
        Level3
    }
}
