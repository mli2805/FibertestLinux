using System.Reflection;
using System.Runtime.InteropServices;
using Fibertest.Dto;
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
                logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), "Dlls are loaded successfully.");
            }
            catch (Exception e)
            {
                logger.Log(LogLevel.Error, Logs.RtuManager.ToInt(), "Interop.DllInit: " + e.Message, e);
                return false;
            }
            return true;
        }

        [DllImport("OtdrMeasEngine/iit_otdr.so")]
        private static extern int InitOTDR(int type, string ip, int port);

        public static bool InitOtdr(ConnectionTypes type, string ip, int port, ILogger logger)
        {
            int initOtdr;
            try
            {
                initOtdr = InitOTDR((int)type, ip, port);
                //         SetEqualStepsOfMeasurement();

                if (initOtdr == 0)
                {
                    logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), "OTDR connected successfully!");
                    return true;
                }

            }
            catch (ExternalException e)
            {
                logger.Log(LogLevel.Error, Logs.RtuManager.ToInt(), "Interop.InitOTDR: " + e.Message);
                return false;
            }
            catch (Exception e)
            {
                logger.Log(LogLevel.Error, Logs.RtuManager.ToInt(), "Interop.InitOTDR: " + e.Message);
                return false;
            }

            logger.Log(LogLevel.Error, Logs.RtuManager.ToInt(), $"OTDR connection failed! Error: {initOtdr}");
            if (initOtdr == 805)
                logger.Log(LogLevel.Error, Logs.RtuManager.ToInt(), $"Interop.InitOTDR: 805 - ERROR_COM_OPEN - check otdr address or reboot rtu");
            return false;
        }
    }
}
