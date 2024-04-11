using Haver.Models;
using Microsoft.Identity.Client.Kerberos;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Haver.DraftModels
{
    public class DraftQualityRepresentative
    {
        public int ID { get; set; }

        [Display(Name = "PO or Prod. No.")]
        public int? PoNo { get; set; }

        [StringLength(255, ErrorMessage = "Sales Order No. is limited to 255 characters.")]
        [Display(Name = "Sales Order No.")]
        public string? SalesOrd { get; set; }

        [Display(Name = "Quantity Received")]
        public int? QuantReceived { get; set; }

        [Display(Name = "Quantity Defective")]
        public int? QuantDefective { get; set; }

        [StringLength(3000, ErrorMessage = "Description defective is limited to 3000 characters.")]
        [Display(Name = "Description of Defect")]
        public string? DescDefect { get; set; }

        [Display(Name = "Item marked Nonconforming")]
        public bool? NonConforming { get; set; }

        [Display(Name = "Check to exclude engineering section")]
        public bool? ConfirmingEng { get; set; }

        [Display(Name = "Date")]
        public DateOnly? QualityRepDate { get; set; }

        [StringLength(255, ErrorMessage = "Quality Representative sign is limited to 255 characters.")]
        [Display(Name = "Quality Representative's Name")]
        public string? QualityRepresentativeSign { get; set; } // LaterAdd - Automaticaly fill with user's name

        [Display(Name = "Quality Photos")]
        public List<Photo> QualityPhotos { get; set; }

        [Display(Name = "Video Links")]
        public List<VideoLink> VideoLinks { get; set; }

        //Foreign Key IDs
        public int? ProblemID { get; set; }

        public int? PartID { get; set; }

        public int? SupplierID { get; set; }

        public int? ProcessApplicableID { get; set; }

    }
}
