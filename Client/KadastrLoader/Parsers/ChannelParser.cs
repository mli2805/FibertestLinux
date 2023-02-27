using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.GrpcClientLib;
using Fibertest.StringResources;
using Fibertest.Utils;
using Microsoft.Extensions.Logging;

namespace KadastrLoader
{
    public class ChannelParser
    {
        private readonly ILogger _logger;
        private readonly LoadedAlready _loadedAlready;
        private readonly GrpcC2DService _grpcC2DService;

        public ChannelParser(ILogger logger, LoadedAlready loadedAlready,
            GrpcC2DService grpcC2DService)
        {
            _logger = logger;
            _loadedAlready = loadedAlready;
            _grpcC2DService = grpcC2DService;
        }

        public void ParseChannels(string folder, BackgroundWorker worker)
        {
            var count = 0;
            var filename = folder + @"\channels.csv";
            var lines = File.ReadAllLines(filename);
            worker.ReportProgress(0, string.Format(Resources.SID__0__lines_found_in_channels_csv, lines.Length));
            foreach (var line in lines)
            {
                if (ProcessOneLine(line) == null) count++;
            }

            worker.ReportProgress(0, string.Format(Resources.SID__0__channels_applied, count));
        }

        private string? ProcessOneLine(string line)
        {
            var fields = line.Split(';');
            if (fields.Length < 4) return "invalid line";

            var cmd = CreateFiberCmd(fields);
            if (cmd == null) return "invalid line";
            _logger.Info(Logs.Client, $"command create fiber {cmd.FiberId.First6()} between: {cmd.NodeId1.First6()} and {cmd.NodeId2.First6()}");

            var result = _grpcC2DService.SendEventSourcingCommand(cmd).Result;
            if (result.ReturnCode == ReturnCode.Error) 
                return result.ErrorMessage;

            _logger.Info(Logs.Client, result.ReturnCode != ReturnCode.Error
                ? "Fiber added successfully."
                : $"Failed to add fiber. {result.ErrorMessage}");
            return result.ErrorMessage;
        }

        private AddFiber? CreateFiberCmd(string[] parts)
        {
            if (!int.TryParse(parts[0], out int inKadastrIdL)) return null;
            var wellL = _loadedAlready.Wells.FirstOrDefault(w => w.InKadastrId == inKadastrIdL);
            if (wellL == null)
                return null;

            if (!int.TryParse(parts[1], out int inKadastrIdR)) return null;
            var wellR = _loadedAlready.Wells.FirstOrDefault(w => w.InKadastrId == inKadastrIdR);
            if (wellR == null)
                return null;

            return new AddFiber(wellL.InFibertestId, wellR.InFibertestId) { FiberId = Guid.NewGuid() };
        }
    }
}