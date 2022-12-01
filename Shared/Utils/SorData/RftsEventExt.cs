using Fibertest.Dto;
using Optixsoft.SorExaminer.OtdrDataFormat;
using Optixsoft.SorExaminer.OtdrDataFormat.Structures;

namespace Fibertest.Utils
{
    public static class RftsEventExt
    {
        public static OpticalAccidentType GetOpticalTypeOfAccident(this RftsEvent rftsEvent)
        {
            if ((rftsEvent.EventTypes & RftsEventTypes.IsFiberBreak) != 0)
                return OpticalAccidentType.Break;

            if ((rftsEvent.EventTypes & RftsEventTypes.IsNew) != 0)
                return OpticalAccidentType.Loss;

            if ((rftsEvent.ReflectanceThreshold.Type & ShortDeviationTypes.IsExceeded) != 0)
                return OpticalAccidentType.Reflectance;
            if ((rftsEvent.AttenuationThreshold.Type & ShortDeviationTypes.IsExceeded) != 0)
                return OpticalAccidentType.Loss;
            if ((rftsEvent.AttenuationCoefThreshold.Type & ShortDeviationTypes.IsExceeded) != 0)
                return OpticalAccidentType.LossCoeff;
            return OpticalAccidentType.None;
        }

        public static IEnumerable<OpticalAccidentType> GetOpticalTypesOfAccident(this RftsEvent rftsEvent)
        {
            if ((rftsEvent.EventTypes & RftsEventTypes.IsFiberBreak) != 0)
                yield return OpticalAccidentType.Break;
            else
            {
                if ((rftsEvent.ReflectanceThreshold.Type & ShortDeviationTypes.IsExceeded) != 0)
                    yield return OpticalAccidentType.Reflectance;
                if ((rftsEvent.AttenuationThreshold.Type & ShortDeviationTypes.IsExceeded) != 0)
                    yield return OpticalAccidentType.Loss;
                if ((rftsEvent.AttenuationCoefThreshold.Type & ShortDeviationTypes.IsExceeded) != 0)
                    yield return OpticalAccidentType.LossCoeff;
            }
        }
    }
}