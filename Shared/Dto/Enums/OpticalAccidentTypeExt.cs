namespace Fibertest.Dto;

public static class OpticalAccidentTypeExt
{
    public static string ToLetter(this OpticalAccidentType type)
    {
        switch (type)
        {
            case OpticalAccidentType.Break: return @"B";
            case OpticalAccidentType.Reflectance: return @"R";
            case OpticalAccidentType.Loss: return @"L";
            case OpticalAccidentType.LossCoeff: return @"C";
            default: return @"N";
        }
    }
}