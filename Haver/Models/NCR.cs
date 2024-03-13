using Microsoft.Build.ObjectModelRemoting;
using System.ComponentModel.DataAnnotations;

namespace Haver.Models
{
    public class NCR
    {
        public int ID { get; set; }

        [Display(Name = "NCR number")]
        public string NCRNum { get; set; }

        [Display(Name = "Is engineering required?")]
        public bool IsEngineerRequired { get; set; }

        [Display(Name = "Is archived required?")]
        public bool IsNCRArchived { get; set; }

        [Display(Name ="Draft?")]
        public bool IsNCRDraft { get; set; }

        [Display(Name = "Voiding reason")]
        public string VoidingReason { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; }

        [Display(Name = "Phase")]
        public string Phase { get; set; }    


        [Display(Name = "Created on")]
        public DateOnly CreatedOn { get; set; }

        //Hard Coded role for testing purposes ONLY
        public string UserRole = "Quality Representative";

        public QualityRepresentative QualityRepresentative { get; set; }
        public Engineering Engineering { get; set; }
        public Purchasing Purchasing { get; set; }
        public Procurement Procurement { get; set; }
        public Reinspection Reinspection { get; set; }

        public int? QualityRepresentativeID { get; set; }
        public int? EngineeringID { get; set; }
        public int? PurchasingID { get; set; }
        public int? ProcurementID { get; set; }
        public int? ReinspectionID { get; set; }

        public int? PrevNCRID { get; set; }
        public int? NewNCRID { get; set; }
    }
}
