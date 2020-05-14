using Analytics;
using DataInterface;
using DataPersistancy;
using System;
using System.Collections.Generic;
using System.Linq;
using static DataPersistancy.JSON_IO;
using static ForkAnalyticsSettings.GlobalConstants;
using static Windows_Desktop.Window_Dashboard_Settings;

namespace Windows_Desktop
{
    public partial class Dashboard_Intermediate_Single
    {

        #region LiveLine
        public void LiveLine_initialize()
        {
            LiveLine_TimeFrameInDays = 1;
            LiveLine_AnalysisPeriodData = rawData.getSubset(AnalysisPeriodData.endTime.AddDays(-LiveLine_TimeFrameInDays), AnalysisPeriodData.endTime);
            LiveLine_AnalysisPeriodData_PreviousPeriod = rawData.getSubset(AnalysisPeriodData.endTime.AddDays(-2 * LiveLine_TimeFrameInDays), AnalysisPeriodData.endTime.AddDays(-LiveLine_TimeFrameInDays));
            LiveLine_AnalysisPeriodData.reMapDowntime(LiveLine_Mapping_A, LiveLine_Mapping_B);
            LiveLine_PopulateIntermediateSheet();
        }

        internal SystemSummaryReport LiveLine_AnalysisPeriodData;
        private SystemSummaryReport LiveLine_AnalysisPeriodData_PreviousPeriod;

        public DateTime LiveLine_SelectedStartTime { get { return LiveLine_AnalysisPeriodData.startTime; } }
        public DateTime LiveLine_SelectedEndTime { get { return LiveLine_AnalysisPeriodData.endTime; } }


