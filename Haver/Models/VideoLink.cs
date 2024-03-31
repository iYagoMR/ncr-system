using Haver.DraftModels;

namespace Haver.Models
{
    public class VideoLink
    {

        public int ID { get; set; }
        public string Link { get; set; }

        public int? QualityRepresentativeID { get; set; }
        public QualityRepresentative QualityRepresentative { get; set; }

        public int? EngineeringID { get; set; }
        public Engineering Engineering { get; set; }

        public int? OperationsID { get; set; }
        public Operations Operations { get; set; }

        public int? ProcurementID { get; set; }
        public Procurement Procurement { get; set; }

        public int? DraftQualityRepresentativeID { get; set; }
        public DraftQualityRepresentative DraftQualityRepresentative { get; set; }

        public int? DraftEngineeringID { get; set; }
        public DraftEngineering DraftEngineering { get; set; }

        public int? DraftOperationsID { get; set; }
        public DraftOperations DraftOperations { get; set; }

        public int? DraftProcurementID { get; set; }
        public DraftProcurement DraftProcurement { get; set; }

        public int? DraftReinspectionID { get; set; }
        public DraftReinspection DraftReinspection { get; set; }

    }
}
