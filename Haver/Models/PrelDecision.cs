using Haver.DraftModels;
using System.ComponentModel.DataAnnotations;

namespace Haver.Models
{
    public class PrelDecision
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Preliminary Decision is required.")]
        [Display(Name = "Preliminary Decision")]
        [StringLength(255, ErrorMessage = "Decision is limited to 255 characters.")]
        public string Decision { get; set; }

        public ICollection<Operations> OperationsS { get; set; } = new HashSet<Operations>();
    }
}
