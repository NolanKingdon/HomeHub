namespace HomeHub.Web.Configuration
{
    public class SerilogOptions
    {
        public int Port { get; set; }
        public string SmtpServer { get; set; }
        public string FromEmail { get; set; }
        public string FromEmailPassword { get; set; }
        public string ToEmail { get; set; }
        public bool EnableSsl { get; set; }
        public string EmailSubject { get; set; }
    }
}