using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.StringResources;

namespace Fibertest.WpfClient
{
    public class ObjectsAsTreeToZonesViewModel : Screen
    {
        private readonly IWcfServiceDesktopC2D _c2DWcfManager;
        public Model ReadModel { get; }
        public List<ObjectToZonesModel> Rows { get; set; } = new  List<ObjectToZonesModel>();
        public ObjectToZonesModel SelectedRow { get; set; }

        public bool IsEnabled { get; set; }

        public ObjectsAsTreeToZonesViewModel(Model readModel, CurrentUser currentUser, IWcfServiceDesktopC2D c2DWcfManager)
        {
            _c2DWcfManager = c2DWcfManager;
            ReadModel = readModel;
            IsEnabled = currentUser.Role <= Role.Root;

            FillInSortedRows();
        }
        

        protected override void OnViewLoaded(object view)
        {
            DisplayName = Resources.SID_Responsibility_zones_subjects;
        }

        private void FillInSortedRows()
        {
            foreach (var rtu in ReadModel.Rtus)
            {
                Rows.Add(RtuToLine(rtu));
                foreach (var trace in ReadModel.Traces.Where(t=>t.RtuId == rtu.Id))
                {
                    Rows.Add(TraceToLine(trace));
                }
            }
        }

        private ObjectToZonesModel RtuToLine(Rtu rtu)
        {
            var rtuLine = new ObjectToZonesModel()
            {
                SubjectTitle = rtu.Title,
                RtuId = rtu.Id,
                IsRtu = true,
            };
            foreach (var zone in ReadModel.Zones)
                rtuLine.IsInZones.Add(new BoolWithNotification() { IsChecked = rtu.ZoneIds.Contains(zone.ZoneId) });
            return rtuLine;
        }

        private ObjectToZonesModel TraceToLine(Trace trace)
        {
            var traceLine = new ObjectToZonesModel()
            {
                SubjectTitle = @"  " + trace.Title,
                TraceId = trace.TraceId,
                RtuId = trace.RtuId,
                IsRtu = false,
            };
            foreach (var zone in ReadModel.Zones)
                traceLine.IsInZones.Add(new BoolWithNotification() { IsChecked = trace.ZoneIds.Contains(zone.ZoneId) });

            return traceLine;
        }

        /// <summary>
        /// Command contains only CHANGES in responsibilities
        /// </summary>
        /// <returns></returns>
        private ChangeResponsibilities PrepareCommand()
        {
            var command = new ChangeResponsibilities() { ResponsibilitiesDictionary = new Dictionary<Guid, List<Guid>>() };
            foreach (var lineModel in Rows)
            {
                var subjectId = lineModel.IsRtu ? lineModel.RtuId : lineModel.TraceId;
                var oldListOfZones = GetOldListOfZones(lineModel);
                var subjectZones = GetChangedZonesForSubject(lineModel, oldListOfZones);
                command.ResponsibilitiesDictionary.Add(subjectId, subjectZones);
            }
            return command;
        }

        private List<Guid> GetChangedZonesForSubject(ObjectToZonesModel lineModel, List<Guid> oldListOfZones)
        {
            var changedZones = new List<Guid>();
            for (int j = 1; j < ReadModel.Zones.Count; j++)
            {
                var isChecked = lineModel.IsInZones[j].IsChecked;
                var zoneId = ReadModel.Zones[j].ZoneId;

                if (isChecked ^ oldListOfZones.Contains(zoneId)) // put in list only if value is changed 
                    changedZones.Add(zoneId);
            }

            return changedZones;
        }

        private List<Guid> GetOldListOfZones(ObjectToZonesModel lineModel)
        {
            return lineModel.IsRtu
                ? ReadModel.Rtus.First(r => r.Id == lineModel.RtuId).ZoneIds
                : ReadModel.Traces.First(t => t.TraceId == lineModel.TraceId).ZoneIds;
        }

        public async void Save()
        {
            await _c2DWcfManager.SendCommandAsObj(PrepareCommand());

            await TryCloseAsync();
        }
        public async void Cancel()
        {
            await TryCloseAsync();
        }
    }
}
