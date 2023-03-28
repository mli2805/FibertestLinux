using Fibertest.Graph;
using Microsoft.EntityFrameworkCore;

namespace KadastrLoader
{
    public class KadastrDbContext : DbContext
    {
        public KadastrDbContext(DbContextOptions<KadastrDbContext> options) : base(options) { }

        public DbSet<Well> Wells => Set<Well>();
        public DbSet<Conpoint> Conpoints => Set<Conpoint>();
    }
}
