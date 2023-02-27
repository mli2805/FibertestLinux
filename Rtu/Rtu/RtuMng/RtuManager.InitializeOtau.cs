using Fibertest.CharonLib;
using Fibertest.Dto;
using Fibertest.Utils;

namespace Fibertest.Rtu;

public partial class RtuManager
{
    private async Task<RtuInitializedDto> InitializeOtau(RtuInitializedDto result)
    {
        await Task.Delay(1);
        var charonIp = _config.Value.General.CharonIp;
        _mainCharon = new Charon(new NetAddress(charonIp, 23), true, _config.Value.Charon, _logger);
        var res = _mainCharon.InitializeOtauRecursively();
        if (res == _mainCharon.NetAddress)
            return new RtuInitializedDto(ReturnCode.OtauInitializationError);

        var previousOwnPortCount = _config.Value.General.PreviousOwnPortCount;
        if (previousOwnPortCount == -1)
            _config.Update(c => c.General.PreviousOwnPortCount = _mainCharon.OwnPortCount);

        if (previousOwnPortCount != _mainCharon.OwnPortCount)
        {
            if (_mainCharon.OwnPortCount != 1) // OTAU is changed - not broken
            {
                _config.Update(c => c.General.PreviousOwnPortCount = _mainCharon.OwnPortCount);
                _mainCharon.IsOk = true;
            }
            else _mainCharon.IsOk = false;
        }

        result.Serial = _mainCharon.Serial;
        result.OwnPortCount = _mainCharon.OwnPortCount;
        result.FullPortCount = _mainCharon.FullPortCount;
        result.Children = _mainCharon.GetChildrenDto();

        result.ReturnCode = ReturnCode.RtuInitializedSuccessfully;

        if (res != null)
        {
            _logger.Info(Logs.RtuManager, $"Child charon {res.ToStringA()} initialization failed.");
            _logger.Info(Logs.RtuManager, "But RTU should work without BOP, so continue...");
        }
            
        _mainCharon.ShowOnDisplayMessageReady();

        return result;
    }

    private async Task<RtuInitializedDto> ReInitializeOtauOnUsersRequest(InitializeRtuDto dto, RtuInitializedDto resultDto)
    {
        _logger.Info(Logs.RtuManager, $"RTU hardware has {_mainCharon.Children.Count} additional OTAU ");
        foreach (var pair in _mainCharon.Children)
            _logger.Info(Logs.RtuManager,
                $"   port {pair.Key}: bop {pair.Value.NetAddress.ToStringA()} {pair.Value.Serial} isOk - {pair.Value.IsOk}");
        _logger.Info(Logs.RtuManager, $"RTU in client has {dto.Children.Count} additional OTAU");
        foreach (var pair in dto.Children)
            _logger.Info(Logs.RtuManager,
                $"   port {pair.Key}: bop {pair.Value.NetAddress.ToStringA()} {pair.Value.Serial} isOk - {pair.Value.IsOk}");

        if (!_mainCharon.IsBopSupported)
        {
            resultDto.ReturnCode = dto.Children.Count > 0 
                ? ReturnCode.RtuDoesNotSupportBop : ReturnCode.Ok;
            return resultDto;
        }

        if (!IsFullMatch(_mainCharon, dto))
        {
            _logger.Info(Logs.RtuManager, "FullMatch - false, need to rewrite ini");
            var expPorts = dto.Children.ToDictionary(pair => pair.Key, pair => pair.Value.NetAddress);
            _mainCharon.RewriteIni(expPorts);
        }

        return await InitializeOtau(resultDto);
    }

    private bool IsFullMatch(Charon mainCharon, InitializeRtuDto dto)
    {
        if (mainCharon.Children.Count != dto.Children.Count)
            return false;

        foreach (var pair in mainCharon.Children)
        {
            if (!dto.Children.ContainsKey(pair.Key))
                return false;
            var otau = dto.Children[pair.Key];
            if (!pair.Value.NetAddress.Equals(otau.NetAddress))
                return false;
        }

        _logger.Info(Logs.RtuManager, "Full match!");
        return true;
    }
}