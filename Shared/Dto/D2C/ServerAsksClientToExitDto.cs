namespace Fibertest.Dto
{
    
    public class ServerAsksClientToExitDto
    {
        
        public bool ToAll;
        
        public string? ConnectionId;
        
        public UnRegisterReason Reason;

        // if user pushed out by new session
        
        public string? NewAddress;
        
        public bool IsNewUserWeb;
    }

    public enum UnRegisterReason
    {
        UserRegistersAnotherSession,
        DbOptimizationStarted,
        DbOptimizationFinished,
    }
}