using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.Utils;
using NEventStore;

namespace Fibertest.DataCenter
{
    public partial class C2DCommandsProcessor
    {
        private async Task MakeSnapshot(MakeSnapshot cmd, string username, string clientIp)
        {
            await _grpcToClient.SendRequest(new DbOptimizationProgressDto()
            {
                Stage = DbOptimizationStage.Starting,
            });

            _globalState.IsDatacenterInDbOptimizationMode = true;
            var tuple = await CreateModelUptoDate(cmd.UpTo);
            var data = await tuple.Item2.Serialize(_logger);

            var portionCount = await _snapshotRepository.AddSnapshotAsync(
                _eventStoreService.StreamIdOriginal, tuple.Item1, cmd.UpTo.Date, data!);
            if (portionCount == -1) return;
            var removedSnapshotPortions = await _snapshotRepository.RemoveOldSnapshots();
            _logger.Info(Logs.DataCenter, $"{removedSnapshotPortions} portions of old snapshot removed");

            await DeleteOldCommits(tuple.Item1);
            _logger.Info(Logs.DataCenter, "Deleted commits included into snapshot");

            _eventStoreService.LastEventNumberInSnapshot = tuple.Item1;
            _eventStoreService.LastEventDateInSnapshot = cmd.UpTo.Date;
      
            var result = await _eventStoreService.SendCommand(cmd, username, clientIp);
            if (result != null)
                _logger.Info(Logs.DataCenter, result);

            _globalState.IsDatacenterInDbOptimizationMode = false;

            await _grpcToClient.SendRequest(new DbOptimizationProgressDto{ Stage = DbOptimizationStage.SnapshotDone});
        }

        private async Task DeleteOldCommits(int lastEventNumber)
        {
            _eventStoreService.StoreEvents!.Dispose();
            _mySqlDbInitializer.RemoveCommitsIncludedIntoSnapshot(lastEventNumber);
            _eventStoreService.StoreEvents = _mySqlDbInitializer.Init();
            await Task.Delay(1000);
        }

        private async Task<Tuple<int, Model>> CreateModelUptoDate(DateTime date)
        {
            var modelForSnapshot = new Model();

            // TODO block RTU events appearance

            var snapshot = await _snapshotRepository.ReadSnapshotAsync(_eventStoreService.StreamIdOriginal);
            var lastIncludedEvent = snapshot!.Item1;
            if (lastIncludedEvent != 0)
            {
                var unused = await modelForSnapshot.Deserialize(_logger, snapshot.Item2);
            }
            var eventStream = _eventStoreService.StoreEvents.OpenStream(_eventStoreService.StreamIdOriginal);
            var events = eventStream.CommittedEvents
                .Where(x => ((DateTime)x.Headers["Timestamp"]).Date <= date.Date)
                .ToList();
            _logger.Info(Logs.DataCenter, $"{events.Count} events should be applied...");

            // -1 because one last event should be left in commits table in order to start correct numeration
            for (int i = 0; i < events.Count - 1; i++)
            {
                modelForSnapshot.Apply(events[i].Body);
                lastIncludedEvent++;
                if (lastIncludedEvent % 1000 == 0)
                {
                    _logger.Info(Logs.DataCenter, $"{lastIncludedEvent}");

                    await _grpcToClient.SendRequest(new DbOptimizationProgressDto()
                    {
                        Stage = DbOptimizationStage.ModelCreating,
                        EventsReplayed = lastIncludedEvent,
                    });
                }
            }

            _logger.Info(Logs.DataCenter, $"last included event {lastIncludedEvent}");
            var result = new Tuple<int, Model>(lastIncludedEvent, modelForSnapshot);
            return result;
        }
    }
}
