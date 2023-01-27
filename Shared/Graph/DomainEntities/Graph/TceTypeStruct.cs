using Fibertest.Dto;

namespace Fibertest.Graph;

public struct TceTypeStruct
{
    public int Id;
    public bool IsVisible; // show only models user has
    public string? Model { get; set; }
    public TceMaker Maker;
    public string? SoftwareVersion { get; set; }
    public string? Code; // for pretty parser switch
    public int SlotCount => SlotPositions.Length;
    public int[] SlotPositions;
    public int GponInterfaceNumerationFrom;
    public string? Comment;

    public string TypeTitle => $@"{Maker} {Model} {SoftwareVersion}";
}