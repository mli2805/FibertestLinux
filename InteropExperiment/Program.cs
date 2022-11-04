using System.Runtime.InteropServices;
using System.Reflection;

namespace InteropExperiment
{
    internal class Program
    {
        [DllImport("OtdrMeasEngine/iit_otdr.so")]
        public static extern void DllInit(string path, IntPtr logFile, IntPtr lenUnit);

        public static void Main() 
        {
            var mainFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
            Console.WriteLine($"Main folder is {mainFolder}");
            var iitFolder = mainFolder + "/OtdrMeasEngine";
            Console.WriteLine($"IIT folder is {iitFolder}"); 

            var result = InitDll(iitFolder); // requires absolute path under vscode
            if (result)
                Console.WriteLine("Dlls are initialized successfully!");
        }

        public static bool InitDll(string folder)
        {
            string path = folder;
            IntPtr logFile = IntPtr.Zero;
            IntPtr lenUnit = IntPtr.Zero;

            try
            {
                DllInit(path, logFile, lenUnit);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }
    }
}