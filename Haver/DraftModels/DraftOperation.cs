using Haver.Models;
using System.ComponentModel.DataAnnotations;

namespace Haver.DraftModels
{
    public class DraftOperations
    {
        public int ID { get; set; }

        [Display(Name = "Was a CAR raised?")]
        public bool CarRaised { get; set; }

        [Display(Name = "Car number")]
        public int? CarNum { get; set; } // LaterAdd - Only if CarRaised True

        [Display(Name = "Follow up required?")]
        public bool IsFollowUpReq { get; set; }

        [StringLength(255, ErrorMessage = "Follow up type is limited to 255 characters.")]
        [Display(Name = "Follow up type")]
        public string FollowUpType { get; set; } // LaterAdd - Only if IsFollowUpReq True

        [Display(Name = "Expected date of follow up")]
        public DateTime? ExpecDate { get; set; }

        [Display(Name = "Date")]
        public DateOnly? OperationsDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);

        [StringLength(255, ErrorMessage = "The Op. manager sign is limited to 255 characters.")]
        [Display(Name = "Operations Manager")]
        public string OpManagerSign { get; set; }

        [StringLength(3000, ErrorMessage = "Decision Text is limited to 3000 characters.")]
        [Display(Name = "Message")]
        public string Message { get; set; }

        [Display(Name = "Quality Photos")]
        public List<Photo> QualityPhotos { get; set; }

        // FOREIGN KEYS :
        [Display(Name = "Purchasing's Preliminary Decision")]
        public int? PrelDecisionID { get; set; }

        [Display(Name = "Video Links")]
        public List<VideoLink> VideoLinks { get; set; }
    }
}
