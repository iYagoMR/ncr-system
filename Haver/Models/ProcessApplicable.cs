using Haver.DraftModels;
using System.ComponentModel.DataAnnotations;

namespace Haver.Models
{
    public class ProcessApplicable
    {
        public int ID { get; set; }

        [Display(Name = "Process")]
        [Required(ErrorMessage = "Process name is required.")]
        [StringLength(255, ErrorMessage = " Process name is limited to 255 characters.")]
        public string ProcessName { get; set; }

        public ICollection<QualityRepresentative> QualityRepresentatives { get; set; } = new HashSet<QualityRepresentative>();
    }
}
