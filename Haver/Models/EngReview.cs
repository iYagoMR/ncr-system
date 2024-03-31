using Haver.DraftModels;
using System.ComponentModel.DataAnnotations;

namespace Haver.Models
{
    public class EngReview
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Review is required.")]
        [Display(Name = "Engineering Review")]
        [StringLength(255, ErrorMessage = "Review is limited to 255 characters.")]
        public string Review { get; set; }

        public ICollection<Engineering> Engineerings { get; set; } = new HashSet<Engineering>();
    }
}
