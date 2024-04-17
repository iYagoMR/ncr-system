using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Haver.Models
{
    public class Engineering : Auditable
    {
        public int ID { get; set; }

        [Display(Name = "Does Customer require notification of NCR")]
        public bool IsCustNotificationNecessary { get; set; }

        [Display(Name = "Message to customer")]
        [StringLength(3000, ErrorMessage = "Customer issue message is limited to 3000 characters.")]
        public string? CustIssueMsg { get; set; } // LaterAdd - Only if IsCustNotificationNecessary True

        [Required(ErrorMessage = "The Disposition is required.")]
        [Display(Name = "Disposition")]
        [StringLength(3000, ErrorMessage = "Disposition is limited to 3000 characters.")]
        public string Disposition { get; set; }

        [Display(Name = "Does the drawing require updating?")]
        public bool DrawReqUpdating { get; set; } // Nullable

        [Display(Name = "Original Rev. Number")]
        public int? OrgRevisionNum { get; set; } // LaterAdd - Only if DrawReqUpdating True

        [Display(Name = "Name of Engineer")]
        public string? RevisionedBy { get; set; } // LaterAdd - Only if DrawReqUpdating True

        [Display(Name = "Updated Revision Number")]
        public int? UpdatedRevisionNum { get; set; } // LaterAdd - Only if DrawReqUpdating True

        [Display(Name = "Revision Date")]
        public DateTime? RevisionDate { get; set; }

        [Required(ErrorMessage = "The Engineer's signature is required.")]
        [StringLength(55, ErrorMessage = "Engineer sign is limited to 55 characters.")]
        [Display(Name = "Engineering")]
        public string EngineerSign { get; set; } // LaterAdd - Automaticaly fill with user's name

        [Required(ErrorMessage = "The Date is required.")]
        [Display(Name = "Date")]
        public DateOnly EngineeringDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);

        [Display(Name = "Quality Photos")]
        public List<Photo> QualityPhotos { get; set; }

        // Foreign Key references

        [Required(ErrorMessage = "You must select an Engineering Review option.")]
        [Display(Name = "Review by HBC Engineering")]
        public int EngReviewID { get; set; }
        public EngReview EngReview { get; set; }

        [Display(Name = "Video Links")]
        public List<VideoLink> VideoLinks { get; set; }

    }
}
