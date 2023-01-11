using System.Runtime.InteropServices;

namespace Fibertest.Rtu;

public partial class InterOpWrapper
{
    // EXTERN_C __declspec(dllexport) long GetSorSize(TSorData* sorData);
    [DllImport("iit_otdr.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetSorSize")]
    public static extern int GetSorSize(IntPtr sorData);

    // EXTERN_C __declspec(dllexport) long GetSorData(TSorData* sorData, char* buffer, long bufferLength);
    [DllImport("iit_otdr.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "GetSorData")]
    public static extern int GetSorData(IntPtr sorData, byte[] buffer, int bufferLength);


    // EXTERN_C __declspec(dllexport) TSorData* CreateSorPtr(const char* buffer, long bufferLength);
    [DllImport("iit_otdr.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "CreateSorPtr")]
    public static extern IntPtr CreateSorPtr(byte[] buffer, int bufferLength);

    // EXTERN_C __declspec(dllexport) void DestroySorPtr(TSorData* sorData);
    [DllImport("iit_otdr.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "DestroySorPtr")]
    public static extern void DestroySorPtr(IntPtr sorData);

    public int GetSorDataSize(IntPtr sorData)
    {
        return GetSorSize(sorData);
    }

    public int GetSordata(IntPtr sorData, byte[] buffer, int bufferLength)
    {
        return GetSorData(sorData, buffer, bufferLength);
    }

    public IntPtr SetSorData(byte[] buffer)
    {
        return CreateSorPtr(buffer, buffer.Length);
    }

    public void FreeSorDataMemory(IntPtr sorData)
    {
        DestroySorPtr(sorData);
    }
}