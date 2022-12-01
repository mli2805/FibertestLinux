namespace Fibertest.Dto;

public class TraceLandmarksDto
{
    public TraceHeaderDto Header = new TraceHeaderDto();
    public List<LandmarkDto>? Landmarks;
}