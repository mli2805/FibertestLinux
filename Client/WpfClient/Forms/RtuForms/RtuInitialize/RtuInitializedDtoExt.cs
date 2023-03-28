using System;
using System.Collections.Generic;
using System.Linq;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.StringResources;
using Fibertest.WpfCommonViews;

namespace Fibertest.WpfClient
{
    public static class RtuInitializedDtoExt
    {
        public static MyMessageBoxViewModel CreateMessageBox(this RtuInitializedDto dto, string rtuTitle)
        {
            switch (dto.ReturnCode)
            {
                case ReturnCode.Ok:
                case ReturnCode.RtuInitializedSuccessfully:
                    var msg = dto.Children.Any(c => !c.Value.IsOk)
                        ? Resources.SID_RTU_initialized2
                        : Resources.SID_RTU_initialized_successfully_;
                    return new MyMessageBoxViewModel(MessageType.Information, msg);
                case ReturnCode.RtuDoesNotSupportBop:
                case ReturnCode.RtuTooBigPortNumber:
                    var strs = new List<string>()
                    {
                        dto.ReturnCode.GetLocalizedString(), "", Resources.SID_Detach_BOP_manually, Resources.SID_and_start_initialization_again_
                    };
                    return new MyMessageBoxViewModel(MessageType.Error, strs);
                case ReturnCode.RtuIsBusy:
                case ReturnCode.RtuInitializationInProgress:
                case ReturnCode.RtuAutoBaseMeasurementInProgress:
                    return new MyMessageBoxViewModel(
                    MessageType.Error, new List<string>()
                        {
                            string.Format(Resources.SID_RTU__0__is_busy_, rtuTitle), "", dto.RtuOccupationState!.GetLocalized(),
                        }, 1);
                case ReturnCode.RtuInitializationError:
                case ReturnCode.OtauInitializationError:
                case ReturnCode.OtdrInitializationFailed:
                default:
                    return new MyMessageBoxViewModel
                        (MessageType.Error, dto.ReturnCode.GetLocalizedWithOsInfo(dto.ErrorMessage).Split('\n'), 0);
            }
        }

        public static string CreateLogMessage(this RtuInitializedDto dto)
        {
            var rtuName = $@"RTU {dto.RtuAddresses.Main.Ip4Address}";
            var message = dto.IsInitialized
                ? $@"{rtuName} initialized successfully."
                : $@"{rtuName} initialization failed. " + Environment.NewLine + dto.ReturnCode.GetLocalizedString();
            if (!string.IsNullOrEmpty(dto.ErrorMessage))
                message += Environment.NewLine + dto.ErrorMessage;
            return message;
        }
    }
}
