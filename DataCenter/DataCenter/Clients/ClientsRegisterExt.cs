using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.Utils;

namespace Fibertest.DataCenter
{
    public static class ClientsRegisterExt
    {
        // R1
        public static ClientRegisteredDto? CheckLicense(this ClientCollection collection, RegisterClientDto dto)
        {
            if (collection.WriteModel.Licenses.Count == 0)
                return new ClientRegisteredDto() { ReturnCode = ReturnCode.NoLicenseHasBeenAppliedYet };

            if (dto.IsUnderSuperClient)
            {
                var licenseCount = collection.WriteModel.GetSuperClientStationLicenseCount();
                if (licenseCount == 0)
                    return new ClientRegisteredDto() { ReturnCode = ReturnCode.SuperClientsCountExceeded };
                if (collection.Clients.Count(c => c.Value.UserRole == Role.SuperClient) >= licenseCount
                    && collection.Clients.All(s => s.Value.ClientIp != dto.ClientIp))
                    return new ClientRegisteredDto() { ReturnCode = ReturnCode.SuperClientsCountExceeded };
            }
            else if (dto.IsWebClient)
            {
                var licenseCount = collection.WriteModel.GetWebClientLicenseCount();
                if (licenseCount == 0)
                    return new ClientRegisteredDto() { ReturnCode = ReturnCode.WebClientsCountExceeded };
                if (collection.Clients.Count(c => c.Value.IsWebClient) >= licenseCount
                    && collection.Clients.All(s => s.Value.ClientIp != dto.ClientIp))
                    return new ClientRegisteredDto() { ReturnCode = ReturnCode.WebClientsCountExceeded };
            }
            else
            {
                var licenseCount = collection.WriteModel.GetClientStationLicenseCount();
                if (licenseCount == 0)
                    return new ClientRegisteredDto() { ReturnCode = ReturnCode.ClientsCountExceeded };
                if (collection.Clients.Count(c => c.Value.IsDesktopClient) >= licenseCount
                    && collection.Clients.All(s => s.Value.ClientIp != dto.ClientIp))
                    return new ClientRegisteredDto() { ReturnCode = ReturnCode.ClientsCountExceeded };
            }
            return null;
        }

        // R2 Check user and password

        // R3
        public static ClientRegisteredDto? CheckRights(this User user, RegisterClientDto dto)
        {
            if (dto.IsUnderSuperClient)
            {
                if (!user.Role.IsSuperClientPermitted())
                    return new ClientRegisteredDto() { ReturnCode = ReturnCode.UserHasNoRightsToStartSuperClient };
            }
            else if (dto.IsWebClient)
            {
                if (!user.Role.IsWebPermitted())
                    return new ClientRegisteredDto() { ReturnCode = ReturnCode.UserHasNoRightsToStartWebClient };
            }
            else
            {
                if (!user.Role.IsDesktopPermitted())
                    return new ClientRegisteredDto() { ReturnCode = ReturnCode.UserHasNoRightsToStartClient };
            }
            return null;
        }


        // R4
        public static async Task<ClientRegisteredDto?> CheckTheSameUser(
            this ClientCollection collection, RegisterClientDto dto, User user)
        {
            var stationWithTheSameUser = 
                collection.Clients.Values.FirstOrDefault(s => s.UserId == user.UserId);
            if (stationWithTheSameUser != null)
            {
                // both clients are desktop
                if (!dto.IsWebClient && !stationWithTheSameUser.IsWebClient)
                {
                    collection.Logger.LogInfo(Logs.DataCenter, $"The same user {dto.UserName} registered from device {stationWithTheSameUser.ClientIp}");
                    return new ClientRegisteredDto() { ReturnCode = ReturnCode.ThisUserRegisteredFromAnotherDevice };
                }
                else
                // different types of clients or both clients are web
                {
                    collection.Logger.LogInfo(Logs.DataCenter, 
                        $"The same client {stationWithTheSameUser.UserName}/{stationWithTheSameUser.ClientIp} with connectionId {stationWithTheSameUser.ConnectionId} removed.");
                    //TODO: notify old station
                    await Task.Delay(0);
                    // var serverAsksClientToExitDto = new ServerAsksClientToExitDto()
                    // {
                    //     ToAll = false,
                    //     ConnectionId = stationWithTheSameUser.ConnectionId,
                    //     Reason = UnRegisterReason.UserRegistersAnotherSession,
                    //
                    //     IsNewUserWeb = dto.IsWebClient,
                    //     NewAddress = dto.ClientIp,
                    // };

                    // var addresses = collection.GetAllDesktopClientsAddresses();
                    // if (addresses != null)
                    // {
                    //     collection.D2CWcfService.SetClientsAddresses(addresses);
                    //     await collection.D2CWcfService.ServerAsksClientToExit(serverAsksClientToExitDto);
                    // }
                    //
                    // await collection.FtSignalRClient.NotifyAll("ServerAsksClientToExit", serverAsksClientToExitDto.ToCamelCaseJson());
                    // await Task.Delay(1000);

                    if (collection.Clients.Remove(stationWithTheSameUser.ConnectionId, out ClientStation? _))
                        collection.Logger.LogInfo(Logs.Client, "Old client deleted");
                }
            }
            return null;
        }


        // R5
        public static ClientRegisteredDto CheckMachineKey(this ClientCollection collection, RegisterClientDto dto, User user)
        {
            if (!collection.WriteModel.IsMachineKeyRequired()) return new ClientRegisteredDto() { ReturnCode = ReturnCode.Ok };
            if (user.MachineKey == dto.MachineKey) return new ClientRegisteredDto() { ReturnCode = ReturnCode.Ok };

            if (string.IsNullOrEmpty(dto.SecurityAdminPassword))
            {
                // prohibited, call Security Admin to confirm 
                return user.MachineKey == null
                    ? new ClientRegisteredDto() { ReturnCode = ReturnCode.EmptyMachineKey }
                    : new ClientRegisteredDto() { ReturnCode = ReturnCode.WrongMachineKey };
            }

            var admin = collection.WriteModel.Users.First(u => u.Role == Role.SecurityAdmin);
            if (admin.EncodedPassword != dto.SecurityAdminPassword)
            {
                return new ClientRegisteredDto() { ReturnCode = ReturnCode.WrongSecurityAdminPassword };
            }

            // if SecurityAdminPassword is sent correctly or it is a first connection for user
            user.MachineKey = dto.MachineKey;

            return new ClientRegisteredDto() { ReturnCode = ReturnCode.SaveUsersMachineKey };
        }

        public static ClientRegisteredDto FillInSuccessfulResult(this ClientCollection collection, RegisterClientDto dto, User user)
        {
            var result = new ClientRegisteredDto();
            result.UserId = user.UserId;
            result.Role = user.Role;
            var zone = collection.WriteModel.Zones.First(z => z.ZoneId == user.ZoneId);
            result.ZoneId = zone.ZoneId;
            result.ZoneTitle = zone.Title;
            result.ConnectionId = dto.ClientConnectionId;
            result.DcCurrentParameters = collection.Config.Value;

            result.ReturnCode = ReturnCode.ClientRegisteredSuccessfully;
            return result;
        }
    }
}