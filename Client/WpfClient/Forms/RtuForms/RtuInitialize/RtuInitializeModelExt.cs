using System.Threading.Tasks;
using Fibertest.Dto;
using Fibertest.StringResources;
using Fibertest.WpfCommonViews;

namespace Fibertest.WpfClient
{
    public static class RtuInitializeModelExt
    {
        public static InitializeRtuDto CreateDto(this RtuInitializeModel fullModel, 
            RtuMaker rtuMaker, DataCenterConfig currentDatacenterParameters)
        {
            if (fullModel.IsReserveChannelEnabled && fullModel.ReserveChannelTestViewModel.NetAddressInputViewModel.Port == -1)
                fullModel.ReserveChannelTestViewModel.NetAddressInputViewModel.Port = rtuMaker == RtuMaker.IIT
                    ? (int)TcpPorts.RtuListenToGrpc
                : (int)TcpPorts.RtuVeexListenTo;

            if (fullModel.MainChannelTestViewModel.NetAddressInputViewModel.Port == -1)
                fullModel.MainChannelTestViewModel.NetAddressInputViewModel.Port = rtuMaker == RtuMaker.IIT
                    ? (int)TcpPorts.RtuListenToGrpc
                    : (int)TcpPorts.RtuVeexListenTo;
            var initializeRtuDto = new InitializeRtuDto(fullModel.OriginalRtu.Id, rtuMaker)
            {
                ServerAddresses = currentDatacenterParameters.General.ServerDoubleAddress,

                Serial = fullModel.OriginalRtu.Serial, // properties after previous initialization (if it was)
                OwnPortCount = fullModel.OriginalRtu.OwnPortCount,
                MainVeexOtau = fullModel.OriginalRtu.MainVeexOtau,
                Children = fullModel.OriginalRtu.Children,

                RtuAddresses = new DoubleAddress()
                {
                    Main = fullModel.MainChannelTestViewModel.NetAddressInputViewModel.GetNetAddress(),
                    HasReserveAddress = fullModel.IsReserveChannelEnabled,
                    Reserve = fullModel.IsReserveChannelEnabled
                        ? fullModel.ReserveChannelTestViewModel.NetAddressInputViewModel.GetNetAddress()
                        : null,
                },
                IsFirstInitialization =
                    fullModel.OriginalRtu.OwnPortCount ==
                    0, // if it's first initialization for this RTU - monitoring should be stopped - in case it's running somehow
            };

            if (!initializeRtuDto.RtuAddresses.HasReserveAddress)
            // if RTU has no reserve address it should not send to server's reserve address
            // (it is an ideological requirement)
                initializeRtuDto.ServerAddresses.HasReserveAddress = false;

            return initializeRtuDto;
        }

        public static async Task<bool> CheckConnectionBeforeInitialization(this RtuInitializeModel fullModel)
        {
            if (!fullModel.MainChannelTestViewModel.NetAddressInputViewModel.IsValidIpAddress())
            {
                await fullModel.WindowManager.ShowDialogWithAssignedOwner(
                    new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Invalid_IP_address));
                return false;
            }
            if (!await fullModel.MainChannelTestViewModel.ExternalTest(fullModel.OriginalRtu.Id, fullModel.OriginalRtu.RtuMaker))
            {
                await fullModel.WindowManager.ShowDialogWithAssignedOwner(
                    new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Cannot_establish_connection_with_RTU_));
                return false;
            }

            if (!fullModel.IsReserveChannelEnabled) return true;

            if (!fullModel.ReserveChannelTestViewModel.NetAddressInputViewModel.IsValidIpAddress())
            {
                await fullModel.WindowManager.ShowDialogWithAssignedOwner(
                    new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Invalid_IP_address));
                return false;
            }
            if (await fullModel.ReserveChannelTestViewModel.ExternalTest(fullModel.OriginalRtu.Id, fullModel.OriginalRtu.RtuMaker)) return true;

            await fullModel.WindowManager.ShowDialogWithAssignedOwner(
                new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Cannot_establish_connection_with_RTU_));
            return false;
        }
    }
}
