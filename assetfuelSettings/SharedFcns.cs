using DataInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows_Desktop;
using static ForkAnalyticsSettings.GlobalConstants;

namespace ForkAnalyticsSettings
{
    public class SharedFcns
    {

        public static List<Tuple<double, string>> getSortedListByDouble(List<double> Item1List, List<string> Item2List)
        {
            var tmpList = new List<Tuple<double, string>>();
            for (int i = 0; i < Item1List.Count; i++)
            {
                tmpList.Add(new Tuple<double, string>(Item1List[i], Item2List[i]));
            }
            return tmpList.OrderBy(x => x.Item1).ToList(); ;
        }

        #region System Summary -> DTevent -> Double
        /* System Summary -> Double */
        public static List<double> getDoubleForMetricFromSystemReport(ref List<SystemSummaryReport> rawList, DowntimeMetrics Metric, string Name, DowntimeField Field)
        {
            List<List<DTeventSummary>> tmpList = getMappedEventListFromSystemReport(ref rawList);
            return getDoubleForMetricFromListOfDTevents(ref tmpList, Metric, Field, Name);
        }

        /* System Summary -> DTevent */
        public static List<List<DTeventSummary>> getMappedEventListFromSystemReport(ref List<SystemSummaryReport> rawData)
        {
            List<List<DTeventSummary>> tmpList = new List<List<DTeventSummary>>();
            for (int i = 0; i < rawData.Count; i++)
            {
                tmpList.Add(rawData[i].DT_Report.MappedDirectory);
            }
            return tmpList;
        }

        /* DTevent -> double */
        public static List<double> getDoubleForMetricFromListOfDTevents(ref List<List<DTeventSummary>> rawList, DowntimeMetrics Metric, DowntimeField Field, string Name)
        {
            List<DTeventSummary> eventList = new List<DTeventSummary>();
            List<double> metricList = new List<double>();

            for (int i = 0; i < rawList.Count; i++) //get the approprate DTevents
            {

            }
            return metricList;
        }
        #endregion
    }
}
