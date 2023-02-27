using Fibertest.Utils;
using Microsoft.EntityFrameworkCore;

namespace Fibertest.DataCenter
{
    public class SorFileRepository
    {
        private readonly IDbInitializer _dbInitializer;
        private readonly ILogger<SorFileRepository> _logger;

        public SorFileRepository(IDbInitializer dbInitializer, ILogger<SorFileRepository> logger)
        {
            _dbInitializer = dbInitializer;
            _logger = logger;
        }

        public async Task<int> AddSorBytesAsync(byte[] sorBytes)
        {
            try
            {
                await using var dbContext = new FtDbContext(_dbInitializer.FtDbContextOptions);
                var sorFile = new SorFile() { SorBytes = sorBytes };
                dbContext.SorFiles.Add(sorFile);
                await dbContext.SaveChangesAsync();

                return sorFile.Id;
            }
            catch (Exception e)
            {
                _logger.Error(Logs.DataCenter, "AddSorBytesAsync: " + e.Message);
                return -1;
            }
        }

        public async Task<List<int>?> AddMultipleSorBytesAsync(List<byte[]> sors)
        {
            try
            {
                await using var dbContext = new FtDbContext(_dbInitializer.FtDbContextOptions);
                var sorFiles = sors.Select(s => new SorFile() { SorBytes = s }).ToList();
                dbContext.SorFiles.AddRange(sorFiles);
                await dbContext.SaveChangesAsync();

                return sorFiles.Select(s => s.Id).ToList();
            }
            catch (Exception e)
            {
                _logger.Error(Logs.DataCenter, "AddMultipleSorBytesAsync: " + e.Message);
                return null;
            }
        }

        public async Task<int> UpdateSorBytesAsync(int sorFileId, byte[] sorBytes)
        {
            try
            {
                await using var dbContext = new FtDbContext(_dbInitializer.FtDbContextOptions);
                var record = await dbContext.SorFiles.Where(s => s.Id == sorFileId).FirstOrDefaultAsync();
                if (record == null) return -1;
                record.SorBytes = sorBytes;
                return await dbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.Error(Logs.DataCenter, "UpdateSorBytesAsync: " + e.Message);
                return -1;
            }
        }

        public async Task<byte[]?> GetSorBytesAsync(int sorFileId)
        {
            try
            {
                await using var dbContext = new FtDbContext(_dbInitializer.FtDbContextOptions);
                var result = await dbContext.SorFiles.Where(s => s.Id == sorFileId).FirstOrDefaultAsync();
                return result?.SorBytes;
            }
            catch (Exception e)
            {
                _logger.Error(Logs.DataCenter, "GetSorBytesAsync: " + e.Message);
                return null;
            }
        }

        public async Task<int> RemoveSorBytesAsync(int sorFileId)
        {
            try
            {
                await using var dbContext = new FtDbContext(_dbInitializer.FtDbContextOptions);
                var result = await dbContext.SorFiles.Where(s => s.Id == sorFileId).FirstOrDefaultAsync();
                if (result == null) return -1;
                dbContext.SorFiles.Remove(result);
                return await dbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.Error(Logs.DataCenter, "RemoveSorBytesAsync: " + e.Message);
                return -1;
            }
        }

        public async Task<int> RemoveManySorAsync(int[] sorIds)
        {
            try
            {
                await using var dbContext = new FtDbContext(_dbInitializer.FtDbContextOptions);
                var sors = dbContext.SorFiles.Where(s => sorIds.Contains(s.Id)).ToList();
                dbContext.SorFiles.RemoveRange(sors);
                var recordsAffected = await dbContext.SaveChangesAsync();
                return recordsAffected;
            }
            catch (Exception e)
            {
                _logger.Error(Logs.DataCenter, "RemoveManySorAsync: " + e.Message);
                return -1;
            }
        }

    }
}