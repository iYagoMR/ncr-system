using Haver.DraftModels;
using System.ComponentModel.DataAnnotations;

namespace Haver.Models
{
    public class Problem
    {
        public int ID { get; set; }

        [Display(Name = "Problem")]
        [Required(ErrorMessage = "Brief problem description is required.")]
        [StringLength(255, ErrorMessage = "Problem Description is limited to 255 characters.")]
        public string ProblemDescription { get; set; }

        public ICollection<QualityRepresentative> QualityRepresentatives { get; set; } = new HashSet<QualityRepresentative>();
    }
}
