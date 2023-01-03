using Fibertest.Dto;
using Fibertest.Utils;
using Microsoft.EntityFrameworkCore;

namespace Fibertest.DataCenter
{
    public class RtuStationsRepository
    {
        private readonly ILogger<RtuStationsRepository> _logger;
        private readonly IDbInitializer _dbInitializer;

        public RtuStationsRepository(ILogger<RtuStationsRepository> logger, IDbInitializer dbInitializer)
        {
            _logger = logger;
            _dbInitializer = dbInitializer;
        }

        public async Task<int> RegisterRtuInitializationResultAsync(RtuStation rtuStation)
        {
            try
            {
                await using var dbContext = new FtDbContext(_dbInitializer.FtDbContextOptions);
                var previousRtuStationRow = dbContext.RtuStations.FirstOrDefault(r => r.RtuGuid == rtuStation.RtuGuid);
                if (previousRtuStationRow == null)
                {
                    dbContext.RtuStations.Add(rtuStation);
                    _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), 
                        $"RtuStation {rtuStation.RtuGuid.First6()} successfully registered with main address {rtuStation.MainAddress}.");
                }
                else
                {
                    previousRtuStationRow.Version = rtuStation.Version;
                    previousRtuStationRow.MainAddress = rtuStation.MainAddress;
                    previousRtuStationRow.MainAddressPort = rtuStation.MainAddressPort;
                    previousRtuStationRow.IsReserveAddressSet = rtuStation.IsReserveAddressSet;
                    previousRtuStationRow.ReserveAddress = rtuStation.ReserveAddress;
                    previousRtuStationRow.ReserveAddressPort = rtuStation.ReserveAddressPort;
                    _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), 
                        $"RtuStation {rtuStation.RtuGuid.First6()} successfully updated.");
                }

                return await dbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, Logs.DataCenter.ToInt(), "RegisterRtuInitializationResultAsync: " + e.Message);
                return -1;
            }
        }

        public async Task<string?> RemoveRtuAsync(Guid rtuId)
        {
            try
            {
                await using var dbContext = new FtDbContext(_dbInitializer.FtDbContextOptions);
                var rtu = dbContext.RtuStations.FirstOrDefault(r => r.RtuGuid == rtuId);
                if (rtu != null)
                {
                    dbContext.RtuStations.Remove(rtu);
                    await dbContext.SaveChangesAsync();
                    _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), "RTU removed.");
                    return null;
                }

                var message = $"RTU with id {rtuId.First6()} not found";
                _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), message);
                return message;
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, Logs.DataCenter.ToInt(), "RemoveRtuAsync: " + e.Message);
                return e.Message;
            }
        }

        public async Task<DoubleAddress?> GetRtuAddresses(Guid rtuId)
        {
            try
            {
                await using var dbContext = new FtDbContext(_dbInitializer.FtDbContextOptions);
                var rtu = await dbContext.RtuStations.FirstOrDefaultAsync(r => r.RtuGuid == rtuId);
                if (rtu != null)
                    return rtu.GetRtuDoubleAddress();

                _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), $"RTU with id {rtuId.First6()} not found");
                return null;
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), "GetRtuAddresses: " + e.Message);
                return null;
            }
        }

        public async Task<int> RegisterRtuHeartbeatAsync(RtuChecksChannelDto dto)
        {
            try
            {
                await using var dbContext = new FtDbContext(_dbInitializer.FtDbContextOptions);
                var rtu = dbContext.RtuStations.FirstOrDefault(r => r.RtuGuid == dto.RtuId);
                if (rtu == null)
                {
                    _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), $"Unknown RTU's {dto.RtuId.First6()} heartbeat.");
                }
                else
                {
                    if (dto.IsMainChannel)
                        rtu.LastConnectionByMainAddressTimestamp = DateTime.Now;
                    else
                        rtu.LastConnectionByReserveAddressTimestamp = DateTime.Now;
                    rtu.Version = dto.Version;
                }
                return await dbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, Logs.DataCenter.ToInt(), "RegisterRtuHeartbeatAsync: " + e.Message);
                return -1;
            }
        }

        public async Task<bool> IsRtuExist(Guid rtuId)
        {
            try
            {
                using (var dbContext = new FtDbContext(_dbInitializer.FtDbContextOptions))
                {
                    var rtu = await dbContext.RtuStations.FirstOrDefaultAsync(r => r.RtuGuid == rtuId);
                    if (rtu != null) return true;
                    _logger.Log(LogLevel.Information, Logs.DataCenter.ToInt(), $"Unknown RTU {rtuId.First6()}");
                    return false;
                }
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, Logs.DataCenter.ToInt(), "IsRtuExist: " + e.Message);
                return false;
            }
        }

        public async Task<List<RtuStation>> GetAllRtuStations()
        {
            try
            {
                await using var dbContext = new FtDbContext(_dbInitializer.FtDbContextOptions);
                return await dbContext.RtuStations.ToListAsync();
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, Logs.DataCenter.ToInt(), "GetAllRtuStations: " + e.Message);
                return new List<RtuStation>();
            }
        }

        public async Task<RtuStation?> GetRtuStation(Guid rtuId)
        {
            try
            {
                using (var dbContext = new FtDbContext(_dbInitializer.FtDbContextOptions))
                {
                    return await dbContext.RtuStations.FirstOrDefaultAsync(r => r.RtuGuid == rtuId);
                }
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, Logs.DataCenter.ToInt(), "GetRtuStation: " + e.Message);
                return null;
            }
        }

        public async Task<int> SaveAvailabilityChanges(List<RtuStation> changedStations)
        {
            try
            {
                await using var dbContext = new FtDbContext(_dbInitializer.FtDbContextOptions);
                foreach (var changedStation in changedStations)
                {
                    var rtuStation = dbContext.RtuStations.First(r => r.RtuGuid == changedStation.RtuGuid);
                    Cop(changedStation, rtuStation);
                }
                return await dbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, Logs.DataCenter.ToInt(), "SaveAvailabilityChanges: " + e.Message);
                return -1;
            }
        }

        private void Cop(RtuStation source, RtuStation destination)
        {
            destination.IsMainAddressOkDuePreviousCheck = source.IsMainAddressOkDuePreviousCheck;
            destination.IsReserveAddressOkDuePreviousCheck = source.IsReserveAddressOkDuePreviousCheck;
            destination.IsReserveAddressSet = source.IsReserveAddressSet;
            destination.LastConnectionByMainAddressTimestamp = source.LastConnectionByMainAddressTimestamp;
            destination.LastConnectionByReserveAddressTimestamp = source.LastConnectionByReserveAddressTimestamp;
            destination.MainAddress = source.MainAddress;
            destination.MainAddressPort = source.MainAddressPort;
            destination.ReserveAddress = source.ReserveAddress;
            destination.ReserveAddressPort = source.ReserveAddressPort;
            destination.RtuGuid = source.RtuGuid;
            destination.Version = source.Version;
            destination.LastMeasurementTimestamp = source.LastMeasurementTimestamp;
        }
    }
}