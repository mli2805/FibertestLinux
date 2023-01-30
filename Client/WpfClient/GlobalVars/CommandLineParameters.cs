using Fibertest.Dto;

namespace Fibertest.WpfClient
{
    public class CommandLineParameters
    {
        public bool IsUnderSuperClientStart { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string ConnectionId { get; set; }

        public NetAddress ServerNetAddress { get; set; }
        public int ClientOrdinal { get; set; }
        public string SuperClientCulture { get; set; }
        public string ServerTitle { get; set; }

    }
}