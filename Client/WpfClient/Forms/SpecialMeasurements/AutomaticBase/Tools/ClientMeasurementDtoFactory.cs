using System.Collections.Generic;
using System.Linq;
using Fibertest.Dto;
using Fibertest.Graph;

namespace Fibertest.WpfClient
{
    public static class ClientMeasurementDtoFactory
    {
        public static DoClientMeasurementDto CreateDoClientMeasurementDto
            (this Leaf parent, int portNumber, bool keepOtdrConnection, Model readModel, CurrentUser currentUser)
        {
            var rtuId = (parent is RtuLeaf leaf ? leaf : (RtuLeaf)parent.Parent).Id;
            var rtu = readModel.Rtus.First(r => r.Id == rtuId);

            var listOfOtauPortDto = parent.PreparePairOfOtauPortDto(portNumber, readModel);

            return new DoClientMeasurementDto(rtu.Id, rtu.RtuMaker)
            {
                OtauPortDto = listOfOtauPortDto,
                KeepOtdrConnection = keepOtdrConnection,
            };
        }

        public static DoClientMeasurementDto SetParams(this DoClientMeasurementDto dto, bool isForAutoBase, bool isInsertIitEvents,
            bool isAutoLmax, List<MeasParamByPosition>? iitMeasParams, VeexMeasOtdrParameters veexMeasParams)
        {
            // IIT
            dto.SelectedMeasParams = iitMeasParams;

            // Veex && IIT
            dto.VeexMeasOtdrParameters = veexMeasParams;

            // Veex
            dto.AnalysisParameters = new AnalysisParameters()
            {
                lasersParameters = new List<LasersParameter>()
                {
                    new LasersParameter()
                        { eventLossThreshold = 0.2, eventReflectanceThreshold = -40, endOfFiberThreshold = 6 }
                }
            };

            // Veex
            if (isForAutoBase)
            {
                dto.AnalysisParameters.findOnlyFirstAndLastEvents = true;
                dto.AnalysisParameters.setUpIitEvents = isInsertIitEvents;
            }

            // IIT
            dto.IsForAutoBase = isForAutoBase;
            dto.IsInsertNewEvents = isInsertIitEvents;
            dto.IsAutoLmax = isAutoLmax;
            return dto;
        }

        public static VeexMeasOtdrParameters GetVeexMeasOtdrParametersBase(this OtdrParametersTemplateModel model, bool isGetLineParametersRequest)
        {
            var dto = new VeexMeasOtdrParameters()
            {
                measurementType = isGetLineParametersRequest ? @"auto_skip_measurement" : @"manual",
                // fastMeasurement = false,
               
                lasers = new List<Laser>() { new Laser() { laserUnit = model.SelectedUnit } },
                opticalLineProperties = new OpticalLineProperties()
                {
                    kind = @"point_to_point",
                    lasersProperties = new List<LasersProperty>()
                    {
                        new LasersProperty()
                        {
                            laserUnit = model.SelectedUnit,
                            backscatterCoefficient = (int)model.BackScatteredCoefficient,
                            refractiveIndex = model.RefractiveIndex,
                        }
                    }
                },
            };
            // for GetLineParametersRequest it should remains NULL
            if (!isGetLineParametersRequest)
                dto.highFrequencyResolution = false;
            return dto;
        }

        public static VeexMeasOtdrParameters? FillInWithTemplate
            (this VeexMeasOtdrParameters dto, ConnectionQuality connectionQuality, string omid)
        {
            var parameters = AutoBaseParams.GetPredefinedParamsForLmax(connectionQuality.lmaxKm, omid);
            if (parameters == null) return null;

            dto.distanceRange = parameters.distanceRange;
            dto.resolution = parameters.resolution;
            dto.pulseDuration = parameters.pulseDuration;
            dto.averagingTime = parameters.averagingTime;

            return dto;
        }
    }
}