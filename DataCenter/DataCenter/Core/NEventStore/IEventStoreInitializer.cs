using NEventStore;

namespace Fibertest.DataCenter
{
    public interface IEventStoreInitializer
    {
        string? DataDir {get; }
        Guid GetStreamIdIfExists();
        IStoreEvents Init();
        long GetDataSize();
        int OptimizeSorFilesTable();
        int RemoveCommitsIncludedIntoSnapshot(int lastEventNumber);

        void DropDatabase();
    }
}