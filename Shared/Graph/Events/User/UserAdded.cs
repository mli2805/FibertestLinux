using Fibertest.Dto;

namespace Graph
{
    public class UserAdded
    {
        public Guid UserId;
        public string? Title;
        public string? EncodedPassword;
        public EmailReceiver? Email;
        public SmsReceiver? Sms;
        public Role Role;
        public Guid ZoneId;

    }
}