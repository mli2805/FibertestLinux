using System.ComponentModel;

namespace KadastrLoader
{
    public class KadastrFilesParser
    {
        private readonly WellParser _wellParser;
        private readonly ChannelParser _channelParser;
        private readonly ConpointParser _conpointParser;

        public KadastrFilesParser(WellParser wellParser, ChannelParser channelParser, ConpointParser conpointParser)
        {
            _wellParser = wellParser;
            _channelParser = channelParser;
            _conpointParser = conpointParser;
        }

        public void Run(string folder, BackgroundWorker worker)
        {
            _wellParser.ParseWells(folder, worker);
            _channelParser.ParseChannels(folder, worker);
            _conpointParser.ParseConpoints(folder, worker);
        }
    }
}