using System.Reflection;
using System.Security.Permissions;

namespace Haver.Models
{
    public class NotificationVM
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
        public string Date { get; set; }
    }
}