        private int LiveLine_TimeFrameInDays { get; set; }
        private void LiveLine_PopulateIntermediateSheet()
        {
            LiveLine_ActualDurationOfEachEvent.Clear();
            LiveLine_EventStartTimes.Clear();
            LiveLine_DTviewer_EventNames.Clear();
            LiveLine_EventTypes.Clear();
            LiveLine_TopLosses.Clear();
            for (int i = 0; i < LiveLine_AnalysisPeriodData.rawData.Count; i++)
            {
                if (LiveLine_AnalysisPeriodData.rawData[i].UT > 0)
                {
                    //uptime
                    LiveLine_ActualDurationOfEachEvent.Add(LiveLine_AnalysisPeriodData.rawData[i].UT);
                    if (LiveLine_AnalysisPeriodData.rawData[i].isExcluded) { LiveLine_EventTypes.Add(EventType.Excluded); }
                    else { LiveLine_EventTypes.Add(EventType.Running); }
                    LiveLine_DTviewer_EventNames.Add("uptime");
                    LiveLine_EventStartTimes.Add(LiveLine_AnalysisPeriodData.rawData[i].startTime_UT);
                    //downtime

                    LiveLine_ActualDurationOfEachEvent.Add(LiveLine_AnalysisPeriodData.rawData[i].DT);
                    if (LiveLine_AnalysisPeriodData.rawData[i].isUnplanned) { LiveLine_EventTypes.Add(EventType.Unplanned); }
                    else if (LiveLine_AnalysisPeriodData.rawData[i].isPlanned) { LiveLine_EventTypes.Add(EventType.Planned); }
                    else if (LiveLine_AnalysisPeriodData.rawData[i].isExcluded) { LiveLine_EventTypes.Add(EventType.Excluded); }
                    else { LiveLine_EventTypes.Add(EventType.Excluded); }
                    LiveLine_DTviewer_EventNames.Add(LiveLine_AnalysisPeriodData.rawData[i].MappedField);
                    LiveLine_EventStartTimes.Add(LiveLine_AnalysisPeriodData.rawData[i].startTime);
                }
            }

            //Top Losses
            double prevPeriodOEE;
            double prevPeriodStops;
            int prevPeriodIndex;
            var topLosses = new List<Tuple<string, double, int, double, int>>();
            var topLosses_Planned = new List<Tuple<string, double, double>>();
            for (int i = 0; i < LiveLine_AnalysisPeriodData.DT_Report.MappedDirectory.Count; i++)
            {
                LiveLine_AnalysisPeriodData.DT_Report.MappedDirectory[i].SchedTime = LiveLine_AnalysisPeriodData.schedTime;

                prevPeriodIndex = LiveLine_AnalysisPeriodData_PreviousPeriod.DT_Report.MappedDirectory.IndexOf(new DTeventSummary(LiveLine_AnalysisPeriodData.DT_Report.MappedDirectory[i].Name));
                if (prevPeriodIndex >= 0)
                {
                    LiveLine_AnalysisPeriodData_PreviousPeriod.DT_Report.MappedDirectory[prevPeriodIndex].SchedTime = LiveLine_AnalysisPeriodData_PreviousPeriod.schedTime;
                    prevPeriodOEE = LiveLine_AnalysisPeriodData_PreviousPeriod.DT_Report.MappedDirectory[prevPeriodIndex].DTpct;
                    prevPeriodStops = LiveLine_AnalysisPeriodData_PreviousPeriod.DT_Report.MappedDirectory[prevPeriodIndex].Stops;
                }
                else
                {
                    prevPeriodOEE = 0;
                    prevPeriodStops = 0;
                }

                topLosses.Add(new Tuple<string, double, int, double, int>(LiveLine_AnalysisPeriodData.DT_Report.MappedDirectory[i].Name, LiveLine_AnalysisPeriodData.DT_Report.MappedDirectory[i].DTpct, (int)LiveLine_AnalysisPeriodData.DT_Report.MappedDirectory[i].Stops, prevPeriodOEE, (int)prevPeriodStops));
            }
            LiveLine_TopLosses = topLosses.OrderBy(x => x.Item2).ToList();
            LiveLine_TopLosses.Reverse();

            //planned
            for (int i = 0; i < LiveLine_AnalysisPeriodData.DT_Report.MappedDirectory_Planned.Count; i++)
            {
                LiveLine_AnalysisPeriodData.DT_Report.MappedDirectory_Planned[i].SchedTime = LiveLine_AnalysisPeriodData.schedTime;
                topLosses_Planned.Add(new Tuple<string, double, double>(LiveLine_AnalysisPeriodData.DT_Report.MappedDirectory_Planned[i].Name, LiveLine_AnalysisPeriodData.DT_Report.MappedDirectory_Planned[i].DT, LiveLine_AnalysisPeriodData.DT_Report.MappedDirectory_Planned[i].DTpct));
            }
            LiveLine_Planned = topLosses_Planned.OrderBy(x => x.Item2).ToList();
            LiveLine_Planned.Reverse();

            //trends
            LiveLine_populateIntermediate_Trends();

            //update the biggest changes
            LiveLine_BiggestChanges.Clear();
            for (int i = 0; i < LiveLine_TopLosses.Count; i++)
            {
                LiveLine_BiggestChanges.Add(new Tuple<string, double, double>(LiveLine_TopLosses[i].Item1, LiveLine_TopLosses[i].Item4 - LiveLine_TopLosses[i].Item2, LiveLine_TopLosses[i].Item5 - LiveLine_TopLosses[i].Item3));
            }
        }


