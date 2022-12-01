// ReSharper disable InconsistentNaming
namespace Fibertest.Dto;

public class VeexOtauAddress
{
    public string? address;
    public int port;
}
    
public class NewOtau
{
    public string? id;
    public VeexOtauAddress? connectionParameters;
}