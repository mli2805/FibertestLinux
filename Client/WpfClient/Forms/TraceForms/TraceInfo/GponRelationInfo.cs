namespace Fibertest.WpfClient
{
    public class GponRelationInfo
    {
        public string TceTitle { get; set; }
        public string TceType { get; set; }
        public int SlotPosition;
        public string SlotPositionStr => string.IsNullOrEmpty(TceType) ? "" : SlotPosition.ToString();

        public int GponInterfaceNumber;
        public string GponInterfaceNumberStr => string.IsNullOrEmpty(TceType) ? "" : GponInterfaceNumber.ToString();
    }
}
