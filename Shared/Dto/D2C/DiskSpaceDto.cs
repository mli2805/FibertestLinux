namespace Fibertest.Dto
{
    
    public class DiskSpaceDto : RequestAnswer
    {
        public double TotalSize;
        public double AvailableFreeSpace;
        public double DataSize;
        public double FreeSpaceThreshold;
    }
}