using Microsoft.EntityFrameworkCore;

namespace Fibertest.DataCenter
{
    public class FtDbContext : DbContext
    {
        public FtDbContext()  { }
        public FtDbContext(DbContextOptions<FtDbContext> options) : base(options) { }


        public DbSet<RtuStation> RtuStations => Set<RtuStation>();
        public DbSet<SorFile> SorFiles => Set<SorFile>();
        public DbSet<Snapshot> Snapshots => Set<Snapshot>();
    }
}
