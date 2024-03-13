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

        public int? PurchasingID { get; set; }
        public Purchasing Purchasing { get; set; }

        public int? ProcurementID { get; set; }
        public Procurement Procurement { get; set; }

        public int? ReinspectionID { get; set; }
        public Reinspection Reinspection { get; set; }

    }
}
