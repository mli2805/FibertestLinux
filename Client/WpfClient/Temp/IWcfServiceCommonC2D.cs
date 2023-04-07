using System;
using System.Threading.Tasks;
using Fibertest.Dto;

namespace Fibertest.WpfClient
{
    public interface IWcfServiceCommonC2D
    {
        IWcfServiceCommonC2D SetServerAddresses(DoubleAddress newServerAddress, string username, string clientIp);

        #region VEEX
        Task<ClientMeasurementVeexResultDto> GetClientMeasurementAsync(GetClientMeasurementDto dto);
        Task<ClientMeasurementVeexResultDto> GetClientMeasurementSorBytesAsync(GetClientMeasurementDto dto);
        Task<RequestAnswer> PrepareReflectMeasurementAsync(PrepareReflectMeasurementDto dto);
        #endregion
        //WEB
        Task<string> UpdateMeasurement(string username, UpdateMeasurementDto dto);
    }

    public class WcfServiceCommonC2D : IWcfServiceCommonC2D
    {
        public IWcfServiceCommonC2D SetServerAddresses(DoubleAddress newServerAddress, string username, string clientIp)
        {
            throw new NotImplementedException();
        }

        #region VEEX
        public Task<ClientMeasurementVeexResultDto> GetClientMeasurementAsync(GetClientMeasurementDto dto)
        {
            throw new NotImplementedException();
        }
        public Task<ClientMeasurementVeexResultDto> GetClientMeasurementSorBytesAsync(GetClientMeasurementDto dto)
        {
            throw new NotImplementedException();
        }
        public Task<RequestAnswer> PrepareReflectMeasurementAsync(PrepareReflectMeasurementDto dto)
        {
            throw new NotImplementedException();
        }
        #endregion
        
        //WEB
        public Task<string> UpdateMeasurement(string username, UpdateMeasurementDto dto)
        {
            throw new NotImplementedException();
        }
    }
}
