namespace Fibertest.Graph
{
    public interface IMachineKeyProvider
    {
        string Get();
    }
    public class MachineKeyProvider : IMachineKeyProvider
    {
        public string Get()
        {
            var cpuId = GetCpuId();
            var mbSerial = GetMotherBoardSerial();
            var ddSerial = GetDiskDriveSerial();
            return cpuId + mbSerial + ddSerial;
        }

        private string GetCpuId()
        {
            try
            {
                return "cpu";

            }
            catch (Exception)
            {
                return @"ExceptionWhileGettingCpuId";
            }
        }

        private static string GetMotherBoardSerial()
        {
            try
            {
                return "board";
            }
            catch (Exception )
            {
                return @"ExceptionWhileGettingMotherBoardSerial";
            }
        }  
        
        private static string GetDiskDriveSerial()
        {
            try
            {
                return "disk";
            }
            catch (Exception )
            {
                return @"ExceptionWhileGettingDiskDriveSerial";
            }
        }
    }
}
