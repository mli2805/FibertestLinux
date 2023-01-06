using Microsoft.EntityFrameworkCore;
using NEventStore;

namespace Fibertest.DataCenter
{
    public interface IDbInitializer
    {
        public string EsConnectionString { get; } // Event sourcing
        DbContextOptions<FtDbContext> FtDbContextOptions { get; } // other tables

        string ConnectionLogLine { get; }
        string? DataDir {get; }
        Guid GetStreamIdIfExists();
        IStoreEvents Init();
        long GetDataSize();
        int OptimizeSorFilesTable();
        int RemoveCommitsIncludedIntoSnapshot(int lastEventNumber);

        void DropDatabase();
    }
}