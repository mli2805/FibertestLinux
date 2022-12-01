namespace Fibertest.Dto;

public enum OpticalAccidentType
{
    Break,                   // B,  обрыв
    Loss,                    // L,  превышение порога затухания
    Reflectance,             // R,  превышение порога коэффициента отражения 
    LossCoeff,               // C,  превышение порога коэффициента затухания

    None,
}