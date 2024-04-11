using System.ComponentModel.DataAnnotations;

namespace Haver.Models
{
    public class Reinspection : Auditable
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Choice is required.")]
        [Display(Name = "Re-Inspected Acceptable?")]
        public bool ReinspecAccepted { get; set; } // IF FALSE START NEW NCR (ANOTHER TABLE REQUIRED)

        [Display(Name = "New NCR number")]
        public string? NewNCRNum { get; set; }

        [Required(ErrorMessage = "Reinspection date is required.")]
        [Display(Name = "Date")]
        public DateOnly ReinspectionDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);

        [Required(ErrorMessage = "Quality Representative's signature is required.")]
        [StringLength(255, ErrorMessage = "Quality Representative's signature is limited to 255 characters.")]
        [Display(Name = "Inspector's Name")]
        public string ReinspecInspectorSign { get; set; }

        [Display(Name = "Quality Photos")]
        public List<Photo> QualityPhotos { get; set; }

        [Display(Name = "Video Links")]
        public List<VideoLink> VideoLinks { get; set; }

    }
}
