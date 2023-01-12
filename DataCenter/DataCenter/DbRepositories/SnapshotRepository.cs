using Fibertest.Utils;
using Microsoft.EntityFrameworkCore;

namespace Fibertest.DataCenter;

public class SnapshotRepository
{
    private readonly ILogger<SnapshotRepository> _logger;
    private readonly IDbInitializer _dbInitializer;

    public SnapshotRepository(ILogger<SnapshotRepository> logger, IDbInitializer dbInitializer)
    {
        _logger = logger;
        _dbInitializer = dbInitializer;
    }

    // max_allowed_packet is 16M ???
    public async Task<int> AddSnapshotAsync(Guid graphDbVersionId, int lastEventNumber, DateTime lastEventDate, byte[] data)
    {
        try
        {
            await using var dbContext = new FtDbContext(_dbInitializer.FtDbContextOptions);

            _logger.LogInfo(Logs.DataCenter, "Snapshot adding...");
            var portion = 2 * 1024 * 1024;
            for (int i = 0; i <= data.Length / portion; i++)
            {
                var payload = data.Skip(i * portion).Take(portion).ToArray();
                var snapshot = new Snapshot()
                {
                    StreamIdOriginal = graphDbVersionId,
                    LastEventNumber = lastEventNumber,
                    LastEventDate = lastEventDate,
                    Payload = payload
                };
                dbContext.Snapshots.Add(snapshot);
                var result = await dbContext.SaveChangesAsync();
                if (result == 1)
                    _logger.LogInfo(Logs.DataCenter, $"{i+1} portion,   {payload.Length} size");
                else return -1;
            }
            return data.Length / portion;
        }
        catch (Exception e)
        {
            _logger.LogError(Logs.DataCenter, "AddSnapshotAsync: " + e.Message);
            return -1;
        }
    }

    public async Task<Tuple<int, byte[], DateTime>?> ReadSnapshotAsync(Guid graphDbVersionId)
    {
        try
        {
            await using var dbContext = new FtDbContext(_dbInitializer.FtDbContextOptions);

            _logger.LogInfo(Logs.DataCenter, "Snapshot reading...");
            var portions = await dbContext.Snapshots
                .Where(l => l.StreamIdOriginal == graphDbVersionId)
                .ToListAsync();
            if (!portions.Any())
            {
                _logger.LogInfo(Logs.DataCenter, "No snapshots");
                return null;
            }
            var size = portions.Sum(p => p.Payload.Length);
            var offset = 0;
            byte[] data = new byte[size];
            foreach (var t in portions)
            {
                t.Payload.CopyTo(data, offset);
                offset = offset + t.Payload.Length;
            }
            var result = new Tuple<int, byte[], DateTime>
                (portions.First().LastEventNumber, data, portions.First().LastEventDate);
            _logger.LogInfo(Logs.DataCenter, 
                $@"Snapshot size {result.Item2.Length:0,0} bytes.    Number of last event in snapshot {result.Item1:0,0}.");
            return result;
        }
        catch (Exception e)
        {
            _logger.LogError(Logs.DataCenter, "ReadSnapshotAsync: " + e.Message);
            return null;
        }
    }

     
    public async Task<int> RemoveOldSnapshots()
    {
        try
        {
            using (var dbContext = new FtDbContext(_dbInitializer.FtDbContextOptions))
            {
                _logger.LogInfo(Logs.DataCenter, "Snapshots removing...");
                var maxLastEventNumber = dbContext.Snapshots.Max(f => f.LastEventNumber); 
                var oldSnapshotPortions = dbContext.Snapshots.Where(f=>f.LastEventNumber != maxLastEventNumber).ToList();
                dbContext.Snapshots.RemoveRange(oldSnapshotPortions);
                return await dbContext.SaveChangesAsync();
            }
        }
        catch (Exception e)
        {
            _logger.LogError(Logs.DataCenter, "RemoveOldSnapshots: " + e.Message);
            return -1;
        }
    }


}