using System.Collections.Generic;
using Fibertest.StringResources;

namespace Fibertest.WpfCommonViews
{
    public static class LongOperationExt
    {
        public static List<string> ToLines(this LongOperation longOperation)
        {
            switch (longOperation)
            {
                case LongOperation.DrawingGraph:
                    return new List<string>()
                    {
                        Resources.SID_Drawing_graph_of_traces_,
                        "",
                        Resources.SID_Depending_on_graph_size_and_performance_of_your_PC,
                        Resources.SID_it_could_take_a_few_minutes_,
                        "",
                        Resources.SID_Please__wait___
                    };
                case LongOperation.DbOptimization:
                    return new List<string>()
                    {
                        Resources.SID_Database_optimization_,
                        "",
                        Resources.SID_Depending_on_database_size__choosen_parameters_and_performance_of_your_PC,
                        Resources.SID_it_could_take_a_few_minutes_,
                        "",
                        Resources.SID_Please__wait___
                    };
                case LongOperation.CollectingEventLog:
                    return new List<string>()
                    {
                        Resources.SID_Collecting_data_for_event_log___,
                        "",
                        Resources.SID_Depending_on_database_size__choosen_parameters_and_performance_of_your_PC,
                        Resources.SID_it_could_take_a_few_minutes_,
                        "",
                        Resources.SID_Please__wait___
                    };
                case LongOperation.PathFinding:
                    return new List<string>()
                    {
                        Resources.SID_Searching_route_for_new_trace___,
                        "",
                        Resources.SID_Depending_on_graph_size_and_performance_of_your_PC,
                        Resources.SID_it_could_take_a_few_minutes_,
                        "",
                        Resources.SID_Please__wait___
                    };
                //case LongOperation.MakingSnapshot:
                default: return new List<string>() { "Making snapshot...",
                    "",
                    Resources.SID_Depending_on_graph_size_and_performance_of_your_PC,
                    Resources.SID_it_could_take_a_few_minutes_,
                    "",
                    Resources.SID_Please__wait___
                };
            }
        }

    }
}