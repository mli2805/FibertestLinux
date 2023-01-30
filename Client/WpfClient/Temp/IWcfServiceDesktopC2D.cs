using System.Collections.Generic;
using System.Threading.Tasks;
using Fibertest.Dto;

namespace Fibertest.WpfClient
{
    public interface IWcfServiceDesktopC2D
    {
        IWcfServiceDesktopC2D SetServerAddresses(DoubleAddress newServerAddress, string username, string clientIp);
        Task<bool> SendHeartbeat(HeartbeatDto dto);
        Task<bool> CheckServerConnection(CheckServerConnectionDto dto);
        Task<DiskSpaceDto> GetDiskSpaceGb();
        Task<int> ExportEvents();

        #region Event sourcing
        Task<int> SendCommands(List<string> jsons, string username, string clientIp); // especially for Migrator.exe
        Task<int> SendCommandsAsObjs(List<object> cmds);
        Task<string> SendCommandAsObj(object cmd);
        Task<string> SendCommand(string json, string username, string clientIp);
        Task<string[]> GetEvents(GetEventsDto dto);
        Task<SerializedModelDto> GetModelDownloadParams(GetSnapshotDto dto);
        Task<byte[]> GetModelPortion(int portionOrdinal);
        #endregion
        
        Task<bool> RemoveUnused();

        #region Settings
        Task<bool> SaveSmtpSettings(SmtpConfig dto);
        Task<bool> SaveAndTestSnmpSettings(SnmpConfig dto);
        Task<bool> SaveGisMode(bool isMapVisible);
        Task<bool> SaveGsmComPort(string comPort);
        Task<bool> SendTest(string to, NotificationType notificationType);
        #endregion
    }

    public class WcfServiceDesktopC2D : IWcfServiceDesktopC2D
    {
        public IWcfServiceDesktopC2D SetServerAddresses(DoubleAddress newServerAddress, string username, string clientIp)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> SendHeartbeat(HeartbeatDto dto)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> CheckServerConnection(CheckServerConnectionDto dto)
        {
            throw new System.NotImplementedException();
        }

        public Task<DiskSpaceDto> GetDiskSpaceGb()
        {
            throw new System.NotImplementedException();
        }

        public Task<int> ExportEvents()
        {
            throw new System.NotImplementedException();
        }

        public Task<int> SendCommands(List<string> jsons, string username, string clientIp)
        {
            throw new System.NotImplementedException();
        }

        public Task<int> SendCommandsAsObjs(List<object> cmds)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> SendCommandAsObj(object cmd)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> SendCommand(string json, string username, string clientIp)
        {
            throw new System.NotImplementedException();
        }

        public Task<string[]> GetEvents(GetEventsDto dto)
        {
            throw new System.NotImplementedException();
        }

        public Task<SerializedModelDto> GetModelDownloadParams(GetSnapshotDto dto)
        {
            throw new System.NotImplementedException();
        }

        public Task<byte[]> GetModelPortion(int portionOrdinal)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> RemoveUnused()
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> SaveSmtpSettings(SmtpConfig dto)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> SaveAndTestSnmpSettings(SnmpConfig dto)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> SaveGisMode(bool isMapVisible)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> SaveGsmComPort(string comPort)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> SendTest(string to, NotificationType notificationType)
        {
            throw new System.NotImplementedException();
        }
    }
}