using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.StringResources;
using Fibertest.WpfCommonViews;
using GrpsClientLib;

namespace Fibertest.WpfClient
{
    public interface IRtuHolder
    {
        Task<bool> SetRtuOccupationState(Guid rtuId, string rtuTitle, RtuOccupation rtuOccupation);
    }

    public class RtuHolder : IRtuHolder
    {
        private readonly IWindowManager _windowManager;
        private readonly GrpcC2DService _grpcC2DService;

        public RtuHolder(IWindowManager windowManager, GrpcC2DService grpcC2DService)
        {
            _windowManager = windowManager;
            _grpcC2DService = grpcC2DService;
        }

        public async Task<bool> SetRtuOccupationState(Guid rtuId, string rtuTitle, RtuOccupation rtuOccupation)
        {
            var result =
                await _grpcC2DService.SendAnyC2DRequest<OccupyRtuDto, RequestAnswer>(
                    new OccupyRtuDto(rtuId, new RtuOccupationState(rtuOccupation, null)));
                
            if (result.ReturnCode == ReturnCode.RtuIsBusy)
            {
                var mb = new MyMessageBoxViewModel(MessageType.Error, new List<string>()
                {
                    string.Format(Resources.SID_RTU__0__is_busy_, rtuTitle), "", result.RtuOccupationState!.GetLocalized(),
                });
                await _windowManager.ShowDialogWithAssignedOwner(mb);
                return false;
            }

            return true;
        } 
    }
}