using Fibertest.StringResources;

namespace Fibertest.Graph;

public static class AccessModeFtExt
{
    public static string ToLocalizedString(this Dto.AccessModeFt accessMode)
    {
        switch (accessMode)
        {
            case Dto.AccessModeFt.ServerOnly:
                return Resources.SID_Server_only;
            case Dto.AccessModeFt.CacheOnly:
                return Resources.SID_Cache_only;
            //     case AccessModeFt.ServerAndCache:
            default:
                return Resources.SID_Server_and_cache;
        }
    }

    public static Dto.AccessModeFt FromEnumConstant(string str)
    {
        if (str == @"ServerOnly")
            return Dto.AccessModeFt.ServerOnly;
        if (str == @"CacheOnly")
            return Dto.AccessModeFt.CacheOnly;
        return Dto.AccessModeFt.ServerAndCache;
    } 
        
    public static Dto.AccessModeFt FromLocalizedString(string localizedString)
    {
        if (localizedString == Resources.SID_Server_only)
            return Dto.AccessModeFt.ServerOnly;
        if (localizedString == Resources.SID_Cache_only)
            return Dto.AccessModeFt.CacheOnly;
        return Dto.AccessModeFt.ServerAndCache;
    }
}