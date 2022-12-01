namespace Fibertest.Dto;

public static class VeexPortExt
{
    private static bool IsEqual(this VeexOtauPort otauPort, VeexOtauPort? anotherOtauPort)
    {
        if (anotherOtauPort == null) return false;
        return otauPort.otauId == anotherOtauPort.otauId && otauPort.portIndex == anotherOtauPort.portIndex;
    }

    public static bool IsEqual(this List<VeexOtauPort> otauPorts, List<VeexOtauPort>? anotherList)
    {
        if (anotherList == null) return false;
        if (otauPorts.Count != anotherList.Count) return false;
        return otauPorts.All(otauPort => anotherList.Any(o => IsEqual(o, otauPort)));
    }

    public static string PortName(this List<VeexOtauPort>? otauPorts)
    {
        if (otauPorts == null || otauPorts.Count == 0) return "no port";
        if (otauPorts.Count == 1)
            return (otauPorts[0].portIndex + 1).ToString();
        return $"{otauPorts[0].portIndex + 1}-{otauPorts[1].portIndex + 1}";
    }

    private static VeexOtauPort Map(OtauPortDto otauPortDto)
    {
        return new VeexOtauPort()
        {
            otauId = otauPortDto.IsPortOnMainCharon ? otauPortDto.OtauId : "S2_" + otauPortDto.OtauId,
            portIndex = otauPortDto.OpticalPort - 1,
        };
    }

    public static List<VeexOtauPort> Create(OtauPortDto otauWithLine, OtauPortDto mainOtau)
    {
        var otauPorts = new List<VeexOtauPort>();
        if (!otauWithLine.IsPortOnMainCharon)
        {
            otauPorts.Add(Map(mainOtau));
        }
        otauPorts.Add(Map(otauWithLine));
        return otauPorts;
    }

    public static List<VeexOtauPort> Create(PortWithTraceDto dto, string mainOtauId)
    {
        var otauPorts = new List<VeexOtauPort>();
        if (dto.OtauPort == null) return otauPorts;
        if (!dto.OtauPort.IsPortOnMainCharon)
        {
            otauPorts.Add(new VeexOtauPort()
            {
                otauId = mainOtauId,
                portIndex = dto.OtauPort.MainCharonPort - 1,
            });
        }
        otauPorts.Add(Map(dto.OtauPort));
        return otauPorts;
    }
}