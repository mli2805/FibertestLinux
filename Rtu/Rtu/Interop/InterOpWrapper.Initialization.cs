using System.Diagnostics;
using System.Runtime.InteropServices;
using Fibertest.Dto;
using Fibertest.Utils;
using OperatingSystem = Fibertest.Utils.OperatingSystem;

namespace Fibertest.Rtu;

public partial class InterOpWrapper
{
    private readonly ILogger<InterOpWrapper> _logger;

    // [DllImport("OtdrMeasEngine/iit_otdr.dll")]
    // private static extern void DllInit(string path, IntPtr logFile, IntPtr lenUnit);
    // [DllImport("OtdrMeasEngine/iit_otdr.so")]
    // private static extern int InitOTDR(int type, string ip, int port);

    public InterOpWrapper(ILogger<InterOpWrapper> logger)
    {
        _logger = logger;
    }

    public bool InitDll(string path)
    {
        IntPtr logFile = IntPtr.Zero;
        IntPtr lenUnit = IntPtr.Zero;

        try
        {
            if (OperatingSystem.IsWindows())
                WindowsImportDecl.DllInit(path, logFile, lenUnit);
            else
                LinuxImportDecl.DllInit(path, logFile, lenUnit);

            // DllInit(path, logFile, lenUnit); // under VSCode requires absolute path

            var libFileName = OperatingSystem.IsWindows() ? "iit_otdr.dll" : "iit_otdr.so";
            var iitOtdrLib = Path.Combine(path, libFileName);

            FileVersionInfo info = FileVersionInfo.GetVersionInfo(iitOtdrLib);
            var creationTime = File.GetLastWriteTime(iitOtdrLib);
            var version = $"{info.FileVersion} built {creationTime:dd/MM/yyyy}";

            _logger.Info(Logs.RtuManager, $"{libFileName} {version} loaded successfully.");
        }
        catch (Exception e)
        {
            _logger.Log(LogLevel.Error, Logs.RtuManager.ToInt(), "InterOpWrapper.DllInit: " + e.Message, e);
            return false;
        }
        return true;
    }

  
    public bool InitOtdr(ConnectionTypes type, string ip, int port)
    {
        int initOtdr;
        try
        {
            initOtdr = LinuxImportDecl.InitOTDR((int)type, ip, port);
            SetEqualStepsOfMeasurement();

            if (initOtdr == 0)
            {
                var word1 = type == ConnectionTypes.FreePort ? "disconnected" : "connected";
                _logger.Info(Logs.RtuManager, $"OTDR {word1} successfully!");
                return true;
            }

        }
        catch (ExternalException e)
        {
            _logger.Error(Logs.RtuManager, "InterOpWrapper.InitOTDR: " + e.Message);
            return false;
        }
        catch (Exception e)
        {
            _logger.Error(Logs.RtuManager, "InterOpWrapper.InitOTDR: " + e.Message);
            return false;
        }

        var word = type == ConnectionTypes.FreePort ? "disconnection" : "connection";
        _logger.Error(Logs.RtuManager, $"OTDR {word} failed! Error: {initOtdr}");
        if (initOtdr == 805)
            _logger.Error(Logs.RtuManager,
                "InterOpWrapper.InitOTDR: 805 - ERROR_COM_OPEN - check otdr address or reboot RTU");
        return false;
    }
}