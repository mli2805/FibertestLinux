using System.Reflection;
using System.Runtime.InteropServices;
using Fibertest.Utils;

namespace Fibertest.Rtu
{
    public static class Interop
    {
        [DllImport("OtdrMeasEngine/iit_otdr.so")]
        private static extern void DllInit(string path, IntPtr logFile, IntPtr lenUnit);

        public static bool InitDll(ILogger logger)
        {
            var mainFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
            logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), $"Main folder is {mainFolder}");
            var iitFolder = mainFolder + "/OtdrMeasEngine";
            logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), $"IIT folder is {iitFolder}"); 

            IntPtr logFile = IntPtr.Zero;
            IntPtr lenUnit = IntPtr.Zero;

            try
            {
                DllInit(iitFolder, logFile, lenUnit); // requires absolute path under VSCode
            }
            catch (Exception e)
            {
                logger.Log(LogLevel.Error, Logs.RtuManager.ToInt(), "Interop.DllInit: " + e.Message, e);
                return false;
            }
            return true;
        }
    }
}
