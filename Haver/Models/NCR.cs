using Microsoft.Build.ObjectModelRemoting;
using System.ComponentModel.DataAnnotations;
using Haver.DraftModels;

namespace Haver.Models
{
    public class NCR : Auditable
    {
        public int ID { get; set; }

        [Display(Name = "NCR number")]
        public string NCRNum { get; set; }

        public bool IsNCRArchived { get; set; }

        public bool ExpecDateReturnReminded { get; set; }

        public bool IsNCRDraft { get; set; }

        [Display(Name = "Voiding reason")]
        public string VoidingReason { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; }

        [Display(Name = "Phase")]
        public string Phase { get; set; }    

        [Display(Name = "Created on DateOnly")]
        public DateOnly CreatedOnDO { get; set; }

        [ScaffoldColumn(false)]
        [Timestamp]
        public Byte[] RowVersion { get; set; }//Added for concurrency

        public QualityRepresentative QualityRepresentative { get; set; }
        public Engineering Engineering { get; set; }
        public Operations Operations { get; set; }
        public Procurement Procurement { get; set; }
        public Reinspection Reinspection { get; set; }

        public int? QualityRepresentativeID { get; set; }
        public int? EngineeringID { get; set; }
        public int? OperationsID { get; set; }
        public int? ProcurementID { get; set; }
        public int? ReinspectionID { get; set; }

        public int? PrevNCRID { get; set; }
        public int? NewNCRID { get; set; }

        //Draft models
        public DraftQualityRepresentative DraftQualityRepresentative { get; set; }
        public DraftEngineering DraftEngineering { get; set; }
        public DraftOperations DraftOperations { get; set; }
        public DraftProcurement DraftProcurement { get; set; }
        public DraftReinspection DraftReinspection { get; set; }

        public int? DraftQualityRepresentativeID { get; set; }
        public int? DraftEngineeringID { get; set; }
        public int? DraftOperationsID { get; set; }
        public int? DraftProcurementID { get; set; }
        public int? DraftReinspectionID { get; set; }
    }
}
