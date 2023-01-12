using Fibertest.Dto;
using Fibertest.Utils;

namespace Fibertest.Rtu;

public partial class RtuManager
{
    public async Task<OtauAttachedDto> AttachOtau(AttachOtauDto param)
    {
        await Task.Delay(1);
        _logger.TimestampWithoutMessage(Logs.RtuManager);
        OtauAttachedDto result;

        var newCharon = _mainCharon.AttachOtauToPort(param.NetAddress, param.OpticalPort);
        if (newCharon != null)
        {
            _logger.LogInfo(Logs.RtuManager,
                $"Otau {param.NetAddress.ToStringA()} attached to port {param.OpticalPort} and has {newCharon.OwnPortCount} ports");
            result = new OtauAttachedDto(ReturnCode.OtauAttachedSuccessfully)
            {
                OtauId = param.OtauId,
                RtuId = param.RtuId,
                Serial = newCharon.Serial,
                PortCount = newCharon.OwnPortCount,
            };
        }
        else
            result = new OtauAttachedDto(ReturnCode.RtuAttachOtauError)
            {
                ErrorMessage = _mainCharon.LastErrorMessage
            };

        _logger.LogInfo(Logs.RtuManager,
            $"Now RTU has {_mainCharon.OwnPortCount}/{_mainCharon.FullPortCount} ports");
        return result;
    }

    public async Task<OtauDetachedDto> DetachOtau(DetachOtauDto param)
    {
        await Task.Delay(1);
        _logger.TimestampWithoutMessage(Logs.RtuManager);
        OtauDetachedDto result;

        if (_mainCharon.DetachOtauFromPort(param.OpticalPort))
        {
            _logger.LogInfo(Logs.RtuManager,
                $"Otau {param.NetAddress.ToStringA()} detached from port {param.OpticalPort}");
            _logger.LogInfo(Logs.RtuManager,
                $"Now RTU has {_mainCharon.OwnPortCount}/{_mainCharon.FullPortCount} ports");

            result = new OtauDetachedDto(ReturnCode.OtauDetachedSuccessfully);
        }
        else
        {
            result = new OtauDetachedDto(ReturnCode.RtuDetachOtauError)
            {
                ErrorMessage = _mainCharon.LastErrorMessage
            };
        }

        return result;
    }
}