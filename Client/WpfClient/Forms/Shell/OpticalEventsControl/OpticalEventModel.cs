using System;
using System.Collections.Generic;
using System.Windows.Media;
using Fibertest.Dto;
using Fibertest.Graph;

namespace Fibertest.WpfClient
{
    public class OpticalEventModel
    {
        public int Nomer { get; set; }
        public DateTime MeasurementTimestamp { get; set; }
        public DateTime EventRegistrationTimestamp { get; set; }
        public string RtuTitle { get; set; } = null!;
        public Guid RtuId { get; set; }

        public Guid TraceId { get; set; }
        public string TraceTitle { get; set; } = null!;

        public BaseRefType BaseRefType { get; set; }
        public FiberState TraceState { get; set; }


        public Brush BaseRefTypeBrush =>
            TraceState == FiberState.Ok
                ? Brushes.White
                : BaseRefType == BaseRefType.Fast
                    ? Brushes.Yellow
                    : TraceState.GetBrush(isForeground: false);

        public string TraceStateOnScreen => BaseRefType == BaseRefType.Fast && TraceState != FiberState.Ok
            ? FiberState.Suspicion.ToLocalizedString()
            : TraceState.ToLocalizedString();


        public EventStatus EventStatus { get; set; }

        public Brush EventStatusBrush => GetBrush();
        public string EventStatusInTable => EventStatus.GetLocalizedString();

        public string? StatusChangedTimestamp { get; set; }
        public string? StatusChangedByUser { get; set; }
        public string? Comment { get; set; }

        public List<AccidentOnTraceV2>? Accidents { get; set; }
        public int SorFileId { get; set; }

        private Brush GetBrush()
        {
            switch (EventStatus)
            {
                case EventStatus.Confirmed: return Brushes.Red;
                case EventStatus.Suspended:
                case EventStatus.Unprocessed: return Brushes.LightSkyBlue;
                case EventStatus.NotConfirmed:
                case EventStatus.NotImportant:
                case EventStatus.Planned: return Brushes.Transparent;
            }
            return Brushes.Transparent;
        }
    }
}