using System.Runtime.InteropServices;

namespace Fibertest.Rtu
{
    public static class CppImportDecl
    {
        // https://blog.magnusmontin.net/2018/11/05/platform-conditional-compilation-in-net-core/
        #if Linux
        private const string LibFileName = "OtdrMeasEngine/iit_otdr.so";
        #elif Windows
        private const string LibFileName = "OtdrMeasEngine/iit_otdr.dll";
        #endif


        [DllImport(LibFileName)]
        public static extern void DllInit(string path, IntPtr logFile, IntPtr lenUnit);

        [DllImport(LibFileName)]
        public static extern int ServiceFunction(int cmd, ref int prm1, ref IntPtr prm2);

        [DllImport(LibFileName)]
        public static extern int InitOTDR(int type, string ip, int port);

        [DllImport(LibFileName)]
        public static extern int GetSorSize(IntPtr sorData);

        [DllImport(LibFileName)]
        public static extern int GetSorData(IntPtr sorData, byte[] buffer, int bufferLength);


        [DllImport(LibFileName)]
        public static extern IntPtr CreateSorPtr(byte[] buffer, int bufferLength);

        [DllImport(LibFileName)]
        public static extern void DestroySorPtr(IntPtr sorData);

        [DllImport(LibFileName)]
        public static extern int MeasPrepare(int measurementMode);

        [DllImport(LibFileName)]
        public static extern int MeasStep(ref IntPtr sorData);

        [DllImport(LibFileName)]
        public static extern int MeasStop(ref IntPtr sorData, int isImmediateStop);
    }
}
