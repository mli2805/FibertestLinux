using System.IO.Ports;
using Fibertest.Dto;
using Microsoft.Extensions.Logging;

namespace Fibertest.Utils
{
    public static class CharonSerialPortExt
    {
        // public static ReturnCode ResetCharon(ILogger logger)
        // {
        //     try
        //     {
        //         var serialPort = new SerialPort(_comPortName, _comPortSpeed);
        //         serialPort.Open();
        //         logger.LogInfo(Logs.RtuManager, $"port {_comPortName} opened successfully");
        //
        //         serialPort.RtsEnable = !serialPort.RtsEnable;
        //         Thread.Sleep(10);
        //         logger.LogInfo(Logs.RtuManager, $"Now RTS is toggled to {serialPort.RtsEnable}");
        //         serialPort.RtsEnable = !serialPort.RtsEnable;
        //         Thread.Sleep(10);
        //         logger.LogInfo(Logs.RtuManager, $"Now RTS is toggled to {serialPort.RtsEnable}");
        //         serialPort.Close();
        //         logger.LogInfo(Logs.RtuManager, $"port {_comPortName} closed successfully");
        //
        //         logger.LogInfo(Logs.RtuManager, $"Pause after charon reset {_pauseAfterReset} seconds...");
        //         Thread.Sleep(TimeSpan.FromSeconds(_pauseAfterReset));
        //         logger.LogInfo(Logs.RtuManager, "Charon reset finished");
        //         return ReturnCode.Ok;
        //     }
        //     catch (Exception e)
        //     {
        //         logger.LogError(Logs.RtuManager, e.Message);
        //         return ReturnCode.CharonComPortError;
        //     }
        // }
    }
}
