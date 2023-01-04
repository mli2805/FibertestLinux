using Fibertest.Dto;
using Fibertest.Graph;

namespace Fibertest.DataCenter;

public static class ClientRegistrationExt
{
    public static ClientRegisteredDto FillInSuccessfulResult(this ClientCollection collection, RegisterClientDto dto, User user)
    {
        var result = new ClientRegisteredDto(ReturnCode.ClientRegisteredSuccessfully);

        result.ConnectionId = dto.ConnectionId;

        result.UserId = user.UserId;
        result.Role = user.Role;

        // var zone = collection.WriteModel.Zones.First(z => z.ZoneId == user.ZoneId);
        // result.ZoneId = zone.ZoneId;
        // result.ZoneTitle = zone.Title;
        //
        // result.DatacenterVersion = collection.CurrentDatacenterParameters.DatacenterVersion;
        // result.IsWithoutMapMode = collection.IniFile.Read(IniSection.Server, IniKey.IsWithoutMapMode, false);
        // result.SmtpNotifier = collection.CurrentDatacenterParameters.SmtpNotifier;
        // result.GsmModemComPort = collection.CurrentDatacenterParameters.GsmModemComPort;
        // result.Snmp = collection.CurrentDatacenterParameters.Snmp;

        return result;
    }

}

public static class ClientFinderExt
{
    public static ClientStation? Get(this ClientCollection collection, string connectionId)
    {
        return !collection.Clients.TryGetValue(connectionId, out ClientStation? station) ? null : station;
    }
}