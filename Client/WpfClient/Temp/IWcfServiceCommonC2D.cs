using System.Threading.Tasks;
using Fibertest.Dto;

namespace Fibertest.WpfClient
{
    public interface IWcfServiceCommonC2D
    {
        IWcfServiceCommonC2D SetServerAddresses(DoubleAddress newServerAddress, string username, string clientIp);
        
        Task<BaseRefAssignedDto> ReSendBaseRefAsync(ReSendBaseRefsDto baseRefs);
        // Task<ClientMeasurementStartedDto> StartClientMeasurementAsync(DoClientMeasurementDto dto);
        Task<ClientMeasurementVeexResultDto> GetClientMeasurementAsync(GetClientMeasurementDto dto);
        Task<ClientMeasurementVeexResultDto> GetClientMeasurementSorBytesAsync(GetClientMeasurementDto dto);
        Task<RequestAnswer> PrepareReflectMeasurementAsync(PrepareReflectMeasurementDto dto);
        Task<RequestAnswer> InterruptMeasurementAsync(InterruptMeasurementDto dto);
        Task<string> UpdateMeasurement(string username, UpdateMeasurementDto dto);

      
    }

    public class WcfServiceCommonC2D : IWcfServiceCommonC2D
    {
        public IWcfServiceCommonC2D SetServerAddresses(DoubleAddress newServerAddress, string username, string clientIp)
        {
            throw new System.NotImplementedException();
        }

        public Task<BaseRefAssignedDto> ReSendBaseRefAsync(ReSendBaseRefsDto baseRefs)
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

        public Task<RequestAnswer> InterruptMeasurementAsync(InterruptMeasurementDto dto)
        {
            throw new System.NotImplementedException();
        }
     
        public Task<string> UpdateMeasurement(string username, UpdateMeasurementDto dto)
        {
            throw new System.NotImplementedException();
        }

      
    }
}
