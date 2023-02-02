using System.Net.NetworkInformation;
using System.Text;
using Fibertest.Dto;

namespace Fibertest.Utils
{
    public static class Pinger
    {
        public static bool Ping(NetAddress address)
        {
            return address.IsAddressSetAsIp ? Ping(address.Ip4Address) : Ping(address.HostName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address">can be an IPaddress or host name</param>
        /// <param name="timeout">in ms</param>
        /// <returns></returns>
        public static bool Ping(string address, int timeout = 120)
        {
            var pingSender = new Ping();
            var options = new PingOptions { DontFragment = true };
            byte[] buffer = Encoding.ASCII.GetBytes(@"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
            PingReply reply = pingSender.Send(address, timeout, buffer, options);
            return reply.Status == IPStatus.Success;
        }

    }
}