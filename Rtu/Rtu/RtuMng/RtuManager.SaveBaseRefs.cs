using Fibertest.Dto;
using Fibertest.Utils;
using Fibertest.Utils.Setup;

namespace Fibertest.Rtu;

public partial class RtuManager
{
    public async Task<BaseRefAssignedDto> SaveBaseRefs(AssignBaseRefsDto dto)
    {
        await Task.Delay(1);
        try
        {
            var fibertestPath = FileOperations.GetMainFolder();
            var portDataFolder = Path.Combine(fibertestPath, @"PortData");

            if (!Directory.Exists(portDataFolder))
            {
                Directory.CreateDirectory(portDataFolder);
                _logger.LogInfo(Logs.RtuService, $"Created: {portDataFolder}");
            }

            var portFolder = portDataFolder + $"/{dto.OtauPortDto!.Serial}p{dto.OtauPortDto!.OpticalPort:000}";
            if (!Directory.Exists(portFolder))
            {
                Directory.CreateDirectory(portFolder);
                _logger.LogInfo(Logs.RtuService, $"Created: {portFolder}");
            }

            _logger.LogDebug(Logs.RtuService, $"SaveBaseRefs: {dto.BaseRefs.Count} refs");
            foreach (var baseRef in dto.BaseRefs)
                RemoveOldSaveNew(baseRef, portFolder);
            return new BaseRefAssignedDto(ReturnCode.BaseRefAssignedSuccessfully);
        }
        catch (Exception e)
        {
            _logger.LogError(Logs.RtuService, $"SaveBaseRefs: {e.Message}");
            return new BaseRefAssignedDto(ReturnCode.BaseRefAssignmentFailed);
        }
    }

    private void RemoveOldSaveNew(BaseRefDto baseRef, string fullFolderName)
    {
        var fullPath = BuildFileFullPath(baseRef.BaseRefType, fullFolderName);
        if (File.Exists(fullPath))
            File.Delete(fullPath);
        if (baseRef.SorBytes != null)
            File.WriteAllBytes(fullPath, baseRef.SorBytes);
    }

    private string BuildFileFullPath(BaseRefType baseRefType, string fullFolderName)
    {
        var filename = baseRefType.ToBaseFileName();
        var fullPath = Path.Combine(fullFolderName, filename);
        _logger.LogInfo(Logs.RtuService, $"{fullPath}");
        return fullPath;
    }
}