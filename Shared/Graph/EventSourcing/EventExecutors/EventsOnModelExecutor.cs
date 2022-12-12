namespace Fibertest.Graph
{
    public static class EventsOnModelExecutor
    {
        public static string? Apply(this Model model, object e)
        {
            try
            {
                switch (e)
                {
                    case ClientStationRegistered _: return null;
                    case ClientStationUnregistered _: return null;
                    case ClientConnectionLost _: return null;
                    case SnapshotMade _: return null;

                    case NodeIntoFiberAdded evnt: return model.AddNodeIntoFiber(evnt);
                    case NodeUpdated evnt: return model.UpdateNode(evnt);
                    case NodeUpdatedAndMoved evnt: return model.UpdateAndMoveNode(evnt);
                    case NodeMoved evnt: return model.MoveNode(evnt);
                    case NodeRemoved evnt: return model.RemoveNode(evnt);

                    case FiberAdded evnt: return model.AddFiber(evnt);
                    case FiberUpdated evnt: return model.UpdateFiber(evnt);
                    case FiberRemoved evnt: return model.RemoveFiber(evnt);

                    case EquipmentIntoNodeAdded evnt: return model.AddEquipmentIntoNode(evnt);
                    case EquipmentAtGpsLocationAdded evnt: return model.AddEquipmentAtGpsLocation(evnt);
                    case EquipmentAtGpsLocationWithNodeTitleAdded evnt: return model.AddEquipmentAtGpsLocationWithNodeTitle(evnt);
                    case EquipmentUpdated evnt: return model.UpdateEquipment(evnt);
                    case EquipmentRemoved evnt: return model.RemoveEquipment(evnt);
                    case EquipmentIntoTraceIncluded evnt: return model.IncludeEquipmentIntoTrace(evnt);
                    case EquipmentFromTraceExcluded evnt: return model.ExcludeEquipmentFromTrace(evnt);

                    case UnusedRemoved _: return model.RemoveUnused();

                    case RtuAtGpsLocationAdded evnt: return model.AddRtuAtGpsLocation(evnt);
                    case RtuUpdated evnt: return model.UpdateRtu(evnt);
                    case RtuRemoved evnt: return model.RemoveRtu(evnt);
                    case OtauAttached evnt: return model.AttachOtau(evnt);
                    case OtauDetached evnt: return model.DetachOtau(evnt);
                    case AllTracesDetached evnt: return model.DetachAllTraces(evnt);

                    case TraceAdded evnt: return model.AddTrace(evnt);
                    case TraceUpdated evnt: return model.UpdateTrace(evnt);
                    case TraceCleaned evnt: return model.CleanTrace(evnt);
                    case TraceRemoved evnt: return model.RemoveTrace(evnt);
                    case TraceAttached evnt: return model.AttachTrace(evnt);
                    case TraceDetached evnt: return model.DetachTrace(evnt);

                    case TceWithRelationsAddedOrUpdated evnt: return model.AddOrUpdateTceWithRelations(evnt);
                    case TceRemoved evnt: return model.RemoveTce(evnt);
                    case TceTypeStructListReSeeded evnt: return model.ReSeedTceTypes(evnt);

                    case UserAdded evnt: return model.AddUser(evnt);
                    case UserUpdated evnt: return model.UpdateUser(evnt);
                    case UserRemoved evnt: return model.RemoveUser(evnt);
                    case UsersMachineKeyAssigned evnt: return model.AssignUsersMachineKey(evnt);
                    case LicenseApplied evnt: return model.ApplyLicense(evnt);

                    case ZoneAdded evnt: return model.AddZone(evnt);
                    case ZoneUpdated evnt: return model.UpdateZone(evnt);
                    case ZoneRemoved evnt: return model.RemoveZone(evnt);
                    case ResponsibilitiesChanged evnt: return model.ChangeResponsibilities(evnt);

                    case BaseRefAssigned evnt: return model.AssignBaseRef(evnt);
                    case RtuInitialized evnt: return model.InitializeRtu(evnt);
                    case MonitoringSettingsChanged evnt: return model.ChangeMonitoringSettings(evnt);
                    case MonitoringStarted evnt: return model.StartMonitoring(evnt);
                    case MonitoringStopped evnt: return model.StopMonitoring(evnt);

                    case MeasurementAdded evnt: return model.AddMeasurement(evnt);
                    case MeasurementUpdated evnt: return model.UpdateMeasurement(evnt);

                    case NetworkEventAdded evnt: return model.AddNetworkEvent(evnt);
                    case BopNetworkEventAdded evnt: return model.AddBopNetworkEvent(evnt);

                    case EventsAndSorsRemoved evnt: return model.RemoveEventsAndSors(evnt);

                    case VeexTestAdded evnt: return model.AddVeexTest(evnt);
                    case VeexTestRemoved evnt: return model.RemoveVeexTest(evnt);

                    default: return @"Unknown event";
                }
            }
            catch (Exception exception)
            {
                return $@"EventsOnModelExecutor crashed while applying event {e.GetType().FullName}"
                       + Environment.NewLine + exception.Message;
            }
        }
    }
}