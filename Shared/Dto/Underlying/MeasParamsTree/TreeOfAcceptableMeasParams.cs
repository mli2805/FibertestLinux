namespace Fibertest.Dto
{
    [Serializable]
    public class TreeOfAcceptableMeasParams
    {
        public Dictionary<string, BranchOfAcceptableMeasParams> Units = new Dictionary<string, BranchOfAcceptableMeasParams>();

    }
}
