using System;
using System.Diagnostics;
using Caliburn.Micro;
using Fibertest.Dto;
using GrpsClientLib;

namespace WpfExperiment
{
    public class ShellViewModel : PropertyChangedBase, IShell
    {
        private readonly Class1 _class1;
        private readonly Class2 _class2;
        public string DcAddress { get; set; } = "192.168.96.109"; // virtualBox Ubuntu 20.04
        public string RtuAddress { get; set; } = "192.168.96.56"; // MAK 0068613

        public ShellViewModel(Class1 class1, Class2 class2)
        {
            _class1 = class1;
            _class2 = class2;
        }

        public async void InitializeOtdr()
        {
            Debug.WriteLine("Debug message");
            Console.WriteLine($@"{_class2.GetInt()}");

            var uri = $"http://{DcAddress}:{(int)TcpPorts.ServerListenToCommonClient}";

            var r = await _class1.F(uri);
            Console.WriteLine(r?.Omid ?? "null");
        }

       
    }
}
