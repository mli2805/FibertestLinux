using System.IO.Ports;
using Fibertest.Dto;
using Fibertest.Utils;
using Microsoft.Extensions.Options;

namespace Fibertest.Rtu;

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
    private readonly ILogger<SerialPortManager> _logger;
    private readonly string _comPortName;
    private readonly int _comPortSpeed;
    private readonly int _pauseAfterReset;

    public SerialPortManager(IOptions<CharonConfig> config, ILogger<SerialPortManager> logger)
    {
        _logger = logger;
        _comPortName = config.Value.ComPortName;
        _comPortSpeed = config.Value.ComPortSpeed;
        _pauseAfterReset = config.Value.PauseAfterReset != 0 ? config.Value.PauseAfterReset : 5;
    }

    public ReturnCode ResetCharon()
    {
        try
        {
            var serialPort = new SerialPort(_comPortName, _comPortSpeed);
            serialPort.Open();
            _logger.LLog(Logs.RtuManager.ToInt(), $"port {_comPortName} opened successfully");

            serialPort.RtsEnable = !serialPort.RtsEnable;
            Thread.Sleep(10);
            _logger.LLog(Logs.RtuManager.ToInt(), $"Now RTS is toggled to {serialPort.RtsEnable}");
            serialPort.RtsEnable = !serialPort.RtsEnable;
            Thread.Sleep(10);
            _logger.LLog(Logs.RtuManager.ToInt(), $"Now RTS is toggled to {serialPort.RtsEnable}");
            serialPort.Close();
            _logger.LLog(Logs.RtuManager.ToInt(), $"port {_comPortName} closed successfully");

            _logger.LLog(Logs.RtuManager.ToInt(), $"Pause after charon reset {_pauseAfterReset} seconds...");
            Thread.Sleep(TimeSpan.FromSeconds(_pauseAfterReset));
            _logger.LLog(Logs.RtuManager.ToInt(), "Charon reset finished");
            return ReturnCode.Ok;
        }
        catch (Exception e)
        {
            _logger.Log(LogLevel.Error, Logs.RtuManager.ToInt(), e.Message);
            return ReturnCode.CharonComPortError;
        }
    }

    public void ShowOnLedDisplay(LedDisplayCode code)
    {
        _logger.Log(LogLevel.Debug, Logs.RtuManager.ToInt(), $"Write <{code}> on led display");

        var serialPort = new SerialPort(_comPortName, _comPortSpeed);
        try
        {
            serialPort.Open();
            Thread.Sleep(1000);

            _logger.Log(LogLevel.Debug, Logs.RtuManager.ToInt(), $"{_comPortName} opened successfully.");
        }
        catch (Exception e)
        {
            _logger.Log(LogLevel.Error, Logs.RtuManager.ToInt(), $"Can't open {_comPortName}.  {e.Message}");
            return;
        }

        byte[] buffer = { (byte)code };
        try
        {
            serialPort.Write(buffer, 0, 1);
            Thread.Sleep(1000);
            _logger.Log(LogLevel.Debug, Logs.RtuManager.ToInt(), $"{(byte)code:X} sent successfully.");
            serialPort.Close();
            _logger.Log(LogLevel.Debug, Logs.RtuManager.ToInt(), $"{_comPortName} closed successfully.");
        }
        catch (Exception e)
        {
            _logger.Log(LogLevel.Error, Logs.RtuManager.ToInt(), $"Can't send to {_comPortName}.  {e.Message}");
        }
        Thread.Sleep(1000);
    }
}