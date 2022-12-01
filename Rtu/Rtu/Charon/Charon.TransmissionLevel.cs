using System.Net.Sockets;
using System.Text;
using Fibertest.Utils;

namespace Fibertest.Rtu;

public partial class Charon
{
    private void SendCommand(string cmd)
    {
        Thread.Sleep(TimeSpan.FromMilliseconds(_pauseBetweenCommands));
        LastAnswer = "";
        LastErrorMessage = "";
        IsLastCommandSuccessful = false;
        try
        {
            var client = new TcpClient();
            var connection = client.BeginConnect(NetAddress.Ip4Address, NetAddress.Port, null, null);
            var success = connection.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(_connectionTimeout));
            if (!success)
            {
                LastErrorMessage = "Can't establish connection. Check connection timeout";
                _logger.Log(LogLevel.Error, Logs.RtuManager.ToInt(), LastErrorMessage);
                return;
            }
            client.SendTimeout = TimeSpan.FromSeconds(_writeTimeout).Milliseconds;
            client.ReceiveTimeout = TimeSpan.FromSeconds(_readTimeout).Milliseconds;

            NetworkStream nwStream = client.GetStream();
            byte[] bytesToSend = Encoding.ASCII.GetBytes(cmd);

            //---send the text---
            _logger.Log(LogLevel.Debug, Logs.RtuManager.ToInt(), $"Sending : {cmd.Trim()}");
            nwStream.Write(bytesToSend, 0, bytesToSend.Length);

            // for bulk command could be needed
            Thread.Sleep(TimeSpan.FromMilliseconds(200));

            //---read back the text---
            byte[] bytesToRead = new byte[client.ReceiveBufferSize];
            int bytesRead = nwStream.Read(bytesToRead, 0, client.ReceiveBufferSize);
            client.Close();
            LastAnswer = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
            _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), $"Received : {LastAnswer.Trim()}");
            IsLastCommandSuccessful = true;
        }
        catch (Exception e)
        {
            _logger.Log(LogLevel.Error, Logs.RtuManager.ToInt(), e.Message);
            LastErrorMessage = e.Message;
        }
    }

    private void SendWriteIniCommand(string content)
    {
        Thread.Sleep(TimeSpan.FromMilliseconds(_pauseBetweenCommands));
        LastAnswer = "";
        LastErrorMessage = "";
        IsLastCommandSuccessful = false;
        string cmd = "ini_write\r\n";
        try
        {
            //---create a TCPClient object at the IP and port no.---
            var client = new TcpClient();
            var connection = client.BeginConnect(NetAddress.Ip4Address, NetAddress.Port, null, null);
            var success = connection.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(4));
            if (!success)
            {
                LastErrorMessage = "Can't establish connection. Check connection timeout";
                _logger.Log(LogLevel.Error, Logs.RtuManager.ToInt(), LastErrorMessage);
                return;
            }
            client.SendTimeout = TimeSpan.FromSeconds(2).Milliseconds;
            client.ReceiveTimeout = TimeSpan.FromSeconds(4).Milliseconds;

            NetworkStream nwStream = client.GetStream();

            //---send the command---
            byte[] bytesToSend = Encoding.ASCII.GetBytes(cmd);
            _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), $"Sending : {cmd.Trim()}");
            nwStream.Write(bytesToSend, 0, bytesToSend.Length);

            //---read back the answer---
            byte[] bytesToRead = new byte[client.ReceiveBufferSize];
            int bytesRead = nwStream.Read(bytesToRead, 0, client.ReceiveBufferSize);
            LastAnswer = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
            _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), $"Received : {LastAnswer.Trim()}");

            //---send the content---
            byte[] contentBytes = new byte[CharonIniSize];
            var contentB = Encoding.ASCII.GetBytes(content);
            Array.Copy(contentB, contentBytes, contentB.Length);

            int rest = CharonIniSize;
            while (rest > 0)
            {
                if (rest >= 256)
                {
                    byte[] bytes256 = new byte[256];
                    Array.Copy(contentBytes, CharonIniSize - rest, bytes256, 0, 256);
                    nwStream.Write(bytes256, 0, bytes256.Length);
                    rest = rest - 256;
                }
                else
                {
                    byte[] bytes = new byte[rest];
                    Array.Copy(contentBytes, CharonIniSize - rest, bytes, 0, rest);
                    nwStream.Write(bytes, 0, bytes.Length);
                    rest = 0;
                }
            }

            Thread.Sleep(TimeSpan.FromMilliseconds(1000));

            //---read back the answer---
            bytesToRead = new byte[client.ReceiveBufferSize];
            bytesRead = nwStream.Read(bytesToRead, 0, client.ReceiveBufferSize);
            LastAnswer = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
            _logger.Log(LogLevel.Information, Logs.RtuManager.ToInt(), $"Received : {LastAnswer.Trim()}");

            client.Close();
            IsLastCommandSuccessful = true;
        }
        catch (Exception e)
        {
            _logger.Log(LogLevel.Error, Logs.RtuManager.ToInt(), e.Message);
            LastErrorMessage = e.Message;
        }

    }

}