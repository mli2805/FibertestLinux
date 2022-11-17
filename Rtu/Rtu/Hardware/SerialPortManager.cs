using System.IO.Ports;
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

    public SerialPortManager(IOptions<CharonConfig> config, ILogger<SerialPortManager> logger)
    {
        _logger = logger;
        _comPortName = config.Value.ComPortName ?? "/dev/ttyS1";
        _comPortSpeed = config.Value.ComPortSpeed;
    }

    public void ResetCharon()
    {
        try
        {
            var serialPort = new SerialPort(_comPortName, _comPortSpeed);
            serialPort.Open();
            _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(),$"Now RTS is {serialPort.RtsEnable}");
            serialPort.RtsEnable = !serialPort.RtsEnable;
            Thread.Sleep(10);
            _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(),$"Now RTS is {serialPort.RtsEnable}");
            serialPort.RtsEnable = !serialPort.RtsEnable;
            Thread.Sleep(10);
            _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(),$"Now RTS is {serialPort.RtsEnable}");
        }
        catch (Exception e)
        {
            _logger.Log(LogLevel.Error, Logs.RtuManager.ToInt(), e.Message);
        }
    }

    public void ShowOnLedDisplay(LedDisplayCode code)
    {
        _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(),$"Write <{code.ToString()}> on led display", 3);

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

        byte[] buffer = new byte[] { (byte)code };
        try
        {
            serialPort.Write(buffer, 0, 1);
            serialPort.Close();
        }
        catch (Exception e)
        {
            _logger.Log(LogLevel.Error, Logs.RtuManager.ToInt(), $"Can't send {_comPortName}.  {e.Message}");
        }
    }
}