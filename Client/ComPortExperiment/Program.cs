using System.IO.Ports;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
namespace ComPortExperiment
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            using IHost host = Host.CreateDefaultBuilder(args).Build();
            IConfiguration config = host.Services.GetRequiredService<IConfiguration>();

            string comPort = config.GetValue<string>("Settings:ComPortName") ?? "parameter not found";
            Console.WriteLine($"Hello, World! Com port is {comPort}");

            TcpExp();
            SerialPort(comPort);


            await host.RunAsync();
        }

        private static void TcpExp()
        {
            var cmd = "get_rtu_number\r\n";
            var client = new TcpClient(AddressFamily.InterNetwork); // можно без параметра, тогда использует ipv6 под Ubuntu Server, но работает
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

        private static void SerialPort(string comport)
        {
            var serialPort = new SerialPort(comport, 115200);

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