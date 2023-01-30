using Fibertest.StringResources;
using GMap.NET;

namespace Fibertest.Graph;

public static class AccessModeExt
{
    public static string ToLocalizedString(this AccessMode accessMode)
    {
        switch (accessMode)
        {
            case AccessMode.ServerOnly:
                return Resources.SID_Server_only;
            case AccessMode.CacheOnly:
                return Resources.SID_Cache_only;
            //     case AccessModeFt.ServerAndCache:
            default:
                return Resources.SID_Server_and_cache;
        }
    }

    public static AccessMode FromEnumConstant(string str)
    {
        if (str == @"ServerOnly")
            return AccessMode.ServerOnly;
        if (str == @"CacheOnly")
            return AccessMode.CacheOnly;
        return AccessMode.ServerAndCache;
    } 
        
    public static AccessMode FromLocalizedString(string localizedString)
    {
        if (localizedString == Resources.SID_Server_only)
            return AccessMode.ServerOnly;
        if (localizedString == Resources.SID_Cache_only)
            return AccessMode.CacheOnly;
        return AccessMode.ServerAndCache;
    }
}