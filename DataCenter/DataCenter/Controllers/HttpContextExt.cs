namespace Fibertest.DataCenter
{
    public static class HttpContextExt
    {
        public static string GetRemoteAddress(this HttpContext context, string localIpAddress)
        {
            var ip1 = context.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            // browser started on the same pc as this service
            return ip1 == "::1" || ip1 == "127.0.0.1" ? localIpAddress : ip1;
        }
    }
}
