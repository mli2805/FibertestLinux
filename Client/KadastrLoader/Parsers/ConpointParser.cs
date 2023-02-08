using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.GrpcClientLib;
using Fibertest.StringResources;

namespace KadastrLoader
{
    public class ConpointParser
    {
        private readonly KadastrDbProvider _kadastrDbProvider;
        private readonly LoadedAlready _loadedAlready;
        private readonly GrpcC2DService _grpcC2DService;

        public ConpointParser(KadastrDbProvider kadastrDbProvider, LoadedAlready loadedAlready,
             GrpcC2DService grpcC2DService)
        {
            _kadastrDbProvider = kadastrDbProvider;
            _loadedAlready = loadedAlready;
            _grpcC2DService = grpcC2DService;
        }

        public void ParseConpoints(string folder, BackgroundWorker worker)
        {
            var count = 0;
            var filename = folder + @"\conpoints.csv";

            var lines = File.ReadAllLines(filename);
            var str = string.Format(Resources.SID__0__lines_found_in_conpoints_csv, lines.Length);
            worker.ReportProgress(0, str);
            foreach (var line in lines)
            {
                if (ProcessOneLine(line) == null) count++;
            }
            worker.ReportProgress(0, string.Format(Resources.SID__0__conpoints_applied, count));
        }

        private string? ProcessOneLine(string line)
        {
            var fields = line.Split(';');
            if (fields.Length < 4) return "invalid line";

            if (!int.TryParse(fields[3], out int conpointInKadastrId)) return "invalid line";
            if (!int.TryParse(fields[0], out int wellInKadastrId)) return "invalid line";
            if (_loadedAlready.Conpoints.FirstOrDefault(c => c.InKadastrId == conpointInKadastrId) != null)
                return "conpoint exists already";

            var conpoint = new Conpoint()
            {
                InKadastrId = conpointInKadastrId,
            };
            _loadedAlready.Conpoints.Add(conpoint);
            _kadastrDbProvider.AddConpoint(conpoint).Wait();

            if (!int.TryParse(fields[2], out int conType) || conType == 0) return "invalid line";
            
            // conType == 1 - fork
            // conType == 2 - joint closure

            var well = _loadedAlready.Wells.FirstOrDefault(w => w.InKadastrId == wellInKadastrId);
            if (well == null) return "no such well";
            var cmd = new AddEquipmentIntoNode();
            cmd.NodeId = well.InFibertestId;
            cmd.EquipmentId = Guid.NewGuid();
            cmd.Type = EquipmentType.Closure;

            var result = _grpcC2DService.SendEventSourcingCommand(cmd).Result;
            return result.ErrorMessage;
        }

    }
}