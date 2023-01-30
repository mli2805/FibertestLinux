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
}