using Microsoft.EntityFrameworkCore;

namespace Fibertest.DataCenter
{
    public interface IParameterizer
    {
        void Init();
        void LogSettings();
        DbContextOptions<FtDbContext> Options { get; }

    }
}
