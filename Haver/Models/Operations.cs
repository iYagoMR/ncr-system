using System.ComponentModel.DataAnnotations;

namespace Haver.Models
{
    public class Operations : Auditable
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Choice is required.")]
        [Display(Name = "Was a CAR raised?")]
        public bool CarRaised { get; set; }

        [Display(Name = "Car number")]
        public int? CarNum { get; set; } // LaterAdd - Only if CarRaised True

        [Required(ErrorMessage = "Choice is required.")]
        [Display(Name = "Follow up required?")]
        public bool IsFollowUpReq { get; set; }

        [StringLength(255, ErrorMessage = "Follow up type is limited to 255 characters.")]
        [Display(Name = "Follow up type")]
        public string? FollowUpType { get; set; } // LaterAdd - Only if IsFollowUpReq True

        [Display(Name = "Expected date of follow up")]
        public DateTime? ExpecDate { get; set; }

        [Required(ErrorMessage = "Operations date is required.")]
        [Display(Name = "Date")]
        public DateOnly OperationsDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);

        [Required(ErrorMessage = "The Op. Manager's signature is required.")]
        [StringLength(255, ErrorMessage = "The Op. manager sign is limited to 255 characters.")]
        [Display(Name = "Operations Manager")]
        public string OpManagerSign { get; set; }

        [StringLength(3000, ErrorMessage = "Decision Text is limited to 3000 characters.")]
        [Display(Name = "Message")]
        public string? Message { get; set; }

        [Display(Name = "Quality Photos")]
        public List<Photo> QualityPhotos { get; set; }

        // FOREIGN KEYS :
        [Required(ErrorMessage = "You must select a Prel. decision option.")]
        [Display(Name = "Purchasing's Preliminary Decision")]
        public int PrelDecisionID { get; set; }
        public PrelDecision PrelDecision { get; set; }

        [Display(Name = "Video Links")]
        public List<VideoLink> VideoLinks { get; set; }
    }
}
