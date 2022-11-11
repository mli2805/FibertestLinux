using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ComPortExperiment
{
    internal class Program
    {
        static void Main()
        {
            Console.WriteLine("Hello, World!");

            //SerialPort();
            TcpExp();

            Console.ReadKey();
        }

        private static void TcpExp()
        {
            var cmd = "get_rtu_number\r\n";
            var client = new TcpClient(AddressFamily.InterNetwork);
            client.Client.DualMode = false;
            client.Connect("192.168.88.101", 23);
            var nwStream = client.GetStream();
            byte[] bytesToSend = Encoding.ASCII.GetBytes(cmd);
            nwStream.Write(bytesToSend, 0, bytesToSend.Length);
            Thread.Sleep(TimeSpan.FromMilliseconds(200));
            byte[] bytesToRead = new byte[client.ReceiveBufferSize];
            int bytesRead = nwStream.Read(bytesToRead, 0, client.ReceiveBufferSize);
            var answer = Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);
            client.Close();
            Console.WriteLine(answer);
        }

        private static void SerialPort()
        {
            var serialPort = new SerialPort("COM2", 115200);
            try
            {
                serialPort.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            Console.WriteLine($"Now RTS is {serialPort.RtsEnable}");
            serialPort.RtsEnable = !serialPort.RtsEnable;
            Thread.Sleep(10);
            Console.WriteLine($"Now RTS is {serialPort.RtsEnable}");
            serialPort.RtsEnable = !serialPort.RtsEnable;
            Thread.Sleep(10);
            Console.WriteLine($"Now RTS is {serialPort.RtsEnable}");

            serialPort.Close();
        }
    }
}