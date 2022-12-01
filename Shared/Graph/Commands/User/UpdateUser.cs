using Fibertest.Dto;

namespace Fibertest.Graph;

public class UpdateUser
{
    public Guid UserId;
    public string? Title;
    public string? EncodedPassword;
    public string? MachineKey;
    public EmailReceiver Email = new();
    public SmsReceiver Sms = new();
    public Role Role;
    public Guid ZoneId;
}