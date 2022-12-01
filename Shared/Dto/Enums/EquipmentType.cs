namespace Fibertest.Dto;

[Serializable]
public enum EquipmentType
{
    Error = -1,


    AdjustmentPoint = 100,

    EmptyNode = 200,

    CableReserve = 300,

    AnyEquipment = 399,
    Other = 400,
    Closure = 402, 
    Cross = 403,
    Well = 405,
    Terminal = 406,

    RtuAndEot = 490,
    Rtu = 500,

    AccidentPlace = 501,
}