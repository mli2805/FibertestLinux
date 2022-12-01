namespace Fibertest.Dto
{
    public class TraceHeaderDto
    {
        public string? TraceTitle;

        // for detached trace "-1";
        // for trace on bop use bop's serial plus port number "879151-3"
        public string? Port;

        public string? RtuTitle;
        public string? RtuVersion;
    }
}