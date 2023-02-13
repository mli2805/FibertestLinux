using Fibertest.Dto;

namespace Fibertest.WpfClient
{
    public class CommandLineParameters
    {
        public bool IsUnderSuperClientStart;
        public string? Username;
        public string? Password;
        public string? ConnectionId;

        public NetAddress? ServerNetAddress;
        public int ClientOrdinal;
        public string? SuperClientCulture;
        public string? ServerTitle;

    }
}