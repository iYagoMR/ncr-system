namespace Haver.Utilities
{
    public class ConfigurationVariable
    {
        public int ID { get; set; }
        public int ArchiveNCRsYears { get; set; }
        public int OverdueNCRsNotificationDays { get; set; }
        public DateTime DateToRunNotificationJob { get; set; }
        public DateTime DateToRunArchiveJob { get; set; }
    }
}
