using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fibertest.Dto;
using Fibertest.Graph;
using Fibertest.StringResources;
using iText.StyledXmlParser.Jsoup.Internal;

namespace Fibertest.WpfClient;

public class AllOpticalEventsReportProvider
{
    private readonly DataCenterConfig _server;
    private readonly CurrentUser _currentUser;
    private readonly CurrentGis _currentGis;
    private readonly Model _readModel;
    private readonly OpticalEventsDoubleViewModel _opticalEventsDoubleViewModel;
    private readonly AccidentLineModelFactory _accidentLineModelFactory;
    private OpticalEventsReportModel _reportModel = null!;

    private List<OpticalEventModel> _events = null!;

    public AllOpticalEventsReportProvider(DataCenterConfig server, CurrentUser currentUser,
        CurrentGis currentGis, Model readModel,
        OpticalEventsDoubleViewModel opticalEventsDoubleViewModel, AccidentLineModelFactory accidentLineModelFactory)
    {
        _server = server;
        _currentUser = currentUser;
        _currentGis = currentGis;
        _readModel = readModel;
        _opticalEventsDoubleViewModel = opticalEventsDoubleViewModel;
        _accidentLineModelFactory = accidentLineModelFactory;
    }

    public string? CreateHtmlReport(OpticalEventsReportModel reportModel)
    {
        _reportModel = reportModel;
        _events = _currentUser.ZoneId == _reportModel.SelectedZone.ZoneId
            ? _opticalEventsDoubleViewModel.AllOpticalEventsViewModel.Rows.ToList()
            : FilterZoneEvents();

        var templateFileName = AppDomain.CurrentDomain.BaseDirectory +
                               @"Resources\Reports\OpticalEventsTemplate.html";
        if (!File.Exists(templateFileName))
            return null;
        var content = File.ReadAllText(templateFileName);

        var cssFile = "file:///" + AppDomain.CurrentDomain.BaseDirectory + @"Resources\Reports\OpticalEventsTemplate.css";
        content = content.Replace("PathToCss", cssFile);
        var headerPng = "file:///" + AppDomain.CurrentDomain.BaseDirectory + @"Resources\Reports\header-landscape.png";
        content = content.Replace("PathToHeaderPng", headerPng);

        var dict = DefineConstants();
        foreach (var pair in dict)
            content = content.Replace($"@{pair.Key}@", pair.Value);

        content = DrawConsolidatedTable(content);
        if (_reportModel.IsDetailedReport)
            content = DrawOpticalEvents(content);
        
        return content;
    }

    private List<OpticalEventModel> FilterZoneEvents()
    {
        var result = new List<OpticalEventModel>();
        foreach (var opticalEventModel in _opticalEventsDoubleViewModel.AllOpticalEventsViewModel.Rows)
        {
            var trace = _readModel.Traces.First(t => t.TraceId == opticalEventModel.TraceId);
            if (trace.ZoneIds.Contains(_reportModel.SelectedZone.ZoneId))
                result.Add(opticalEventModel);
        }
        return result;
    }

    private string DrawConsolidatedTable(string content)
    {
        var selectedStates = _reportModel.TraceStateSelectionViewModel.GetCheckedStates();
        var data = OpticalEventsReportFunctions.Create(_events, _reportModel);


        var tableHeader = "<th style=\"width: 20%\"></th>";
        var columnWidth = 80 / selectedStates.Count;
        foreach (var fiberState in selectedStates)
            tableHeader += $"<th style=\"width: {columnWidth}%\">{fiberState.ToLocalizedString()}</th>";
        content = content.Replace("@ConsolidatedTableHeader@", tableHeader);

        var rows = "";
        foreach (var list in data)
        {
            var row = "<tr>";
            for (int i = 0; i < list.Count; i++)
                row += $"<td>{list[i]}</td>";
            row += "</tr>";
            rows += row + Environment.NewLine;
        }

        content = content.Replace("@ConsolidatedTableBody@", rows);

        return content;
    }

    private Dictionary<int, DateTime> _closingTimes;
    private string DrawOpticalEvents(string content)
    {
        _closingTimes = _events.GetAccidentsClosingTimes();
        var checkedStatuses = _reportModel.EventStatusViewModel.GetCheckedStatuses();
        foreach (var eventStatus in EventStatusExt.EventStatusesInRightOrder.Where(eventStatus => checkedStatuses.Contains(eventStatus)))
        {
         //   DrawOpticalEventsWithStatus(section, eventStatus);
        }

        return content;
    }

    private Dictionary<string, string> DefineConstants()
    {
        return new Dictionary<string, string>
        {
            { "ReportTitle",
                string.Format(Resources.SID_Optical_events_report_for__0_d_____1_d_,
                    _reportModel.DateFrom, _reportModel.DateTo) },

            { "Server", string.Format(Resources.SID_Server_____0_____1_____2_,
                _server.General.ServerTitle, _server.General.ServerDoubleAddress.Main.Ip4Address,
                    string.Format(Resources.SID_software____0_, _server.General.DatacenterVersion)) },

            { "ResponsibilityZone", 
                string.Format(Resources.SID_Responsibility_zone___0_, _reportModel.SelectedZone.Title) }
        };
    }
}