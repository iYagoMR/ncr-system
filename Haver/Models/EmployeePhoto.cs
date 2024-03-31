using System.ComponentModel.DataAnnotations;

namespace Haver.Models
{
    public class EmployeePhoto
    {
        public int ID { get; set; }

        [ScaffoldColumn(false)]
        public byte[] Content { get; set; }

        [StringLength(255)]
        public string MimeType { get; set; }

        public int EmployeeID { get; set; }
        public Employee Employee { get; set; }
    }
}
