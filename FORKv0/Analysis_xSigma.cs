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

        #region xSigma
        private SystemSummaryReport xSigma_AnalysisPeriodReport;
        private List<xSigma_Event> xSigma_MasterEventList = new List<xSigma_Event>();
        private List<xSigma_PeriodicSystemReport> DailySigmaReports = new List<xSigma_PeriodicSystemReport>();
        private List<SystemSummaryReport> xSigma_DailyReports = new List<SystemSummaryReport>();
        private const double xSigma_AnalysisDays = 60;
        private TopLevelSelected xSigma_TopLevelSelected = TopLevelSelected.Unplanned;

        public List<List<DTeventSummary>> CurrentMappedData = new List<List<DTeventSummary>>();

        private DateTime xSigma_Unplanned_Trend_StartDateSelected;
        private DateTime xSigma_Unplanned_Trend_EndDateSelected;
        private int xSigma_Unplanned_Trend_TimePeriodInDays = 1;

        #region Shared Intermediate or Planned/Unplanned Variables
        public void xSigma_PlannedUnplanned_ModeRefresh(TopLevelSelected PlannedUnplanned) //STUB
        {
            xSigma_TopLevelSelected = PlannedUnplanned;
            switch (PlannedUnplanned)
            {
                case TopLevelSelected.Planned:
                    xSigma_Planned_PopulateBubbleChart(); //
                    break;
                case TopLevelSelected.Unplanned:
                    xSigma_NewDateSelected(DailySigmaReports.Count - 1);
                    break;
            }
        }

        /* Daily Variables - For Bubble Chart Both Planned & Unplanned */
        public double xSigma_Daily_Yaxis_Max { get; set; }
        public double xSigma_Daily_Xaxis_Max { get; set; } = 10;
        public double xSigma_Daily_Size_Max { get; set; }
        public List<string> xSigma_Daily_Names { get; set; } = new List<string>();
        public List<double> xSigma_Daily_Yaxis_Values { get; set; } = new List<double>(); //Stops
        public List<double> xSigma_Daily_Xaxis_Values { get; set; } = new List<double>(); //cs or stability score
        public List<double> xSigma_Daily_Size_Values { get; set; } = new List<double>(); //Dtpct
        public List<int> xSigma_Daily_Color_Values { get; set; } = new List<int>(); //BETWEEN 1 and 4
        #endregion

        #region Unplanned
        #region Variables
        public List<xSigma_DisplayEvent> xSigma_Unplanned_DataList = new List<xSigma_DisplayEvent>(); //for display in the listview
        /* Trend Variables */
        public int xSigma_Trend_NumberOfDays { get; set; }
        public List<DateTime> xSigma_Trend_Dates { get; set; } = new List<DateTime>();
        public double xSigma_Trend_MaxUnplannedLoss { get; set; }
        public List<double> xSigma_TrendBottom_Values { get; set; } = new List<double>(); //chronic / stable
        public List<double> xSigma_TrendMiddle_Values { get; set; } = new List<double>();
        public List<double> xSigma_TrendTop_Values { get; set; } = new List<double>(); //sporadic / unstable
        internal List<double> xSigma_Selected_ControlChart_Mean { get; set; } = new List<double>();
        internal List<double> xSigma_Selected_ControlChart_StdDev { get; set; } = new List<double>();
        internal List<double> xSigma_Selected_ControlChart_Value { get; set; } = new List<double>();//daily MTBFs
        internal List<DateTime> xSigma_Selected_ControlChart_Dates { get; set; } = new List<DateTime>();
        #endregion

        internal void initializeSigmaControl()
        {
            xSigma_DailyReports = rawData.getPeriodicSubsets(new TimeSpan(hours: 24, minutes: 0, seconds: 0)); //make the daily reports
            xSigma_updateMappedData(); //update the mapped data
            xSigma_Trend_NumberOfDays = CurrentMappedData.Count - (int)xSigma_AnalysisDays;
            xSigma_GenerateEventData(); //get the CS report for every known dtevent
            xSigma_GenerateDailyData(); //now we have all the data by failure mode, but we need it by date
            xSigma_NewDateSelected(DailySigmaReports.Count - 1);
            xSigma_refreshTrends();

            xSigma_Planned_Initialize();
        }

        private void xSigma_GenerateEventData()
        {
            xSigma_Analysis tmpAnalysis;
            for (int dayInc = 0; dayInc < CurrentMappedData.Count; dayInc++) //for each day in our list of lists of DTeventSummary...
            {
                for (int eventInc = 0; eventInc < CurrentMappedData[dayInc].Count; eventInc++) //for each event in that day's list
                {
                    if (!xSigma_MasterEventList.Contains((CurrentMappedData[dayInc][eventInc])))
                    {
                        var tmpList = new List<double>();
                        var DTlist = new List<double>();
                        tmpList = getDoubleForMetricFromListOfDTevents(ref CurrentMappedData, DowntimeMetrics.Stops, DowntimeField.NA, CurrentMappedData[dayInc][eventInc].Name);
                        DTlist = getDoubleForMetricFromListOfDTevents(ref CurrentMappedData, DowntimeMetrics.DT, DowntimeField.NA, CurrentMappedData[dayInc][eventInc].Name);
                        tmpAnalysis = new xSigma_Analysis(tmpList, rawData.startTime, DTlist, xSigma_AnalysisDays);
                        xSigma_MasterEventList.Add(new xSigma_Event(CurrentMappedData[dayInc][eventInc].Name, DowntimeField.NA, tmpAnalysis.DataFields));
                        xSigma_MasterEventList[xSigma_MasterEventList.Count - 1].setAllCSscores();
                    }
                }
            }
        }

        private void xSigma_GenerateDailyData()
        {
            xSigma_Event tmpEvent;
            for (int dayInc = 0; dayInc < xSigma_Trend_NumberOfDays; dayInc++)
            {
                DailySigmaReports.Add(new xSigma_PeriodicSystemReport(xSigma_DailyReports[(int)xSigma_AnalysisDays + dayInc - 1].endTime));
                DailySigmaReports[dayInc].schedTime = xSigma_DailyReports[dayInc + (int)xSigma_AnalysisDays - 1].schedTime;
                for (int eventInc = 0; eventInc < xSigma_MasterEventList.Count; eventInc++)
                {
                    tmpEvent = xSigma_MasterEventList[eventInc].getSubset(DailySigmaReports[dayInc].StartTime);
                    DailySigmaReports[dayInc].DataList.Add(tmpEvent);
                }
                //now all raw data in place in daily sigma reports
                DailySigmaReports[dayInc].calculateAllParameters();
            }
        }

        private void xSigma_refreshTrends()
        {
            //clear lists
            xSigma_Trend_Dates.Clear();
            xSigma_Trend_MaxUnplannedLoss = 0;
            xSigma_TrendBottom_Values.Clear();
            xSigma_TrendMiddle_Values.Clear();
            xSigma_TrendTop_Values.Clear();

            //refresh lists
            for (int dayInc = 0; dayInc < xSigma_Trend_NumberOfDays; dayInc++)
            {
                if (DailySigmaReports[dayInc].DTpct * 100 > xSigma_Trend_MaxUnplannedLoss) { xSigma_Trend_MaxUnplannedLoss = 100 * DailySigmaReports[dayInc].DTpct; }
                xSigma_Trend_Dates.Add(DailySigmaReports[dayInc].StartTime);

                xSigma_TrendBottom_Values.Add(DailySigmaReports[dayInc].DTPct_Chronic * 100);
                xSigma_TrendTop_Values.Add(DailySigmaReports[dayInc].DTPct_Sporadic * 100);
                xSigma_TrendMiddle_Values.Add(DailySigmaReports[dayInc].DTPct_NotCS * 100);
            }
        }

        public void xSigma_Daily_FailureModeSelected(int IndexSelected)
        {
            int analysisPeriodIndex;
            //reset the lists
            xSigma_Selected_ControlChart_Value.Clear();
            xSigma_Selected_ControlChart_Mean.Clear();
            xSigma_Selected_ControlChart_StdDev.Clear();
            xSigma_Selected_ControlChart_Dates.Clear();

            //renew the lists
            string ModeName = xSigma_Daily_Names[IndexSelected];
            xSigma_Event tmp = xSigma_MasterEventList[xSigma_MasterEventList.IndexOf(new xSigma_Event(ModeName, DowntimeField.NA))];
            for (int i = 0; i < xSigma_Trend_NumberOfDays; i++)
            {
                xSigma_Selected_ControlChart_Value.Add(tmp.BaselineData[i].RawMetric);
                xSigma_Selected_ControlChart_Mean.Add(tmp.BaselineData[i].AdjMean);
                xSigma_Selected_ControlChart_StdDev.Add(tmp.BaselineData[i].AdjStdDev);
                xSigma_Selected_ControlChart_Dates.Add(tmp.BaselineData[i].StartDate);
            }

            //now update the KPIs
            analysisPeriodIndex = xSigma_AnalysisPeriodReport.DT_Report.MappedDirectory.IndexOf(new DTeventSummary(ModeName));
            if (analysisPeriodIndex < 0)
            {
                System.Windows.MessageBox.Show("Not found:" + ModeName);
            }
            else
            {
            }
        }

        /* Changes resolution of days of trend chart */
        public void xSigma_Trends_SetTimePeriodResolution(int numberOfDays)
        {
            xSigma_Unplanned_Trend_TimePeriodInDays = numberOfDays;
            xSigma_Trend_NumberOfDays = (xSigma_DailyReports.Count - (int)xSigma_AnalysisDays) / numberOfDays;
            if (numberOfDays == 1) { xSigma_refreshTrends(); }
            else
            {
                //clear lists
                xSigma_Trend_Dates.Clear();
                xSigma_Trend_MaxUnplannedLoss = 0;
                xSigma_TrendBottom_Values.Clear();
                xSigma_TrendMiddle_Values.Clear();
                xSigma_TrendTop_Values.Clear();

                var newPeriods = (int)Math.Floor((double)(DailySigmaReports.Count / numberOfDays));

                double tmpSched;
                double tmpTop;
                double tmpBot;
                double tmpMid;
                int k = DailySigmaReports.Count - 1;
                int x; //tmp int for loop

                for (int i = 0; i < newPeriods; i++)
                {
                    tmpSched = 0;
                    tmpTop = 0;
                    tmpBot = 0;
                    tmpMid = 0;
                    for (int j = 0; j < numberOfDays; j++)
                    {
                        x = k - j - i * numberOfDays;
                        tmpSched += DailySigmaReports[x].schedTime;
                        tmpBot += DailySigmaReports[x].DT_Chronic;
                        tmpMid += DailySigmaReports[x].DT_NotCS;
                        tmpTop += DailySigmaReports[x].DT_Sporadic;

                    }
                    xSigma_Trend_Dates.Add(DailySigmaReports[k - i * numberOfDays].StartTime);
                    xSigma_TrendBottom_Values.Add(tmpBot * 100 / tmpSched);
                    xSigma_TrendMiddle_Values.Add(tmpMid * 100 / tmpSched);
                    xSigma_TrendTop_Values.Add(tmpTop * 100 / tmpSched);
                    if ((tmpBot + tmpTop + tmpMid) * 100 / tmpSched > xSigma_Trend_MaxUnplannedLoss)
                    {
                        xSigma_Trend_MaxUnplannedLoss = (tmpBot + tmpTop + tmpMid) * 100 / tmpSched;
                    }
                }

                xSigma_Trend_Dates.Reverse();
                xSigma_TrendBottom_Values.Reverse();
                xSigma_TrendMiddle_Values.Reverse();
                xSigma_TrendTop_Values.Reverse();
            }
        }

        /* Change Daily Based On Selected Trend Date - Populate bubble chart & gridview */
        public void xSigma_NewDateSelected(int IndexSelected)
        {
            //clear the lists for bubble chart
            xSigma_Daily_Names.Clear();
            xSigma_Daily_Yaxis_Values.Clear();
            xSigma_Daily_Xaxis_Values.Clear();
            xSigma_Daily_Size_Values.Clear();

            //get start & end date for data table which should be same losses as bubble chart
            xSigma_Unplanned_Trend_StartDateSelected = DailySigmaReports[IndexSelected].StartTime;
            xSigma_Unplanned_Trend_EndDateSelected = xSigma_Unplanned_Trend_StartDateSelected.AddDays(xSigma_Unplanned_Trend_TimePeriodInDays);
            xSigma_AnalysisPeriodReport = rawData.getSubset(xSigma_Unplanned_Trend_StartDateSelected, xSigma_Unplanned_Trend_EndDateSelected);
            xSigma_AnalysisPeriodReport.reMapDowntime(xSigma_Mapping_A, xSigma_Mapping_B); //needs to be mapped correctly!!


            //repopulate the lists for bubble chart & gridview
            xSigma_Unplanned_DataList.Clear(); //bound to the planned telerik gridview
            for (int i = 0; i < xSigma_AnalysisPeriodReport.DT_Report.MappedDirectory.Count; i++)
            {
                //start with bubble values
                xSigma_Daily_Names.Add(xSigma_AnalysisPeriodReport.DT_Report.MappedDirectory[i].Name);
                xSigma_Daily_Size_Values.Add(xSigma_AnalysisPeriodReport.DT_Report.MappedDirectory[i].DT);
                xSigma_Daily_Yaxis_Values.Add(xSigma_AnalysisPeriodReport.DT_Report.MappedDirectory[i].Stops);

                //now find DataList index
                double tmpCSscore; int sigmaIndex = -1;
                for (int j = 0; j < DailySigmaReports[IndexSelected].DataList.Count; j++)
                {
                    if (xSigma_AnalysisPeriodReport.DT_Report.MappedDirectory[i].Name == DailySigmaReports[IndexSelected].DataList[j].Name)
                    {
                        sigmaIndex = j;
                    }
                }
                xSigma_Daily_Color_Values.Add((int)DailySigmaReports[IndexSelected].DataList[sigmaIndex].BaselineData[0].Stability_Score);
                tmpCSscore = DailySigmaReports[IndexSelected].DataList[sigmaIndex].BaselineData[0].xSigma_Score;
                xSigma_Daily_Xaxis_Values.Add(tmpCSscore);

                //lets add this to our gridview
                xSigma_Unplanned_DataList.Add(new xSigma_DisplayEvent(xSigma_AnalysisPeriodReport.DT_Report.MappedDirectory[i].Name, xSigma_AnalysisPeriodReport.DT_Report.MappedDirectory[i].Stops, xSigma_AnalysisPeriodReport.UT, xSigma_AnalysisPeriodReport.DT_Report.MappedDirectory[i].DT * 100 / xSigma_AnalysisPeriodReport.schedTime, tmpCSscore));
            }

            //reset the bubble chart maxes
            xSigma_Daily_Xaxis_Max = xSigma_Daily_Xaxis_Values.Max();
            xSigma_Daily_Yaxis_Max = xSigma_Daily_Yaxis_Values.Max();
            xSigma_Daily_Size_Max = xSigma_Daily_Size_Values.Max();
        }


        #endregion

        #region Planned
        private int xSigma_Planned_SelectedIndex = 0;
        internal SystemSummaryReport xSigma_Planned_AnalysisPeriodReport;
        private List<DTeventSummary> xSigma_Planned_AnalysisData;
        internal List<List<double>> xSigma_Planned_RawStopValues = new List<List<double>>();
        internal List<double> xSigma_Planned_Variations = new List<double>();

        internal double xSigma_Planned_getOverallMax()
        {
            double retVal = 0;
            for (int i = 0; i < xSigma_Planned_RawStopValues.Count; i++)
            {
                if (xSigma_Planned_RawStopValues[i].Max() > retVal) { retVal = xSigma_Planned_RawStopValues[i].Max(); }
            }
            return retVal;
        }

        internal double xSigma_Planned_getOverallMin()
        {
            double retVal = 1000; //arbitrarily large number
            for (int i = 0; i < xSigma_Planned_RawStopValues.Count; i++)
            {
                if (xSigma_Planned_RawStopValues[i].Min() < retVal) { retVal = xSigma_Planned_RawStopValues[i].Min(); }
            }
            return retVal;
        }


        private void xSigma_Planned_Initialize()
        {

            xSigma_Planned_AnalysisPeriodReport = rawData.getSubset(this.startTime, this.endTime);
            xSigma_Planned_AnalysisPeriodReport.reMapDowntime(xSigma_Mapping_A, xSigma_Mapping_B);
            xSigma_Planned_AnalysisData = xSigma_Planned_AnalysisPeriodReport.DT_Report.MappedDirectory_Planned;
            xSigma_Planned_GetRawDTvalues();

            xSigma_Planned_SetListView();
            xSigma_Planned_PopulateDistribution();
        }

        private void xSigma_Planned_SetListView()
        {
            xSigma_Planned_DataList.Clear();
            for (int i = 0; i < xSigma_Planned_AnalysisPeriodReport.DT_Report.MappedDirectory_Planned.Count; i++)
            {
                DTeventSummary tmpEvent = xSigma_Planned_AnalysisPeriodReport.DT_Report.MappedDirectory_Planned[i];
                xSigma_Planned_DataList.Add(new xSigma_DisplayEvent(tmpEvent.Name, tmpEvent.Stops, xSigma_Planned_AnalysisPeriodReport.UT, tmpEvent.DT * 100 / xSigma_Planned_AnalysisPeriodReport.schedTime, 1));
            }
        }
        public List<xSigma_DisplayEvent> xSigma_Planned_DataList = new List<xSigma_DisplayEvent>();

        private void xSigma_Planned_GetRawDTvalues()
        {
            //Get Values
            xSigma_Planned_RawStopValues.Clear();
            for (int i = 0; i < xSigma_Planned_AnalysisData.Count; i++)
            {
                var tmpList = new List<double>();
                for (int j = 0; j < rawData.DT_Report.rawDTdata.PlannedData.Count; j++)
                {
                    string testNameA = rawData.DT_Report.rawDTdata.PlannedData[j].getFieldFromInteger(xSigma_Mapping_A, xSigma_Mapping_B);
                    if (testNameA == xSigma_Planned_AnalysisData[i].Name)
                    {
                        tmpList.Add(rawData.DT_Report.rawDTdata.PlannedData[j].DT);
                    }
                }
                xSigma_Planned_RawStopValues.Add(tmpList);
                xSigma_Planned_Variations.Add(tmpList.StandardDeviation());
            }
        }

        private void xSigma_Planned_PopulateBubbleChart()
        {
            //first reset the appropriate values
            xSigma_Daily_Xaxis_Values.Clear();
            xSigma_Daily_Yaxis_Values.Clear();
            xSigma_Daily_Size_Values.Clear();
            xSigma_Daily_Names.Clear();

            for (int i = 0; i < xSigma_Planned_AnalysisData.Count; i++)
            {
                xSigma_Daily_Yaxis_Values.Add(xSigma_Planned_AnalysisData[i].Stops);
                xSigma_Daily_Size_Values.Add(xSigma_Planned_AnalysisData[i].DT);
                xSigma_Daily_Xaxis_Values.Add(xSigma_Planned_Variations[i]);
                xSigma_Daily_Names.Add(xSigma_Planned_AnalysisData[i].Name);
            }

            //set the appropriate max values
            xSigma_Daily_Xaxis_Max = xSigma_Daily_Xaxis_Values.Max();
            xSigma_Daily_Yaxis_Max = xSigma_Daily_Yaxis_Values.Max();
            xSigma_Daily_Size_Max = xSigma_Daily_Size_Values.Max();

        }

        //populate selected distribution for bell curve chart
        private void xSigma_Planned_PopulateDistribution()
        {
            xSigma_OnTarget_Selected_Distribution_EventDurations.Clear();
            if (xSigma_Planned_RawStopValues.Count > 0)
            {
                for (int i = 0; i < xSigma_Planned_RawStopValues[xSigma_Planned_SelectedIndex].Count; i++)
                {
                    xSigma_OnTarget_Selected_Distribution_EventDurations.Add(xSigma_Planned_RawStopValues[xSigma_Planned_SelectedIndex][i]);
                }
            }
            else
            {
                xSigma_OnTarget_Selected_Distribution_EventDurations.Add(0); //for when there are no planned events?
            }
            xSigma_OnTarget_Selected_Distribution_DurationMax = xSigma_OnTarget_Selected_Distribution_EventDurations.Max();
            xSigma_OnTarget_Selected_Distribution_DurationMin = xSigma_OnTarget_Selected_Distribution_EventDurations.Min();
            xSigma_OnTarget_Selected_Distribution_DurationTarget = xSigma_OnTarget_Selected_Distribution_EventDurations.Mean();
        }

        /* Control Chart */
        public double xSigma_OnTarget_Selected_Distribution_DurationMin { get; set; } = 0; //planned
        public double xSigma_OnTarget_Selected_Distribution_DurationMax { get; set; } = 0;
        public double xSigma_OnTarget_Selected_Distribution_DurationTarget { get; set; } = 0;
        public int xSigma_OnTarget_Selected_Distribution_NetEvents { get { return xSigma_OnTarget_Selected_Distribution_EventDurations.Count; } }
        public List<double> xSigma_OnTarget_Selected_Distribution_EventDurations { get; set; } = new List<double>(); //actual event durations (not histogram)

        /* Change Control Chart based on Selected Daily Loss */
        public void xS_OnTarget_FailureModeSelected(int IndexSelected)
        {
            xSigma_Planned_SelectedIndex = IndexSelected;
            xSigma_Planned_PopulateDistribution();
        }

        #endregion

        #region Helper Functions
        private void xSigma_updateMappedData()
        {
            CurrentMappedData.Clear();
            for (int i = 0; i < xSigma_DailyReports.Count; i++)
            {
                xSigma_DailyReports[i].reMapDowntime(xSigma_Mapping_A, xSigma_Mapping_B);
                CurrentMappedData.Add(xSigma_DailyReports[i].DT_Report.MappedDirectory);
            }
        }



        #endregion

        #region Mapping
        public void xSigma_CardRemap(string MappingA, string MappingB = "") { }
        public DowntimeField xSigma_Mapping_A { get; set; } = DowntimeField.Tier2;
        public DowntimeField xSigma_Mapping_B { get; set; } = DowntimeField.NA;

        #endregion
        #endregion

    }
}
