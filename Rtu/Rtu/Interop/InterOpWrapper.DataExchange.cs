using System.Runtime.InteropServices;

namespace Fibertest.Rtu;

public partial class InterOpWrapper
{
    [DllImport("OtdrMeasEngine/iit_otdr.so")]
    public static extern int GetSorSize(IntPtr sorData);

    [DllImport("OtdrMeasEngine/iit_otdr.so")]
    public static extern int GetSorData(IntPtr sorData, byte[] buffer, int bufferLength);


    [DllImport("OtdrMeasEngine/iit_otdr.so")]
    public static extern IntPtr CreateSorPtr(byte[] buffer, int bufferLength);

    [DllImport("OtdrMeasEngine/iit_otdr.so")]
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