using Fibertest.StringResources;

namespace Fibertest.Graph
{
    public static class BoolExt
    {
        public static string ToYesNo(this bool value)
        {
            return value ? Resources.SID_yes : Resources.SID_no;
        }
    }
}
