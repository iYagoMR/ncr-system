namespace Haver.ViewModels
{
    public interface IEmailConfiguration
    {
        string SmtpServer { get; }
        int SmtpPort { get; }
        string SmtpFromName { get; set; }
        string SmtpUserName { get; set; }
        string SmtpPassword { get; set; }
    }
}
