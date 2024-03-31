using Haver.DraftModels;
using System.ComponentModel.DataAnnotations;

namespace Haver.Models
{
    public class Supplier
    {
        public int ID { get; set; }

        [Display(Name = "Supplier")]
        public string SupplierSummary
        {
            get
            {
                return ($"{Convert.ToString(SupplierCode)} - {SupplierName}");
            }
        }

        [Required(ErrorMessage = "Supplier name is required.")]
        [Display(Name = "Supplier Name")]
        [StringLength(255, ErrorMessage = "Supplier Name is limited to 255 characters.")]
        public string SupplierName { get; set; }

        [Display(Name = "Supplier Code")]
        [Required(ErrorMessage = "Supplier name is required.")]
        public int SupplierCode { get; set; }

        public ICollection<QualityRepresentative> QualityRepresentatives { get; set; } = new HashSet<QualityRepresentative>();

    }
}
