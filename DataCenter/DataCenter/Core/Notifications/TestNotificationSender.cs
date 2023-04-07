using Fibertest.Dto;

namespace Fibertest.DataCenter
{
    public class TestNotificationSender
    {
        private readonly SmtpNotifier _smtpNotifier;

        public TestNotificationSender(SmtpNotifier smtpNotifier)
        {
            _smtpNotifier = smtpNotifier;
        }

        public async Task<RequestAnswer> Send(SendTestNotificationDto dto)
        {
            switch (dto.NotificationType)
            {
                case NotificationType.Sms:
                    return new RequestAnswer(ReturnCode.NotImplementedYet);
                case NotificationType.Email:
                    return await _smtpNotifier.SendTest(dto.Email!);
                default :
                    return new RequestAnswer(ReturnCode.Error);
            }
        }
    }
}
