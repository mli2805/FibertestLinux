using Fibertest.Dto;
using Fibertest.Graph;

namespace Fibertest.WpfClient
{
    public class ComponentsReportProvider
    {
        private readonly Model _readModel;
        private readonly TreeOfRtuModel _tree;
        private readonly DataCenterConfig _server;
        private ComponentsReportModel _reportModel = null!;

        public ComponentsReportProvider(Model readModel, TreeOfRtuModel tree,
            DataCenterConfig server)
        {
            _readModel = readModel;
            _tree = tree;
            _server = server;
        }

        public string? Create(ComponentsReportModel reportModel)
        {
            _reportModel = reportModel;

            return null;
        }
    }
}
