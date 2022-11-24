using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Fibertest.Utils;

namespace Fibertest.DataCenter;

public class SnmpTrapListenerService : BackgroundService
{
    private readonly ILogger<SnmpTrapListenerService> _logger;
    private readonly TrapDataProcessor _trapDataProcessor;
    private int _counter;

    public SnmpTrapListenerService(ILogger<SnmpTrapListenerService> logger, TrapDataProcessor trapDataProcessor)
    {
        _logger = logger;
        _trapDataProcessor = trapDataProcessor;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var pid = Process.GetCurrentProcess().Id;
        var tid = Thread.CurrentThread.ManagedThreadId;
        _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), $"SNMP trap listener service started. Process {pid}, thread {tid}. See trap-DATE.log");
        _logger.Log(LogLevel.Information, Logs.SnmpTraps.ToInt(), Environment.NewLine + Environment.NewLine + new string('-', 80));
        _logger.Log(LogLevel.Information, Logs.SnmpTraps.ToInt(), $"SNMP trap listener service started. Process {pid}, thread {tid}");
        await Listen(stoppingToken);
    }

    private async Task Listen(CancellationToken stoppingToken)
    {
        try
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            EndPoint ep = new IPEndPoint(IPAddress.Any, 162);
            socket.Bind(ep);
            // Disable timeout processing. Just block until packet is received
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 0);

            while (!stoppingToken.IsCancellationRequested)
            {
                byte[] inData = new byte[16 * 1024];
                EndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 0);
                try
                {
                    var inLen = socket.ReceiveFrom(inData, ref ipEndPoint);
                    _counter++;
                    _logger.Log(LogLevel.Information, Logs.SnmpTraps.ToInt(), $"SNMP trap # {_counter}");
                    await _trapDataProcessor.ProcessData(inData, inLen, ipEndPoint);

                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Error, Logs.SnmpTraps.ToInt(), $"SNMP trap listener: {ex.Message}");
                }
            }
        }
        catch (Exception e)
        {
            _logger.Log(LogLevel.Error, Logs.SnmpTraps.ToInt(), $"socket.SetSocketOption: Exception: {e.Message}");
        }
    }
}