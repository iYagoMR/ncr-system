using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Haver.Models
{
    public class Part
    {
        public int ID { get; set; }

        public string PartSummary
        {
            get
            {
                return $"{PartNumber} - {PartDesc}";
            }
        }

        [Display(Name = "Part Number")]
        [Required(ErrorMessage = "Part Number is Required")]
        public int PartNumber { get; set; }

        [Display(Name = "Part Description")]
        [Required(ErrorMessage = "Part Description is Required")]
        public string PartDesc { get; set; }

        [Display(Name = "Supplier")]
        public int? SupplierID { get; set; }
        public Supplier Supplier { get; set; }
    }
}
