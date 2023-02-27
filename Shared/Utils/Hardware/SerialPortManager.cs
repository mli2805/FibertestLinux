using System.IO.Ports;
using Fibertest.Dto;
using Microsoft.Extensions.Logging;

namespace Fibertest.Utils;

public enum LedDisplayCode : byte
{
    Wait = 0x13,
    Connecting = 0x14,

    ErrorConnectOtdr = 0x82,
    ErrorConnectOtau = 0x83,
    ErrorConnectBop = 0x84,
    ErrorTogglePort = 0x85,
}

public class SerialPortManager
{
    private ILogger _logger = null!;
    private string _comPortName = null!;
    private int _comPortSpeed;
    private int _pauseAfterReset;

    public void Initialize(CharonConfig config, ILogger logger)
    {
        _comPortName = config.ComPortName;
        _comPortSpeed = config.ComPortSpeed;
        _pauseAfterReset = config.PauseAfterReset;
        _logger = logger;
    }

    public ReturnCode ResetCharon()
    {
        try
        {
            var serialPort = new SerialPort(_comPortName, _comPortSpeed);
            serialPort.Open();
            _logger.Info(Logs.RtuManager, $"  port {_comPortName} opened successfully");

            serialPort.RtsEnable = !serialPort.RtsEnable;
            Thread.Sleep(10);
            _logger.Info(Logs.RtuManager, $"  Now RTS is toggled to {serialPort.RtsEnable}");
            serialPort.RtsEnable = !serialPort.RtsEnable;
            Thread.Sleep(10);
            _logger.Info(Logs.RtuManager, $"  Now RTS is toggled to {serialPort.RtsEnable}");
            serialPort.Close();
            _logger.Info(Logs.RtuManager, $"  port {_comPortName} closed successfully");

            _logger.Info(Logs.RtuManager, $"Pause after charon reset {_pauseAfterReset} seconds...");
            Thread.Sleep(TimeSpan.FromSeconds(_pauseAfterReset));
            _logger.Info(Logs.RtuManager, "  Charon reset finished");
            return ReturnCode.Ok;
        }
        catch (Exception e)
        {
            _logger.Error(Logs.RtuManager, e.Message);
            return ReturnCode.CharonComPortError;
        }
    }

    public void ShowOnLedDisplay(LedDisplayCode code)
    {
        _logger.Debug(Logs.RtuManager, $"  Write <{code}> on led display");

        var serialPort = new SerialPort(_comPortName, _comPortSpeed);
        try
        {
            serialPort.Open();
            Thread.Sleep(1000);

            _logger.Debug(Logs.RtuManager, $"  {_comPortName} opened successfully.");
        }
        catch (Exception e)
        {
            _logger.Error(Logs.RtuManager, $"  Can't open {_comPortName}.  {e.Message}");
            return;
        }

        byte[] buffer = { (byte)code };
        try
        {
            serialPort.Write(buffer, 0, 1);
            Thread.Sleep(1000);
            _logger.Debug(Logs.RtuManager, $"  {(byte)code:X} sent successfully.");
            serialPort.Close();
            _logger.Debug(Logs.RtuManager, $"  {_comPortName} closed successfully.");
        }
        catch (Exception e)
        {
            _logger.Error(Logs.RtuManager, $"  Can't send to {_comPortName}.  {e.Message}");
        }
        Thread.Sleep(1000);
    }
}