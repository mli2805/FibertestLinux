namespace Fibertest.Dto;

public enum SorType
{
    Base,
    Analysis,
    Meas,
    Raw,   // before any processing
    Error, // error while measurement
    Previous,
}