using Fibertest.Dto;

namespace Fibertest.Graph
{
    public class AddUser
    {
        public Guid UserId;
        public string? Title;
        public string? EncodedPassword;
        public EmailReceiver Email = new();
        public SmsReceiver Sms = new();
        public Role Role;
        public Guid ZoneId;

    }
}