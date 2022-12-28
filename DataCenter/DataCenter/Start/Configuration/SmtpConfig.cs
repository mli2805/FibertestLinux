namespace Fibertest.DataCenter;

public class SmtpConfig
{
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public string MailFrom { get; set; } = string.Empty;
    public string MailFromPassword { get; set; } = string.Empty;
    public int SmtpHSmtpTimeoutMsost { get; set; }
}