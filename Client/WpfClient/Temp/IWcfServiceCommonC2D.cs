using System.Threading.Tasks;
using Fibertest.Dto;

namespace Fibertest.WpfClient
{
    public interface IWcfServiceCommonC2D
    {
        IWcfServiceCommonC2D SetServerAddresses(DoubleAddress newServerAddress, string username, string clientIp);

        // C2D
        
        Task<ClientRegisteredDto> RegisterClientAsync(RegisterClientDto dto);

        
        Task<RequestAnswer> SetRtuOccupationState(OccupyRtuDto dto);

        
        Task<RequestAnswer> RegisterHeartbeat(string connectionId);

        
        Task<int> UnregisterClientAsync(UnRegisterClientDto dto);

        
        Task<RequestAnswer> DetachTraceAsync(DetachTraceDto dto);


        // C2D2R
        
        Task<RtuConnectionCheckedDto> CheckRtuConnectionAsync(CheckRtuConnectionDto rtuAddress);

        
        Task<RtuInitializedDto> InitializeRtuAsync(InitializeRtuDto dto);

        
        Task<OtauAttachedDto> AttachOtauAsync(AttachOtauDto dto);

        
        Task<OtauDetachedDto> DetachOtauAsync(DetachOtauDto dto);

        
        Task<bool> StopMonitoringAsync(StopMonitoringDto dto);

        
        Task<RequestAnswer> ApplyMonitoringSettingsAsync(ApplyMonitoringSettingsDto settings);

        
        Task<BaseRefAssignedDto> AssignBaseRefAsync(AssignBaseRefsDto baseRefs);

        
        Task<RequestAnswer> AttachTraceAndSendBaseRefs(AttachTraceDto cmd);

        
        Task<BaseRefAssignedDto> AssignBaseRefAsyncFromMigrator(AssignBaseRefsDto baseRefs);

        
        Task<BaseRefAssignedDto> ReSendBaseRefAsync(ReSendBaseRefsDto baseRefs);

        
        Task<ClientMeasurementStartedDto> StartClientMeasurementAsync(DoClientMeasurementDto dto);

        
        Task<ClientMeasurementVeexResultDto> GetClientMeasurementAsync(GetClientMeasurementDto dto);

        
        Task<ClientMeasurementVeexResultDto> GetClientMeasurementSorBytesAsync(GetClientMeasurementDto dto);

        
        Task<RequestAnswer> PrepareReflectMeasurementAsync(PrepareReflectMeasurementDto dto);

        
        Task<RequestAnswer> DoOutOfTurnPreciseMeasurementAsync(DoOutOfTurnPreciseMeasurementDto dto);

        
        Task<RequestAnswer> InterruptMeasurementAsync(InterruptMeasurementDto dto);

        
        Task<RequestAnswer> FreeOtdrAsync(FreeOtdrDto dto);

        
        Task<string> UpdateMeasurement(string username, UpdateMeasurementDto dto);

        // C2Database
        
        Task<byte[]> GetSorBytes(int sorFileId);

        
        Task<RftsEventsDto> GetRftsEvents(int sorFileId);

    }
}
