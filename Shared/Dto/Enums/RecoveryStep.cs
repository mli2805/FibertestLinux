namespace Fibertest.Dto;

public enum RecoveryStep
{
    Ok              = 0,
    ResetArpAndCharon        = 1,
    RestartService  = 2,
    RebootPc        = 3,
}