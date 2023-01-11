using System.Reflection;
using Fibertest.Dto;
using Fibertest.Utils;

namespace Fibertest.Rtu;

public partial class RtuManager
{
    public async Task<BaseRefAssignedDto> SaveBaseRefs(AssignBaseRefsDto dto)
    {
        await Task.Delay(1);
        var fullFolderName = GetAbsolutePortFolder(dto.OtauPortDto!);

        foreach (var baseRef in dto.BaseRefs)
            RemoveOldSaveNew(baseRef, fullFolderName);
        return new BaseRefAssignedDto(ReturnCode.BaseRefAssignedSuccessfully);
    }

    private void RemoveOldSaveNew(BaseRefDto baseRef, string fullFolderName)
    {
        var fullPath = BuildFullPath(baseRef.BaseRefType, fullFolderName);
        if (File.Exists(fullPath))
            File.Delete(fullPath);
        if (baseRef.SorBytes != null)
            File.WriteAllBytes(fullPath, baseRef.SorBytes);
    }

    private string BuildFullPath(BaseRefType baseRefType, string fullFolderName)
    {
        var filename = baseRefType.ToBaseFileName();
        var fullPath = Path.Combine(fullFolderName, filename);
        _logger.LLog(Logs.RtuService.ToInt(), $"{fullPath}");
        return fullPath;
    }

    private string GetAbsolutePortFolder(OtauPortDto otauPortDto)
    {
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var basePath = Path.GetDirectoryName(assemblyLocation) ?? "";

        var portFolder = $@"PortData/{otauPortDto.Serial}p{otauPortDto.OpticalPort:000}/";
        return Path.Combine(basePath, $@"../{portFolder}");
    }
}