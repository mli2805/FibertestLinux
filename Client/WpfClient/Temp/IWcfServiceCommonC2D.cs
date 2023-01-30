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

    public class WcfServiceCommonC2D : IWcfServiceCommonC2D
    {
        public IWcfServiceCommonC2D SetServerAddresses(DoubleAddress newServerAddress, string username, string clientIp)
        {
            throw new System.NotImplementedException();
        }

        public Task<ClientRegisteredDto> RegisterClientAsync(RegisterClientDto dto)
        {
            throw new System.NotImplementedException();
        }

        public Task<RequestAnswer> SetRtuOccupationState(OccupyRtuDto dto)
        {
            throw new System.NotImplementedException();
        }

        public Task<RequestAnswer> RegisterHeartbeat(string connectionId)
        {
            throw new System.NotImplementedException();
        }

        public Task<int> UnregisterClientAsync(UnRegisterClientDto dto)
        {
            throw new System.NotImplementedException();
        }

        public Task<RequestAnswer> DetachTraceAsync(DetachTraceDto dto)
        {
            throw new System.NotImplementedException();
        }

        public Task<RtuConnectionCheckedDto> CheckRtuConnectionAsync(CheckRtuConnectionDto rtuAddress)
        {
            throw new System.NotImplementedException();
        }

        public Task<RtuInitializedDto> InitializeRtuAsync(InitializeRtuDto dto)
        {
            throw new System.NotImplementedException();
        }

        public Task<OtauAttachedDto> AttachOtauAsync(AttachOtauDto dto)
        {
            throw new System.NotImplementedException();
        }

        public Task<OtauDetachedDto> DetachOtauAsync(DetachOtauDto dto)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> StopMonitoringAsync(StopMonitoringDto dto)
        {
            throw new System.NotImplementedException();
        }

        public Task<RequestAnswer> ApplyMonitoringSettingsAsync(ApplyMonitoringSettingsDto settings)
        {
            throw new System.NotImplementedException();
        }

        public Task<BaseRefAssignedDto> AssignBaseRefAsync(AssignBaseRefsDto baseRefs)
        {
            throw new System.NotImplementedException();
        }

        public Task<RequestAnswer> AttachTraceAndSendBaseRefs(AttachTraceDto cmd)
        {
            throw new System.NotImplementedException();
        }

        public Task<BaseRefAssignedDto> AssignBaseRefAsyncFromMigrator(AssignBaseRefsDto baseRefs)
        {
            throw new System.NotImplementedException();
        }

        public Task<BaseRefAssignedDto> ReSendBaseRefAsync(ReSendBaseRefsDto baseRefs)
        {
            throw new System.NotImplementedException();
        }

        public Task<ClientMeasurementStartedDto> StartClientMeasurementAsync(DoClientMeasurementDto dto)
        {
            throw new System.NotImplementedException();
        }

        public Task<ClientMeasurementVeexResultDto> GetClientMeasurementAsync(GetClientMeasurementDto dto)
        {
            throw new System.NotImplementedException();
        }

        public Task<ClientMeasurementVeexResultDto> GetClientMeasurementSorBytesAsync(GetClientMeasurementDto dto)
        {
            throw new System.NotImplementedException();
        }

        public Task<RequestAnswer> PrepareReflectMeasurementAsync(PrepareReflectMeasurementDto dto)
        {
            throw new System.NotImplementedException();
        }

        public Task<RequestAnswer> DoOutOfTurnPreciseMeasurementAsync(DoOutOfTurnPreciseMeasurementDto dto)
        {
            throw new System.NotImplementedException();
        }

        public Task<RequestAnswer> InterruptMeasurementAsync(InterruptMeasurementDto dto)
        {
            throw new System.NotImplementedException();
        }

        public Task<RequestAnswer> FreeOtdrAsync(FreeOtdrDto dto)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> UpdateMeasurement(string username, UpdateMeasurementDto dto)
        {
            throw new System.NotImplementedException();
        }

        public Task<byte[]> GetSorBytes(int sorFileId)
        {
            throw new System.NotImplementedException();
        }

        public Task<RftsEventsDto> GetRftsEvents(int sorFileId)
        {
            throw new System.NotImplementedException();
        }
    }
}
