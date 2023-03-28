using System.Net;
using System.Net.Sockets;
using Fibertest.Utils;
using SnmpSharpNet;

namespace Fibertest.DataCenter;

public class SnmpEngine2
{
    private readonly ILogger<SnmpEngine2> _logger;
    private readonly TrapParser _trapParser;
    private readonly OutOfTurnRequestBuilder _outOfTurnRequestBuilder;
    private readonly OutOfTurnData _outOfTurnData;

    public SnmpEngine2(ILogger<SnmpEngine2> logger, TrapParser trapParser, 
        OutOfTurnRequestBuilder outOfTurnRequestBuilder, OutOfTurnData outOfTurnData)
    {
        _logger = logger;
        _trapParser = trapParser;
        _outOfTurnRequestBuilder = outOfTurnRequestBuilder;
        _outOfTurnData = outOfTurnData;
    }

    public async Task Start(CancellationToken token)
    {
            _logger.Info(Logs.SnmpTraps, "SNMP engine starts");
        try
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            EndPoint ep = new IPEndPoint(IPAddress.Any, 162);
            socket.Bind(ep);
            // Disable timeout processing. Just block until packet is received
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 0);

            while (!token.IsCancellationRequested)
            {
                byte[] inData = new byte[16 * 1024];
                EndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 0);
                try
                {
                    var inLen = socket.ReceiveFrom(inData, ref ipEndPoint);
                    await ProcessData(inData, inLen, ipEndPoint);
                }
                catch (Exception ex)
                {
                    _logger.Error(Logs.SnmpTraps, $"Exception {ex.Message}");
                }
            }
        }
        catch (Exception e)
        {
            _logger.Error(Logs.SnmpTraps, $"Failed to start listen to port 162. {e.Message}");
        }
    }

    private async Task ProcessData(byte[] inData, int inLen, EndPoint endPoint)
    {
        await Task.Delay(1);
        if (inLen > 0)
        {
            // Check protocol version int
            int ver = SnmpPacket.GetProtocolVersion(inData, inLen);
            if (ver == (int)SnmpVersion.Ver1)
            {
                SnmpV1TrapPacket pkt = new SnmpV1TrapPacket();
                pkt.decode(inData, inLen);
                _logger.LogSnmpVersion1TrapPacket(pkt, endPoint);
            }
            else
            {
                SnmpV2Packet pkt = new SnmpV2Packet();
                pkt.decode(inData, inLen);
                _logger.LogSnmpVersion2TrapPacket(pkt, endPoint);
                var parsedTrap = _trapParser.ParseTrap(pkt, endPoint);
                if (parsedTrap == null) return;
                var dto = _outOfTurnRequestBuilder.BuildDto(parsedTrap);
                if (dto == null) return;

                _outOfTurnData.AddNewRequest(dto);
            }
        }
        else
        {
            if (inLen == 0)
                _logger.Error(Logs.SnmpTraps, "Zero length packet received.");
        }
    }
}