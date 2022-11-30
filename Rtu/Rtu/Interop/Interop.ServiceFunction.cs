using System.Runtime.InteropServices;
using Fibertest.Dto;
using Fibertest.Utils;

namespace Fibertest.Rtu
{
    public partial class Interop
    {
        [DllImport("OtdrMeasEngine/iit_otdr.so")]
        public static extern int ServiceFunction(int cmd, ref int prm1, ref IntPtr prm2);

        public string? GetOtdrInfo(GetOtdrInfo infoType)
        {
            int cmd = (int)ServiceFunctionCommand.GetOtdrInfo;
            int prm = (int)infoType;
            IntPtr otdrInfo = IntPtr.Zero;

            var result = ServiceFunction(cmd, ref prm, ref otdrInfo);
            if (result == 0)
                return Marshal.PtrToStringAnsi(otdrInfo);

            _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), $"Get OTDR info error ={result}!");
            return "";
        }
    }
}
