using Microsoft.Identity.Client.Kerberos;
using System.ComponentModel.DataAnnotations;

namespace Haver.Models
{
    public class QualityRepresentative : Auditable
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "The PO No. is required.")]
        [Display(Name = "PO or Prod. No.")]
        public int PoNo { get; set; }

        [Required(ErrorMessage = "The Sales Order No. is required.")]
        [StringLength(255, ErrorMessage = "Sales Order No. is limited to 255 characters.")]
        [Display(Name = "Sales Order No.")]
        public string SalesOrd { get; set; }

        [Required(ErrorMessage = "The Quantity Received is required.")]
        [Display(Name = "Quantity Received")]
        public int QuantReceived { get; set; }

        [Required(ErrorMessage = "The Quantity Defective is required.")]
        [Display(Name = "Quantity Defective")]
        public int QuantDefective { get; set; }

        [Required(ErrorMessage = "The Description of Defect is required.")]
        [StringLength(3000, ErrorMessage = "Description defective is limited to 3000 characters.")]
        [Display(Name = "Description of Defect")]
        public string DescDefect { get; set; }

        [Required(ErrorMessage = "You must select an option.")]
        [Display(Name = "Item marked Nonconforming")]
        public bool NonConforming { get; set; }

        [Display(Name = "Check to exclude engineering section")]
        public bool ConfirmingEng { get; set; }

        [Required(ErrorMessage = "You must enter the start date of the report.")]
        [Display(Name = "Date")]
        public DateOnly QualityRepDate { get; set; }

        [Required(ErrorMessage = "The Quality Representative's signature is required.")]
        [StringLength(255, ErrorMessage = "Quality Representative sign is limited to 255 characters.")]
        [Display(Name = "Quality Representative's Name")]
        public string QualityRepresentativeSign { get; set; } // LaterAdd - Automaticaly fill with user's name

        [Display(Name = "Quality Photos")]
        public List<Photo> QualityPhotos { get; set; }

        //Foreign Key references
        [Required(ErrorMessage = "You must select a Problem.")]
        [Display(Name = "Problem")]
        public int ProblemID { get; set; }
        public Problem Problem { get; set; }

        [Required(ErrorMessage = "You must select a Part.")]
        [Display(Name = "Part")]
        public int PartID {  get; set; }
        public Part Part { get; set; }

        [Required(ErrorMessage = "You must select a Supplier.")]
        [Display(Name = "Supplier")]
        public int SupplierID { get; set; }
        public Supplier Supplier { get; set; }

        [Required(ErrorMessage = "You must select a Process applicable.")]
        [Display(Name = "Identify Process Applicable")]
        public int ProcessApplicableID { get; set; }
        public ProcessApplicable ProcessApplicable { get; set; }

        [Display(Name = "Video Links")]
        public List<VideoLink> VideoLinks { get; set; }

        public static implicit operator List<object>(QualityRepresentative v)
        {
            throw new NotImplementedException();
        }
    }
}
