

// ReSharper disable InconsistentNaming
namespace Fibertest.Dto;

public class VeexMeasOtdrParameters
{
    public string? measurementType;
    public OpticalLineProperties? opticalLineProperties;
    public List<Laser>? lasers;
    public string? distanceRange;
    public string? pulseDuration;
    public string? resolution;
    public bool? fastMeasurement = null;
    public string? averagingTime;
    public bool? highFrequencyResolution = null;
    public List<RequiredConnectionQualitiesItem>? requiredConnectionQualities;
}

public class RequiredConnectionQualitiesItem
{
    public double loss;
    public double reflectance;
}