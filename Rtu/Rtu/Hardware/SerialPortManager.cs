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
        _comPortName = config.Value.ComPortName ?? "/dev/ttyS1";
        _comPortSpeed = config.Value.ComPortSpeed;
        _pauseAfterReset = config.Value.PauseAfterReset;
    }

    public ReturnCode ResetCharon()
    {
        try
        {
            var serialPort = new SerialPort(_comPortName, _comPortSpeed);
            _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), $"Com port {_comPortName}  speed {_comPortSpeed}");
            serialPort.Open();
            _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), $"{_comPortName} port opened successfully");

            _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(),$"Now RTS is {serialPort.RtsEnable}");
            serialPort.RtsEnable = !serialPort.RtsEnable;
            Thread.Sleep(10);
            _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(),$"Now RTS is {serialPort.RtsEnable}");
            serialPort.RtsEnable = !serialPort.RtsEnable;
            Thread.Sleep(10);
            _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(),$"Now RTS is {serialPort.RtsEnable}");
            serialPort.Close();

            _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), $"Pause after charon reset {_pauseAfterReset} seconds...");
            Thread.Sleep(TimeSpan.FromSeconds(_pauseAfterReset));
            _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), "Charon reset finished");
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
        _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(),$"Write <{code}> on led display", 3);

        var serialPort = new SerialPort(_comPortName, _comPortSpeed);
        try
        {
            serialPort.Open();
        }
        catch (Exception e)
        {
            _logger.Log(LogLevel.Error, Logs.RtuManager.ToInt(), $"Can't open {_comPortName}.  {e.Message}");
            return;
        }
        _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(),$"{_comPortName} opened successfully.", 0, 3);

        byte[] buffer = { (byte)code };
        try
        {
            serialPort.Write(buffer, 0, 1);
            serialPort.Close();
        }
        catch (Exception e)
        {
            _logger.Log(LogLevel.Error, Logs.RtuManager.ToInt(), $"Can't send to {_comPortName}.  {e.Message}");
        }
    }
}