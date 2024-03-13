using System.ComponentModel.DataAnnotations;

namespace Haver.Models
{
    public class QualityPhoto
    {
        public int ID { get; set; }

        [ScaffoldColumn(false)]
        public byte[] Content { get; set; }

        [StringLength(255)]
        public string MimeType { get; set; }

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
