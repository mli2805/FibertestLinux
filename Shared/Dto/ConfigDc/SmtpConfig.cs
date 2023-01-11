namespace Fibertest.Dto;

public class SmtpConfig
{
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public string MailFrom { get; set; } = string.Empty;
    public string MailFromPassword { get; set; } = string.Empty;
    public int SmtpTimeoutMs { get; set; }

    public void FillIn(SmtpConfig other)
    {
        SmtpHost = other.SmtpHost;
        SmtpPort = other.SmtpPort;
        MailFrom = other.MailFrom;
        MailFromPassword = other.MailFromPassword;
        SmtpTimeoutMs = other.SmtpTimeoutMs;
    }
}