// ReSharper disable InconsistentNaming

namespace Fibertest.Dto
{
    // VeexOtauCascadingScheme myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class Connection
    {
        public string? outputOtauId;
        public int outputOtauPort;
        public string? inputOtauId;
        public int inputOtauPort;
    }

    public class RootConnection
    {
        public string? inputOtauId;
        public int inputOtauPort;
    }
    public class VeexOtauCascadingScheme
    {
        public List<Connection> connections;
        public List<RootConnection> rootConnections;

        public VeexOtauCascadingScheme()
        {
            rootConnections = new List<RootConnection>();
            connections = new List<Connection>();

        }

        public VeexOtauCascadingScheme(string mainOtauId)
        {
            rootConnections = new List<RootConnection>()
            {
                new RootConnection()
                {
                    inputOtauId = mainOtauId,
                    inputOtauPort = 0,
                }
            };
            connections = new List<Connection>();
        }
    }

}
