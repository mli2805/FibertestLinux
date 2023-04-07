namespace Fibertest.Dto;

public class SendTestNotificationDto : BaseRequest
{
    public NotificationType NotificationType;
    public string? Email;
    public string? PhoneNumber;

    public override string What => "SendTestNotification";

}