using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.StringResources;
using Fibertest.Utils;
using GrpsClientLib;
using Microsoft.Extensions.Logging;

namespace KadastrLoader
{
    public class WellParser
    {
        private readonly ILogger _logger;
        private readonly KadastrDbProvider _kadastrDbProvider;
        private readonly LoadedAlready _loadedAlready;
        private readonly GrpcC2DService _grpcC2DService;

        public WellParser(ILogger logger, KadastrDbProvider kadastrDbProvider, 
             LoadedAlready loadedAlready, GrpcC2DService grpcC2DService)
        {
            _logger = logger;
            _kadastrDbProvider = kadastrDbProvider;
            _loadedAlready = loadedAlready;
            _grpcC2DService = grpcC2DService;
        }

        public void ParseWells(string folder, BackgroundWorker worker)
        {
            var count = 0;
            var filename = folder + @"\wells.csv";

            var lines = File.ReadAllLines(filename);
            worker.ReportProgress(0, string.Format(Resources.SID__0__lines_found_in_wells_csv, lines.Length));
            foreach (var line in lines)
            {
                if (ProcessOneLine(line) == null) count++;
            }

            worker.ReportProgress(0, string.Format(Resources.SID__0__wells_applied, count));
        }

        private string?  ProcessOneLine(string line)
        {
            var fields = line.Split(';');
            if (fields.Length < 5) return "invalid line";

            if (!int.TryParse(fields[0], out int inKadastrId)) return "invalid line";

            var existingWell = _loadedAlready.Wells.FirstOrDefault(w => w.InKadastrId == inKadastrId);

            var well = new Well()
            {
                InKadastrId = inKadastrId,
                InFibertestId = existingWell?.InFibertestId ?? Guid.NewGuid(),
            };
            _loadedAlready.Wells.Add(well);
            _kadastrDbProvider.AddWell(well).Wait();

            var cmd = CreateNodeCmd(fields, well.InFibertestId);
            var result = _grpcC2DService.SendEventSourcingCommand(cmd).Result;
            _logger.LogInfo(Logs.Client, result.ReturnCode != ReturnCode.Error
                ? $"Well {fields[1].Trim()} added successfully."
                : $"Failed to add well {fields[1].Trim()}.  {result.ErrorMessage}");
            return result.ErrorMessage;
        }

        private AddEquipmentAtGpsLocationWithNodeTitle CreateNodeCmd(string[] parts, Guid inFibertestId)
        {
            var cmd = new AddEquipmentAtGpsLocationWithNodeTitle();
            cmd.NodeId = inFibertestId;
            cmd.Title = parts[1].Trim();
            cmd.EmptyNodeEquipmentId = Guid.NewGuid();
            cmd.Type = EquipmentType.EmptyNode;

            var ds = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            double.TryParse(parts[2].Replace('.', ds[0]).Replace(',', ds[0]), out double xKadastr);
            cmd.Latitude = Latitude0 + xKadastr * Latitude1M;
            double.TryParse(parts[3].Replace('.', ds[0]).Replace(',', ds[0]), out double yKadastr);
            cmd.Longitude = Longitude0 + yKadastr * Longitude1M;

            return cmd;
        }

        // точка с кооординатами в кадастре 0,0 имеет такие координаты GPS
        private const double Latitude0 = 53.903103;
        private const double Longitude0 = 27.555008;

        // в одном метре столько градусов в районе Минска
        private const double Longitude1M = 15.228e-6;
        private const double Latitude1M = 8.981e-6;
    }
}