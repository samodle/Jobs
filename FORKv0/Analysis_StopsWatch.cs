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

        #region Stops Watch
        private const int StopsWatch_DaysToShow = 25;
        private List<SystemSummaryReport> StopsWatch_DailyData { get; set; } = new List<SystemSummaryReport>();
        private List<SystemSummaryReport> StopsWatch_HourlyData { get; set; } = new List<SystemSummaryReport>();
        private int StopsWatch_DaySelected { get; set; } = 0;
        private int StopsWatch_FailureSelected { get; set; } = 0;
        public void StopsWatch_initialize()
        {
            StopsWatch_DailyData = rawData.getPeriodicSubsets(new TimeSpan(hours: 24, minutes: 0, seconds: 0)); //make the daily reports
            StopsWatch_ReMap(StopsWatch_Mapping_A, StopsWatch_Mapping_B);
        }
        private void StopsWatch_SetSelectedHourlyData()
        {
            StopsWatch_HourlyData.Clear();
            DateTime firstHourStart = StopsWatch_DailyDates[StopsWatch_DaySelected];
            for (int i = 0; i < 24; i++)
            {
                StopsWatch_HourlyData.Add(rawData.getSubset(firstHourStart.AddHours(i), firstHourStart.AddHours(i + 1)));
            }
        }

        #region Daily
        public double StopsWatch_DailyMax { get; set; } = 0; //overall max for daily stops for all modes
        public double StopsWatch_DailyMax_FailureModes { get; set; } = 0;
        public List<List<double>> StopsWatch_DailyStops { get; set; } = new List<List<double>>(); //put daily line stops first
        public List<DateTime> StopsWatch_DailyDates { get; set; } = new List<DateTime>();
        public List<string> StopsWatch_DailyFailureModeNames { get; set; } = new List<string>();
        #endregion
        private void StopsWatch_setDailyIntermediate()
        {
            int tmpIndex;
            StopsWatch_DailyDates.Clear();
            StopsWatch_DailyStops.Clear();
            StopsWatch_DailyFailureModeNames.Clear();
            StopsWatch_DailyMax = 0;
            StopsWatch_DailyMax_FailureModes = 0;

            var tmpDailyStops = new List<double>();
            for (int i = StopsWatch_DailyData.Count - StopsWatch_DaysToShow; i < StopsWatch_DailyData.Count; i++)
            {
                StopsWatch_DailyDates.Add(StopsWatch_DailyData[i].startTime);
                tmpDailyStops.Add(StopsWatch_DailyData[i].Stops);
                for (int j = 0; j < StopsWatch_DailyData[i].DT_Report.MappedDirectory.Count; j++)
                {
                    tmpIndex = StopsWatch_DailyFailureModeNames.IndexOf(StopsWatch_DailyData[i].DT_Report.MappedDirectory[j].Name);
                    if (tmpIndex == -1) { StopsWatch_DailyFailureModeNames.Add(StopsWatch_DailyData[i].DT_Report.MappedDirectory[j].Name); }
                }
            }
            StopsWatch_DailyStops.Add(tmpDailyStops);//add the line stops first
            StopsWatch_DailyMax = Math.Max(StopsWatch_DailyMax, tmpDailyStops.Max());
            //now do the per failure mode
            for (int i = 0; i < StopsWatch_DailyFailureModeNames.Count; i++)
            {//for each of the known names
                var tmpStops = new List<double>();
                for (int j = StopsWatch_DailyData.Count - StopsWatch_DaysToShow; j < StopsWatch_DailyData.Count; j++)
                { //populate all the known values
                    tmpIndex = StopsWatch_DailyData[j].DT_Report.MappedDirectory.IndexOf(new DTeventSummary(StopsWatch_DailyFailureModeNames[i]));
                    if (tmpIndex == -1) { tmpStops.Add(0); }
                    else { tmpStops.Add(StopsWatch_DailyData[j].DT_Report.MappedDirectory[tmpIndex].Stops); }
                }
                StopsWatch_DailyStops.Add(tmpStops);
                StopsWatch_DailyMax_FailureModes = Math.Max(StopsWatch_DailyMax_FailureModes, tmpStops.Max());
            }
            StopsWatch_DailyFailureModeNames.Insert(0, "System");
            StopsWatch_sortDailyStopsList();
        }

        private void StopsWatch_sortDailyStopsList()
        {
            var sumsList = new List<double>();
            var sortTuple = new List<Tuple<double, int>>(); //double -> stops sum, int -> initial index
            for (int i = 0; i < StopsWatch_DailyFailureModeNames.Count; i++)
            {
                sumsList.Add(StopsWatch_DailyStops[i].Sum());
            }
            for (int i = 0; i < StopsWatch_DailyFailureModeNames.Count; i++)
            {
                sortTuple.Add(new Tuple<double, int>(sumsList[i], i));
            }
            List<Tuple<double, int>> result = sortTuple.OrderBy(x => x.Item1).ToList();

            //now actually sort it!
            var newNames = new List<string>();
            var newStops = new List<List<double>>();
            int tempIndex;
            for (int i = 0; i < StopsWatch_DailyFailureModeNames.Count; i++)
            {
                tempIndex = StopsWatch_DailyFailureModeNames.Count - i - 1;
                newNames.Add(StopsWatch_DailyFailureModeNames[result[tempIndex].Item2]);
                newStops.Add(StopsWatch_DailyStops[result[tempIndex].Item2]);
            }

            StopsWatch_DailyStops = newStops;
            StopsWatch_DailyFailureModeNames = newNames;
        }

        private void StopsWatch_setHourlyIntermediate()
        {
            StopsWatch_HourlyStops_24.Clear();
            StopsWatch_HourlyAvailability_24.Clear();
            StopsWatch_HourlyHadPlannedEvent_24.Clear();
            int tmpIndex;
            string FailureMode = StopsWatch_DailyFailureModeNames[StopsWatch_FailureSelected];
            if (StopsWatch_FailureSelected == 0) //if whole line
            {
                for (int j = 0; j < StopsWatch_HourlyData.Count; j++)
                {
                    StopsWatch_HourlyAvailability_24.Add(StopsWatch_HourlyData[j].OEE);
                    StopsWatch_HourlyStops_24.Add(StopsWatch_HourlyData[j].Stops);
                    if (StopsWatch_HourlyData[j].DT_Report.MappedDirectory_Planned.Count > 0) { StopsWatch_HourlyHadPlannedEvent_24.Add(true); }
                    else { StopsWatch_HourlyHadPlannedEvent_24.Add(false); }
                }
            }
            else //single failure mode
            {
                for (int j = 0; j < StopsWatch_HourlyData.Count; j++)
                {
                    StopsWatch_HourlyAvailability_24.Add(StopsWatch_HourlyData[j].OEE);
                    if (StopsWatch_HourlyData[j].DT_Report.MappedDirectory_Planned.Count > 0) { StopsWatch_HourlyHadPlannedEvent_24.Add(true); }
                    else { StopsWatch_HourlyHadPlannedEvent_24.Add(false); }
                    tmpIndex = StopsWatch_HourlyData[j].DT_Report.MappedDirectory.IndexOf(new DTeventSummary(FailureMode));
                    if (tmpIndex == -1)
                    {
                        StopsWatch_HourlyStops_24.Add(0);
                    }
                    else
                    {
                        StopsWatch_HourlyStops_24.Add(StopsWatch_HourlyData[j].DT_Report.MappedDirectory[tmpIndex].Stops);
                    }
                }
            }
        }


        #region Mapping
        public string StopsWatch_Mapping_A_String { get { return getStringForEnum(StopsWatch_Mapping_A); } }
        public string StopsWatch_Mapping_B_String { get { return getStringForEnum(StopsWatch_Mapping_B); } }
        public DowntimeField StopsWatch_Mapping_A { get; set; } = DowntimeField.Tier2;
        public DowntimeField StopsWatch_Mapping_B { get; set; } = DowntimeField.NA;
        public void StopsWatch_ReMap(string MappingA, string MappingB = "")
        {
            StopsWatch_ReMap(getEnumForString(MappingA), getEnumForString(MappingB));
        }
        private void StopsWatch_ReMap(DowntimeField MappingA, DowntimeField MappingB)
        {
            StopsWatch_Mapping_A = MappingA; //actually remap everything
            StopsWatch_Mapping_B = MappingB;
            for (int i = 0; i < StopsWatch_DailyData.Count; i++)
            {
                StopsWatch_DailyData[i].reMapDowntime(StopsWatch_Mapping_A, StopsWatch_Mapping_B);
            }
            //redo our intermediate sheets
            StopsWatch_setDailyIntermediate();
            StopsWatch_SetSelectedHourlyData();
            for (int i = 0; i < StopsWatch_HourlyData.Count; i++)
            {
                StopsWatch_HourlyData[i].reMapDowntime(StopsWatch_Mapping_A, StopsWatch_Mapping_B);
            }
            StopsWatch_setHourlyIntermediate();
            //set 12 hr view
            if (SW_isAM) { StopsWatch_SetAM(); } else { StopsWatch_SetPM(); }
        }
        #endregion

        #region Hourly
        private bool SW_isAM { get; set; } = true;
        public void StopsWatch_SetAM()
        {
            SW_isAM = true;
            StopsWatch_HourlyStops_12.Clear();
            StopsWatch_HourlyAvailability_12.Clear();
            StopsWatch_HourlyHadPlannedEvent_12.Clear();
            for (int i = 0; i < 12; i++)
            {
                StopsWatch_HourlyStops_12.Add(StopsWatch_HourlyStops_24[i]);
                StopsWatch_HourlyAvailability_12.Add(StopsWatch_HourlyAvailability_24[i]);
                StopsWatch_HourlyHadPlannedEvent_12.Add(StopsWatch_HourlyHadPlannedEvent_24[i]);
            }
        }
        public void StopsWatch_SetPM()
        {
            SW_isAM = false;
            StopsWatch_HourlyStops_12.Clear();
            StopsWatch_HourlyAvailability_12.Clear();
            StopsWatch_HourlyHadPlannedEvent_12.Clear();
            for (int i = 12; i < 24; i++)
            {
                StopsWatch_HourlyStops_12.Add(StopsWatch_HourlyStops_24[i]);
                StopsWatch_HourlyAvailability_12.Add(StopsWatch_HourlyAvailability_24[i]);
                StopsWatch_HourlyHadPlannedEvent_12.Add(StopsWatch_HourlyHadPlannedEvent_24[i]);
            }
        }
        public void StopsWatch_DailySelectionChange(int DayIndex, int FailureModeIndex) //0 FailureModeIndex for daily
        {
            StopsWatch_FailureSelected = FailureModeIndex;
            StopsWatch_DaySelected = DayIndex;
            StopsWatch_SetSelectedHourlyData();
            StopsWatch_ReMap(StopsWatch_Mapping_A, StopsWatch_Mapping_B);
        }
        public double StopsWatch_HourlyMax_12 { get { return StopsWatch_HourlyStops_12.Max(); } }
        public List<double> StopsWatch_HourlyStops_12 { get; set; } = new List<double>();
        public List<double> StopsWatch_HourlyAvailability_12 { get; set; } = new List<double>();
        public List<bool> StopsWatch_HourlyHadPlannedEvent_12 { get; set; } = new List<bool>(); //true if any planned event in that hour
        public double StopsWatch_HourlyMax_24 { get { return StopsWatch_HourlyStops_24.Max(); } }
        public List<double> StopsWatch_HourlyStops_24 { get; set; } = new List<double>();
        public List<double> StopsWatch_HourlyAvailability_24 { get; set; } = new List<double>();
        public List<bool> StopsWatch_HourlyHadPlannedEvent_24 { get; set; } = new List<bool>();
        #endregion
        #endregion

    }
}
