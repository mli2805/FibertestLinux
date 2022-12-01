using System.Diagnostics;
using System.Runtime.InteropServices;
using Fibertest.Dto;
using Fibertest.Utils;

namespace Fibertest.Rtu;

public partial class InterOpWrapper
{
    private readonly ILogger<InterOpWrapper> _logger;

    [DllImport("OtdrMeasEngine/iit_otdr.so")]
    private static extern void DllInit(string path, IntPtr logFile, IntPtr lenUnit);

    public InterOpWrapper(ILogger<InterOpWrapper> logger)
    {
        _logger = logger;
    }

    public bool InitDll()
    {
        IntPtr logFile = IntPtr.Zero;
        IntPtr lenUnit = IntPtr.Zero;

        try
        {
            DllInit("./OtdrMeasEngine", logFile, lenUnit); // under VSCode might require absolute path

            var iitOtdrLib = "./OtdrMeasEngine/iit_otdr.so";
            FileVersionInfo info = FileVersionInfo.GetVersionInfo(iitOtdrLib);
            var creationTime = File.GetLastWriteTime(iitOtdrLib);
            var version = $"{info.FileVersion} built {creationTime:dd/MM/yyyy}";

            _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), $"Iit_otdr.so {version} loaded successfully.");
        }
        catch (Exception e)
        {
            _logger.Log(LogLevel.Error, Logs.RtuManager.ToInt(), "InterOpWrapper.DllInit: " + e.Message, e);
            return false;
        }
        return true;
    }

    [DllImport("OtdrMeasEngine/iit_otdr.so")]
    private static extern int InitOTDR(int type, string ip, int port);

    public bool InitOtdr(ConnectionTypes type, string ip, int port)
    {
        int initOtdr;
        try
        {
            initOtdr = InitOTDR((int)type, ip, port);
            //         SetEqualStepsOfMeasurement();

            if (initOtdr == 0)
            {
                _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), "OTDR connected successfully!");
                return true;
            }

        }
        catch (ExternalException e)
        {
            _logger.Log(LogLevel.Error, Logs.RtuManager.ToInt(), "InterOpWrapper.InitOTDR: " + e.Message);
            return false;
        }
        catch (Exception e)
        {
            _logger.Log(LogLevel.Error, Logs.RtuManager.ToInt(), "InterOpWrapper.InitOTDR: " + e.Message);
            return false;
        }

        _logger.Log(LogLevel.Error, Logs.RtuManager.ToInt(), $"OTDR connection failed! Error: {initOtdr}");
        if (initOtdr == 805)
            _logger.Log(LogLevel.Error, Logs.RtuManager.ToInt(), $"InterOpWrapper.InitOTDR: 805 - ERROR_COM_OPEN - check otdr address or reboot rtu");
        return false;
    }
}