        #region Mapping
        public DowntimeField LiveLine_Mapping_A { get; set; } = DowntimeField.Tier2;
        public DowntimeField LiveLine_Mapping_B { get; set; } = DowntimeField.NA;
        public void LiveLine_ReMap(DowntimeField MappingA, DowntimeField MappingB)
        {
            LiveLine_Mapping_A = MappingA;
            LiveLine_Mapping_B = MappingB;
            LiveLine_AnalysisPeriodData.reMapDowntime(MappingA, MappingB);
            LiveLine_AnalysisPeriodData_PreviousPeriod.reMapDowntime(MappingA, MappingB);
            LiveLine_PopulateIntermediateSheet();
        }
        #endregion
        #region DTviewer
        public void LiveLine_setDTviewerTimeFrame(int timeFrameInDays)
        {
            LiveLine_TimeFrameInDays = timeFrameInDays;
            LiveLine_AnalysisPeriodData = rawData.getSubset(AnalysisPeriodData.endTime.AddDays(-timeFrameInDays), AnalysisPeriodData.endTime);
            LiveLine_AnalysisPeriodData_PreviousPeriod = rawData.getSubset(AnalysisPeriodData.endTime.AddDays(-2 * LiveLine_TimeFrameInDays), AnalysisPeriodData.endTime.AddDays(-LiveLine_TimeFrameInDays));

            LiveLine_PopulateIntermediateSheet();
        }
        public List<double> LiveLine_ActualDurationOfEachEvent { get; set; } = new List<double>();
        public int LiveLine_NumberOfEvents { get { return LiveLine_ActualDurationOfEachEvent.Count; } }
        public List<EventType> LiveLine_EventTypes { get; set; } = new List<EventType>();
        public List<DateTime> LiveLine_EventStartTimes { get; set; } = new List<DateTime>();
        public List<string> LiveLine_DTviewer_EventNames { get; set; } = new List<string>();
        #endregion
        #region Top Loss / Biggest Changes
        //string-> name, double -> DTpct, int -> num of stops, 2nd double -> previous period loss amount, 2nd int -> previous period stops amt
        public List<Tuple<string, double, int, double, int>> LiveLine_TopLosses = new List<Tuple<string, double, int, double, int>>();
        //double -> dtpct/OEE impact, 2nd double -> duration. sorted by longest DT
        public List<Tuple<string, double, double>> LiveLine_Planned = new List<Tuple<string, double, double>>();

        public List<Tuple<string, double, double>> LiveLine_BiggestChanges = new List<Tuple<string, double, double>>();

        public double LiveLine_TopLoss_MaxValue_Planned { get { return LiveLine_Planned.Count == 0 ? 0 : LiveLine_Planned.Select(t => t.Item2).ToList().Max(); } }
        public double LiveLine_TopLoss_MaxLossValue { get { return LiveLine_TopLosses.Count == 0 ? 0 : LiveLine_TopLosses.Select(t => t.Item2).ToList().Max(); } }
        #endregion
        #region Trend
        //date -> start time, double -> OEE, int -> stops
        public List<Tuple<DateTime, double, int>> LiveLine_TrendsData = new List<Tuple<DateTime, double, int>>();
        private List<SystemSummaryReport> LiveLine_TrendRawData = new List<SystemSummaryReport>();
        public double LiveLine_Trends_MaxOEE { get { return LiveLine_TrendsData.Select(t => t.Item2).ToList().Max(); } }
        public double LiveLine_Trends_MaxStops { get { return LiveLine_TrendsData.Select(t => t.Item3).ToList().Max(); } }

        private void LiveLine_populateIntermediate_Trends()
        {
            double TrendTimePeriod_Hours = 1;
            double TrendTimePeriod_Number = 24;
            DateTime tmpStartTime; DateTime tmpEndTime;

            LiveLine_TrendsData.Clear();
            LiveLine_TrendRawData.Clear();

            //figure out the time periods to display
            if (LiveLine_TimeFrameInDays < 7)
            {
                TrendTimePeriod_Hours = 1;
                TrendTimePeriod_Number = 24;
            }
            else if (LiveLine_TimeFrameInDays < 30)
            {
                TrendTimePeriod_Hours = 24;
                TrendTimePeriod_Number = 7;
            }
            else
            {
                TrendTimePeriod_Hours = 24;
                TrendTimePeriod_Number = 30;
            }

            //populate the raw data list
            for (int i = 0; i < TrendTimePeriod_Number; i++)
            {
                tmpEndTime = rawData.endTime.AddHours(-(i * TrendTimePeriod_Hours));
                tmpStartTime = tmpEndTime.AddHours(-TrendTimePeriod_Hours);
                LiveLine_TrendRawData.Add(rawData.getSubset(tmpStartTime, tmpEndTime));
            }
            LiveLine_TrendRawData.Reverse();
            //convert raw data list to intermediate sheet
            for (int i = 0; i < LiveLine_TrendRawData.Count; i++)
            {
                LiveLine_TrendsData.Add(new Tuple<DateTime, double, int>(LiveLine_TrendRawData[i].startTime, LiveLine_TrendRawData[i].OEE, (int)LiveLine_TrendRawData[i].Stops));
            }
        }
        #endregion


        #endregion

    }
}
