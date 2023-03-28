using System.Windows.Media;
using Fibertest.Dto;
using Fibertest.Graph;

namespace Fibertest.WpfClient
{
    public class TraceMeasurementModel
    {
        public Measurement Measurement { get; set; }

        public bool IsOpticalEvent => Measurement.EventStatus != EventStatus.JustMeasurementNotAnEvent;
        public string BaseRefTypeString => Measurement.BaseRefType.GetLocalizedString();
        public Brush TraceStateBrush =>
            Measurement.TraceState == FiberState.Ok
                ? Brushes.White
                : Measurement.BaseRefType == BaseRefType.Fast
                    ? Brushes.Yellow
                    : Measurement.TraceState.GetBrush(isForeground: false);
        public string TraceStateOnScreen => Measurement.BaseRefType == BaseRefType.Fast && Measurement.TraceState != FiberState.Ok
            ? FiberState.Suspicion.ToLocalizedString()
            : Measurement.TraceState.ToLocalizedString();

        public TraceMeasurementModel(Measurement measurement)
        {
            Measurement = measurement;
        }
    }
}
