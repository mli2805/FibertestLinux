using Fibertest.Graph;

namespace Fibertest.DataCenter
{
    public partial class C2DCommandsProcessor
    {
        private async Task MakeSnapshot(MakeSnapshot cmd, string username, string clientIp)
        {

        }

        private void DeleteOldCommits(int lastEventNumber)
        {

        }

        private async Task<Tuple<int, Model>> CreateModelUptoDate(DateTime date)
        {
            var result = new Tuple<int, Model>(0, new Model());
            return result;
        }
    }
}
