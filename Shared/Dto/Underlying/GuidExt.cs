namespace Fibertest.Dto;

public static class GuidExt
{
    public static string First6(this Guid guid)
    {
        return guid.ToString().Substring(0, 6);
    }
}