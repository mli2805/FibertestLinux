using Fibertest.Dto;
using Fibertest.Utils;
using Microsoft.Extensions.Options;

namespace Fibertest.DataCenter
{
    public class DiskSpaceProvider
    {
        private readonly MySqlDbInitializer _mySqlEventStoreInitializer;
        private readonly ILogger<DiskSpaceProvider> _logger;

        private readonly double _freeSpaceThresholdGb;

        public DiskSpaceProvider(MySqlDbInitializer mySqlEventStoreInitializer, 
            IOptions<MysqlConfig> mysqlConfig, ILogger<DiskSpaceProvider> logger)
        {
            _mySqlEventStoreInitializer = mySqlEventStoreInitializer;
            _logger = logger;

            _freeSpaceThresholdGb = mysqlConfig.Value.FreeSpaceThresholdGb;
        }

        public async Task<DiskSpaceDto>  GetDiskSpaceGb()
        {
            const double gb = 1024.0 * 1024 * 1024;
            var result = new DiskSpaceDto();
            //TODO: linux drive name
            var driveInfo = new DriveInfo(_mySqlEventStoreInitializer.DataDir!.Substring(0,1));
            result.TotalSize = driveInfo.TotalSize / gb;
            result.AvailableFreeSpace = driveInfo.AvailableFreeSpace / gb;
            result.FreeSpaceThreshold = _freeSpaceThresholdGb;
            result.DataSize = _mySqlEventStoreInitializer.GetDataSize() / gb;

            var totalSize = $"Drive {driveInfo.Name}'s size is {result.TotalSize:0.0}Gb";
            var freeSpace = $"free space is {result.AvailableFreeSpace:0.0}Gb";
            var threshold = $"threshold {result.FreeSpaceThreshold}Gb";
            var dbSize = $"DB size is {result.DataSize:0.0}Gb";
            _logger.LogInfo(Logs.DataCenter, $@"{totalSize},  {freeSpace},  {threshold},  {dbSize}");
            await Task.Delay(1);
            return result;
        }
    }
}