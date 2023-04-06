using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.StringResources;
using Fibertest.Utils;
using Newtonsoft.Json;

namespace Fibertest.DataCenter
{
    public partial class EventStoreService
    {
        private async Task<string?> AmendForTracesWhichUseThisNode(Guid nodeId)
        {
            var tracesWhichUseThisNode =
                _writeModel.Traces.Where(t => t.NodeIds.Contains(nodeId) && t.HasAnyBaseRef).ToList();
            return await AmendBaseRefs(tracesWhichUseThisNode);
        }

        private async Task<string?> AmendForTracesFromRtu(Guid rtuId)
        {
            var traceFromRtu =
                _writeModel.Traces.Where(t => t.RtuId == rtuId && t.HasAnyBaseRef).ToList();
            return await AmendBaseRefs(traceFromRtu);
        }

        private async Task<string?> ProcessUpdateEquipment(Guid equipmentId)
        {
            var tracesWhichUseThisEquipment = _writeModel.Traces
                .Where(t => t.EquipmentIds.Contains(equipmentId) && t.HasAnyBaseRef).ToList();
            return await AmendBaseRefs(tracesWhichUseThisEquipment);
        }

        private async Task<string?> ProcessUpdateFiber(Guid fiberId)
        {
            var tracesWhichUseThisFiber =
                _writeModel.GetTracesPassingFiber(fiberId).Where(t => t.HasAnyBaseRef).ToList();
            return await AmendBaseRefs(tracesWhichUseThisFiber);
        }

        private async Task<string?> ProcessNodeRemoved(List<Guid> traceIds)
        {
            var tracesWhichUsedThisNode = new List<Trace>();
            foreach (var id in traceIds)
            {
                var trace = _writeModel.Traces.FirstOrDefault(t => t.TraceId == id);
                if (trace != null && trace.HasAnyBaseRef)
                    tracesWhichUsedThisNode.Add(trace);
            }
            return await AmendBaseRefs(tracesWhichUsedThisNode);
        }

        private async Task<string?> AmendBaseRefs(List<Trace> traces)
        {
            string? returnStr = null;
            foreach (var trace in traces)
            {
                var listOfBaseRef = await GetBaseRefDtos(trace);

                if (!listOfBaseRef.Any())
                    return string.Format(Resources.SID_Can_t_get_base_refs_for_trace__0_, trace.TraceId.First6());

                foreach (var baseRefDto in listOfBaseRef.Where(b => b.SorFileId > 0))
                {
                    Modify(trace, baseRefDto);
                    if (await _sorFileRepository.UpdateSorBytesAsync(baseRefDto.SorFileId, baseRefDto.SorBytes!) == -1)
                        return Resources.SID_Can_t_save_amended_reflectogram;
                }

                if (trace.OtauPort == null) // unattached trace
                    continue;

                var rtu = _writeModel.Rtus.FirstOrDefault(r => r.Id == trace.RtuId);
                if (rtu == null)
                    return "RTU not found.";

                if (rtu.IsAvailable)
                {
                    var result = await SendAmendedBaseRefsToRtu(rtu, trace, listOfBaseRef);
                    if (result.ReturnCode != ReturnCode.BaseRefAssignedSuccessfully)
                        returnStr = result.ErrorMessage;

                    //var updateResult = await UpdateVeexTestList(result, "system", "localhost");
                    //if (updateResult.ReturnCode != ReturnCode.Ok)
                    //    returnStr = updateResult.ErrorMessage;
                }
                else
                {
                    returnStr = @"SID_Failed_to_resend_base_to_rtu"; // resolve in client
                }
            }

            return returnStr;
        }


        private async Task<List<BaseRefDto>> GetBaseRefDtos(Trace trace)
        {
            var list = new List<BaseRef?>
            {
                _writeModel.BaseRefs.FirstOrDefault(b => b.Id == trace.PreciseId),
                _writeModel.BaseRefs.FirstOrDefault(b => b.Id == trace.FastId),
                _writeModel.BaseRefs.FirstOrDefault(b => b.Id == trace.AdditionalId && b.BaseRefType == BaseRefType.Additional)
            };

            var listOfBaseRef = new List<BaseRefDto>();

            foreach (var baseRef in list)
            {
                if (baseRef == null) continue;
                var sorBytes = await _sorFileRepository.GetSorBytesAsync(baseRef.SorFileId);
                if (sorBytes == null)
                    continue;
                listOfBaseRef.Add(baseRef.CreateFromBaseRef(sorBytes));
            }

            _logger.Info(Logs.DataCenter, $"{listOfBaseRef.Count} base refs changed");
            return listOfBaseRef;
        }

        private void Modify(Trace trace, BaseRefDto baseRefDto)
        {
            try
            {
                var sorData = SorData.FromBytes(baseRefDto.SorBytes!);

                _baseRefLandmarksTool.ApplyTraceToBaseRef(sorData, trace, false);

                baseRefDto.SorBytes = sorData.ToBytes();
            }
            catch (Exception e)
            {
                _logger.Exception(Logs.DataCenter, e, "Amend base ref - Modify: ");
            }
        }

        private async Task<BaseRefAssignedDto> SendAmendedBaseRefsToRtu(Graph.Rtu rtu, Trace trace, List<BaseRefDto> baseRefDtos)
        {
            var dto = new AssignBaseRefsDto(rtu.Id, rtu.RtuMaker, trace.TraceId, baseRefDtos, new List<int>())
            {
                OtdrId = rtu.OtdrId,
                OtauPortDto = trace.OtauPort,
                MainOtauPortDto = new OtauPortDto(trace.OtauPort!.MainCharonPort, true)
                {
                    OtauId = rtu.MainVeexOtau.id,
                    Serial = rtu.MainVeexOtau.serialNumber,
                },
            };

            if (dto.RtuMaker != RtuMaker.IIT)
                return new BaseRefAssignedDto(ReturnCode.NotImplementedYet);

            var rtuAddress = await GetRtuAddress(dto.RtuId);
            if (rtuAddress == null) return new BaseRefAssignedDto(ReturnCode.RtuNotFound);

            var jsonResponse = await _clientToIitRtuTransmitter
                .TransferCommand(rtuAddress, JsonConvert.SerializeObject(dto));
            var result = JsonConvert.DeserializeObject<BaseRefAssignedDto>(jsonResponse);
            if (result == null) return new BaseRefAssignedDto(ReturnCode.FailedDeserializeJson);
            return result;
        }

        private async Task<string?> GetRtuAddress(Guid rtuId)
        {
            var rtuStation = await _rtuStationsRepository.GetRtuStation(rtuId);
            if (rtuStation == null)
                return null;

            return rtuStation.GetRtuAvailableAddress();
        }
    }
}
