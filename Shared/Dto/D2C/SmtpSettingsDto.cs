namespace Fibertest.Dto;

public class SmtpSettingsDto
{
    public string? SmptHost;
    public int SmptPort;
    public string? MailFrom;
    public string? MailFromPassword;
    public int SmtpTimeoutMs;
}