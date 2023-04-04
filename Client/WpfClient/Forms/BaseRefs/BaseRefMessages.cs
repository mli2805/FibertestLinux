using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.StringResources;
using Fibertest.WpfCommonViews;

namespace Fibertest.WpfClient
{
    public class BaseRefMessages
    {
        private readonly IWindowManager _windowManager;

        public BaseRefMessages(IWindowManager windowManager)
        {
            _windowManager = windowManager;
        }

        public void Display(BaseRefAssignedDto dto, Trace trace)
        {
            switch (dto.ReturnCode)
            {
                case ReturnCode.D2RGrpcOperationError:
                    DisplayD2RError(dto);
                    break;
                case ReturnCode.BaseRefAssignmentParamNotAcceptable: DisplayParamIsNotAcceptable(dto); break;
                case ReturnCode.BaseRefAssignmentNoThresholds: DisplayThereIsNoThresholds(dto); break;
                case ReturnCode.BaseRefAssignmentEdgeLandmarksWrong: DisplayEdgeLandmarksAreWrong(dto); break;
                case ReturnCode.BaseRefAssignmentLandmarkCountWrong: DisplayLandmarkCountDoesnotMatch(dto, trace); break;
                case ReturnCode.BaseRefAssignmentFailed:
                default:
                    DisplayCommonError(dto); break;
            }
        }

        private async void DisplayCommonError(BaseRefAssignedDto dto)
        {
            var baseRefHeader = dto.BaseRefType.GetLocalizedFemaleString() + Resources.SID__base_;
            var vm = new MyMessageBoxViewModel(MessageType.Error, new List<string>() 
                { baseRefHeader, "", "", dto.ErrorMessage! });
            await _windowManager.ShowDialogWithAssignedOwner(vm);
        }
        private async void DisplayD2RError(BaseRefAssignedDto dto)
        {
            var vm = new MyMessageBoxViewModel(MessageType.Error, new List<string>() { dto.ReturnCode.GetLocalizedString() });
            await _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        private async void DisplayParamIsNotAcceptable(BaseRefAssignedDto dto)
        {
            var baseRefHeader = dto.BaseRefType.GetLocalizedFemaleString() + Resources.SID__base_;
            var vm = new MyMessageBoxViewModel(MessageType.Error, new List<string>()
            {
                baseRefHeader,
                "",
                "",
                Resources.SID_Measurement_parameters_are_not_compatible_with_this_RTU,
                "",
                string.Format(Resources.SID_Invalid_parameter___Wave_length__0_, dto.WaveLength),
            }, 5);
            await _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        private async void DisplayThereIsNoThresholds(BaseRefAssignedDto dto)
        {
            var baseRefHeader = dto.BaseRefType.GetLocalizedFemaleString() + Resources.SID__base_;
            var message = Resources.SID_There_are_no_thresholds_for_comparison;
            var vm = new MyMessageBoxViewModel(MessageType.Error, new List<string>() { baseRefHeader, "", message }, 2);
            await _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        private async void DisplayEdgeLandmarksAreWrong(BaseRefAssignedDto dto)
        {
            var baseRefHeader = dto.BaseRefType.GetLocalizedFemaleString() + Resources.SID__base_;
            // var message = Resources.SID_First_and_last_landmarks_should_be_associated_with_key_events_;
            var message = Resources.SID_Events_corresponding_to_the_beginning_and_end_of_the_trace_must_be_associated_with_landmarks;
            var vm = new MyMessageBoxViewModel(MessageType.Error, new List<string>() { baseRefHeader, "", message }, 2);
            await _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        private async void DisplayLandmarkCountDoesnotMatch(BaseRefAssignedDto dto, Trace trace)
        {
            var messageStrings = new List<string>() {
                string.Format(Resources.SID__0__base_is_not_compatible_with_trace, dto.BaseRefType.GetLocalizedFemaleString()),
                trace.Title, "", ""
            };

            messageStrings
                .Add(string.Format(Resources.SID_Landmarks_count_in_reflectogram_is__0_, dto.Landmarks) + Environment.NewLine +
                     string.Format(Resources.SID_Trace_s_equipment_count__excluding_Cable_reserve__is__0_, dto.Equipments) + Environment.NewLine +
                     Environment.NewLine +
                     string.Format(Resources.SID_Trace_s_node_count_is__0_, dto.Nodes));

            var vm = new MyMessageBoxViewModel(MessageType.Error, messageStrings, 4);
            await _windowManager.ShowDialogWithAssignedOwner(vm);
        }

        public async Task<bool> IsLengthDifferenceAcceptable(string gpsDistance, string opticalLength)
        {
            var line = string.Format(Resources.SID_Trace_length_on_map_is__0__km, gpsDistance) +
                          Environment.NewLine + string.Format(Resources.SID_Optical_length_is__0__km, opticalLength);

            var message = new List<MyMessageBoxLineModel>();
            message.Add(new MyMessageBoxLineModel() { Line = BaseRefType.Precise.GetLocalizedFemaleString() + Resources.SID__base_ });
            message.Add(new MyMessageBoxLineModel() { Line = "" });
            message.Add(new MyMessageBoxLineModel() { Line = "" });
            message.Add(new MyMessageBoxLineModel() { Line = line, FontWeight = FontWeights.Bold });
            message.Add(new MyMessageBoxLineModel() { Line = "" });
            message.Add(new MyMessageBoxLineModel() { Line = "" });
            message.Add(new MyMessageBoxLineModel() { Line = Resources.SID_Assign_base_reflectogram_ });

            var vmc = new MyMessageBoxViewModel(MessageType.Confirmation, message);
            await _windowManager.ShowDialogWithAssignedOwner(vmc);
            return vmc.IsAnswerPositive;
        }
    }
}