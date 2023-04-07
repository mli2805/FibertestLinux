using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.Utils;

namespace Fibertest.DataCenter
{
    public partial class C2DCommandsProcessor
    {
        private async Task RemoveEventsAndSors(RemoveEventsAndSors removeEventsAndSors, string username, string clientIp)
        {
            await _grpcToClient.SendRequest(new DbOptimizationProgressDto()
            {
                Stage = DbOptimizationStage.Starting,
            });

            _globalState.IsDatacenterInDbOptimizationMode = true;

            _logger.Info(Logs.DataCenter, "block is ON");
            var oldSize = _mySqlDbInitializer.GetDataSize() / 1e9;
            var unused = await ClearSor(removeEventsAndSors);

            await _grpcToClient.SendRequest(new DbOptimizationProgressDto{ Stage = DbOptimizationStage.ModelAdjusting});

            _logger.Info(Logs.DataCenter, "Removing from writeModel");
            await _eventStoreService.SendCommand(removeEventsAndSors, username, clientIp);

            _logger.Info(Logs.DataCenter, "Unblocking connections");
            _globalState.IsDatacenterInDbOptimizationMode = false;
            var newSize = _mySqlDbInitializer.GetDataSize() / 1e9;

            await _grpcToClient.SendRequest(new DbOptimizationProgressDto
            {
                Stage = DbOptimizationStage.OptimizationDone,
                OldSizeGb = oldSize, NewSizeGb = newSize,
            });
           
            await _grpcToClient.SendRequest(new ServerAsksClientToExitDto()
            {
                ToAll = true,
                Reason = UnRegisterReason.DbOptimizationFinished
            });
            await Task.Delay(1000);
            _clientCollection.CleanDeadClients(TimeSpan.FromMilliseconds(1));
        }

        private async Task<int> ClearSor(RemoveEventsAndSors cmd)
        {
            _logger.Info(Logs.DataCenter, "Start SorFiles cleaning");
            if (!cmd.IsMeasurementsNotEvents && !cmd.IsOpticalEvents) return 0;

            var ids = _writeModel.GetMeasurementsForDeletion(cmd.UpTo, cmd.IsMeasurementsNotEvents, cmd.IsOpticalEvents)
                .Select(m => m.SorFileId).ToArray();
            _logger.Info(Logs.DataCenter, $"{ids.Length} measurements chosen for deletion");
            await _grpcToClient.SendRequest(new DbOptimizationProgressDto
            {
                Stage = DbOptimizationStage.SorsRemoving,
                MeasurementsChosenForDeletion = ids.Length,
            });
            var count = await _sorFileRepository.RemoveManySorAsync(ids);
            _logger.Info(Logs.DataCenter, $"{count} measurements removed");

            await MySqlTableOptimization();
            return count;
        }

        private async Task MySqlTableOptimization()
        {
            var dir = _mySqlDbInitializer.DataDir;
            long oldSize = SorFileSize(dir!);
            _logger.Info(Logs.DataCenter, $"Optimization of sorfiles.ibd {oldSize:0,0} started");

            var unused = Task.Factory.StartNew(_mySqlDbInitializer.OptimizeSorFilesTable);
            _logger.Info(Logs.DataCenter, "Optimization process started on another thread");

            _logger.Info(Logs.DataCenter, "And we will check sorfiles.ibd and #sql-ib.....ibd size to know if optimization is finished");
            var oldProc = -5.0;
            while (true)
            {
                Thread.Sleep(2000);

                var files = new DirectoryInfo(dir + @"ft20efcore\").GetFiles();
                var fileInfo = files.FirstOrDefault(f => f.Name.StartsWith("#sql-ib"));
                if (fileInfo == null) break;
                var proc = fileInfo.Length * 100.0 / oldSize;
                await _grpcToClient.SendRequest(new DbOptimizationProgressDto
                {
                    Stage = DbOptimizationStage.TableCompressing,
                    TableOptimizationPercent = proc,
                });
                
                if (proc - oldProc > 5)
                {
                    _logger.Info(Logs.DataCenter, $"{fileInfo.Name}   {proc:0.0}%  copied");
                    oldProc = proc;
                }
            }
            var newSize = SorFileSize(dir!);
            _logger.Info(Logs.DataCenter, $"SorFiles table is optimized, new size is {newSize:0,0}, profit is {oldSize - newSize:0,0} bytes");

        }

        private long SorFileSize(string dir)
        {
            try
            {
                var sorFileInfo = new FileInfo(dir + @"ft20efcore\sorfiles.ibd");
                return sorFileInfo.Length;
            }
            catch (Exception e)
            {
                _logger.Exception(Logs.DataCenter, e, "Failed to get size of sorfiles.ibd: ");
            }

            return -1;
        }
    }
}
