using System;
using System.Collections.Generic;
using System.Linq;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.StringResources;

namespace Fibertest.WpfClient
{
    public class TraceStateModelFactory
    {
        private readonly Model _readModel;
        private readonly AccidentLineModelFactory _accidentLineModelFactory;
        private readonly CurrentGis _currentGis;
        private readonly DataCenterConfig _currentDatacenterParameters;

        public TraceStateModelFactory(Model readModel, AccidentLineModelFactory accidentLineModelFactory, 
            CurrentGis currentGis, DataCenterConfig currentDatacenterParameters)
        {
            _readModel = readModel;
            _accidentLineModelFactory = accidentLineModelFactory;
            _currentGis = currentGis;
            _currentDatacenterParameters = currentDatacenterParameters;
        }

        // TraceLeaf
        // Trace statistics
        // Monitoring result notification
        public TraceStateModel CreateModel(Measurement measurement, bool isLastStateForThisTrace, bool isLastAccidentForThisTrace)
        {
            var model = new TraceStateModel
            {
                Header = PrepareHeader(measurement.TraceId),
                TraceId = measurement.TraceId,
                Trace = _readModel.Traces.First(t => t.TraceId == measurement.TraceId),
                TraceState = measurement.TraceState,
                BaseRefType = measurement.BaseRefType,
                MeasurementTimestamp = measurement.MeasurementTimestamp,
                RegistrationTimestamp = measurement.EventRegistrationTimestamp,
                SorFileId = measurement.SorFileId,
                EventStatus = measurement.EventStatus,
                Comment = measurement.Comment,

                IsLastStateForThisTrace = isLastStateForThisTrace,
                IsLastAccidentForThisTrace = isLastAccidentForThisTrace,
            };
            if (model.TraceState != FiberState.Ok)
                model.Accidents = PrepareAccidents(measurement.Accidents);
            return model;
        }

        // Optical events
        public TraceStateModel CreateModel(OpticalEventModel opticalEventModel, bool isLastStateForThisTrace, bool isLastAccidentForThisTrace)
        {
            try
            {
                TraceStateModel model = new TraceStateModel
                {
                    Header = PrepareHeader(opticalEventModel.TraceId),
                    TraceId = opticalEventModel.TraceId,
                    Trace = _readModel.Traces.First(t => t.TraceId == opticalEventModel.TraceId),
                    TraceState = opticalEventModel.TraceState,
                    BaseRefType = opticalEventModel.BaseRefType,
                    MeasurementTimestamp = opticalEventModel.MeasurementTimestamp,
                    RegistrationTimestamp = opticalEventModel.EventRegistrationTimestamp,
                    SorFileId = opticalEventModel.SorFileId,
                    EventStatus = opticalEventModel.EventStatus,
                    Accidents = PrepareAccidents(opticalEventModel.Accidents),
                    Comment = opticalEventModel.Comment,
                    IsLastStateForThisTrace = isLastStateForThisTrace,
                    IsLastAccidentForThisTrace = isLastAccidentForThisTrace
                };
                return model;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private List<AccidentLineModel> PrepareAccidents(List<AccidentOnTraceV2> accidents)
        {
            var lines = new List<AccidentLineModel>();
            for (var i = 0; i < accidents.Count; i++)
            {
                lines.Add(_accidentLineModelFactory.Create(accidents[i], i + 1, _currentGis.IsGisOn, _currentGis.GpsInputMode));
            }
            return lines;
        }

        private TraceStateModelHeader PrepareHeader(Guid traceId)
        {
            var result = new TraceStateModelHeader();
            var trace = _readModel.Traces.FirstOrDefault(t => t.TraceId == traceId);
            if (trace == null)
                return result;

            result.TraceTitle = trace.Title;
            var rtu = _readModel.Rtus.FirstOrDefault(r => r.Id == trace.RtuId);
            result.RtuPosition = _readModel.Nodes.FirstOrDefault(n => n.NodeId == rtu?.NodeId)?.Position;
            result.RtuTitle = rtu?.Title;
            result.PortTitle = trace.OtauPort == null ? Resources.SID__not_attached_ : trace.OtauPort.IsPortOnMainCharon
                ? trace.OtauPort.OpticalPort.ToString()
                : $@"{trace.OtauPort.MainCharonPort}-{trace.OtauPort.OpticalPort}";
            result.RtuSoftwareVersion = rtu?.Version;

            result.ServerTitle = _currentDatacenterParameters.General.ServerTitle;
            return result;
        }
    }
}