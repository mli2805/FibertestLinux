namespace Fibertest.Dto
{
    
    public class DbOptimizationProgressDto
    {
        
        public string? Username;

        
        public DbOptimizationStage Stage;

        
        public int MeasurementsChosenForDeletion;

        
        public double TableOptimizationPercent;

        
        public double EventsReplayed;

        
        public double OldSizeGb;
        
        public double NewSizeGb;
    }
}