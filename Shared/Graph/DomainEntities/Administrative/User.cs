using Fibertest.Dto;

namespace Fibertest.Graph;

public class User
{
    public Guid UserId;
    public string Title;
    public string EncodedPassword;
    public string? MachineKey;
    public EmailReceiver Email = new EmailReceiver();
    public SmsReceiver Sms = new SmsReceiver();
    public Role Role;
    public Guid ZoneId;

    public bool IsDefaultZoneUser => ZoneId == Guid.Empty;

    public User(string title, string encodedPassword)
    {
        Title = title;
        EncodedPassword = encodedPassword;
    }
}