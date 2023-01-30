using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.StringResources;
using Fibertest.WpfCommonViews;

namespace Fibertest.WpfClient
{
    public interface IRtuHolder
    {
        Task<bool> SetRtuOccupationState(Guid rtuId, string rtuTitle, RtuOccupation rtuOccupation);
    }

    public class RtuHolder : IRtuHolder
    {
        private readonly IWindowManager _windowManager;
        private readonly IWcfServiceCommonC2D _commonC2DWcfManager;

        public RtuHolder(IWindowManager windowManager, IWcfServiceCommonC2D commonC2DWcfManager)
        {
            _windowManager = windowManager;
            _commonC2DWcfManager = commonC2DWcfManager;
        }

        public async Task<bool> SetRtuOccupationState(Guid rtuId, string rtuTitle, RtuOccupation rtuOccupation)
        {
            var result =
                await _commonC2DWcfManager.SetRtuOccupationState(
                    new OccupyRtuDto(rtuId, new RtuOccupationState(rtuOccupation, null)));
                

            if (result == null) return false;
            if (result.ReturnCode == ReturnCode.RtuIsBusy)
            {
                var mb = new MyMessageBoxViewModel(MessageType.Error, new List<string>()
                {
                    string.Format(Resources.SID_RTU__0__is_busy_, rtuTitle), "", result.RtuOccupationState.GetLocalized(),
                });
                await _windowManager.ShowDialogWithAssignedOwner(mb);
                return false;
            }

            return true;
        } 
    }
}