using System.Runtime.InteropServices;

namespace Fibertest.Rtu
{
    public static class LinuxImportDecl
    {
        private const string LibFileName = "OtdrMeasEngine/iit_otdr.so";


        [DllImport(LibFileName)]
        public static extern void DllInit(string path, IntPtr logFile, IntPtr lenUnit);

        [DllImport(LibFileName)]
        public static extern int ServiceFunction(int cmd, ref int prm1, ref IntPtr prm2);

        [DllImport(LibFileName)]
        public static extern int InitOTDR(int type, string ip, int port);

    } 
    
    public static class WindowsImportDecl
    {
        private const string LibFileName = "OtdrMeasEngine/iit_otdr.dll";
       
        [DllImport(LibFileName)]
        public static extern void DllInit(string path, IntPtr logFile, IntPtr lenUnit);

        [DllImport(LibFileName)]
        public static extern int ServiceFunction(int cmd, ref int prm1, ref IntPtr prm2);

        [DllImport(LibFileName)]
        public static extern int InitOTDR(int type, string ip, int port);

    }
}
