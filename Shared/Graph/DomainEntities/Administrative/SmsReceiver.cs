namespace Fibertest.Graph;

[Serializable]
public class SmsReceiver
{
    public string PhoneNumber = "";
    public bool IsFiberBreakOn;
    public bool IsCriticalOn;
    public bool IsMajorOn;
    public bool IsMinorOn;
    public bool IsOkOn;
    public bool IsNetworkEventsOn;
    public bool IsBopEventsOn;
    public bool IsActivated;
}