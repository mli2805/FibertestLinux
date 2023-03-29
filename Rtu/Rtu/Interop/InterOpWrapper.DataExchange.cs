namespace Fibertest.Rtu;

public partial class InterOpWrapper
{
   
    public int GetSorDataSize(IntPtr sorData)
    {
        return CppImportDecl.GetSorSize(sorData);
    }

    public int GetSordata(IntPtr sorData, byte[] buffer, int bufferLength)
    {
        return CppImportDecl.GetSorData(sorData, buffer, bufferLength);
    }

    public IntPtr SetSorData(byte[] buffer)
    {
        return CppImportDecl.CreateSorPtr(buffer, buffer.Length);
    }

    public void FreeSorDataMemory(IntPtr sorData)
    {
        CppImportDecl.DestroySorPtr(sorData);
    }
}