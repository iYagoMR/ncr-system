using System.Reflection;
using System.Security.Permissions;

namespace Haver.Models
{
    public class Notification
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public DateTime CreateOn { get; set; }
        public bool wasSeen { get; set; }
        public Employee Employee { get; set; }
        public int EmployeeID { get; set; }
    }
}
