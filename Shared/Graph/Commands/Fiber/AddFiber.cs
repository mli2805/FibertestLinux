namespace Fibertest.Graph;

public class AddFiber
{
    public Guid FiberId;
    public Guid NodeId1;
    public Guid NodeId2;


    public AddFiber(Guid nodeId1, Guid nodeId2)
    {
        FiberId = Guid.NewGuid();

        NodeId1 = nodeId1;
        NodeId2 = nodeId2;
    }
}