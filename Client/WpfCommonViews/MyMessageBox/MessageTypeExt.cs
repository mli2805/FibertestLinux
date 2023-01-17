using System.Windows;
using Fibertest.StringResources;

namespace WpfCommonViews
{
    public static class MessageTypeExt
    {
        public static string GetLocalizedString(this MessageType messageType)
        {
            switch (messageType)
            {
                case MessageType.Error: return Resources.SID_Error_;
                case MessageType.Information: return Resources.SID_Information;
                case MessageType.Confirmation: return Resources.SID_Confirmation;
                case MessageType.LongOperation: return Resources.SID_Long_operation__please_wait;
                default: return Resources.SID_Message;
            }
        }

        public static Visibility ShouldOkBeVisible(this MessageType messageType)
        {
            return messageType == MessageType.LongOperation ? Visibility.Collapsed : Visibility.Visible;
        }

        public static Visibility ShouldCancelBeVisible(this MessageType messageType)
        {
            return messageType == MessageType.Confirmation ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}