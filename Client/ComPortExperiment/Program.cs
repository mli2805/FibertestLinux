using System.IO.Ports;

namespace ComPortExperiment
{
    internal class Program
    {
        static void Main()
        {
            Console.WriteLine("Hello, World!");

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