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

        #region Trends
        private List<List<DTeventSummary>> Trends_CurrentMappedData = new List<List<DTeventSummary>>();
        private List<List<DTeventSummary>> Trends_CurrentMappedData_Planned = new List<List<DTeventSummary>>();


        internal void initializeTrends()
        {
            Multi_RemapSystemReports(Trends_Mode_MappingA, Trends_Mode_MappingB);
            initializeLineTrends();
            DailyReports = rawData.getPeriodicSubsets(new TimeSpan(hours: 24, minutes: 0, seconds: 0)); //make the daily reports
            initializeModeTrends();
            initializeStepTrends();
        }

        public void Trends_updateMappedData()
        {
            Trends_CurrentMappedData.Clear();
            Trends_CurrentMappedData_Planned.Clear();
            for (int i = 0; i < DailyReports.Count; i++)
            {
                Trends_CurrentMappedData.Add(DailyReports[i].DT_Report.MappedDirectory);
                Trends_CurrentMappedData_Planned.Add(DailyReports[i].DT_Report.MappedDirectory_Planned);
            }
        }

        #region Line / Summary
        #region Variables
        internal List<DowntimeMetrics> Trends_Line_MasterMetricList = new List<DowntimeMetrics> { DowntimeMetrics.OEE, DowntimeMetrics.Stops, DowntimeMetrics.UPDTpct, DowntimeMetrics.PDTpct, DowntimeMetrics.UnitsProduced, DowntimeMetrics.SPD, DowntimeMetrics.SKUs, DowntimeMetrics.MTBF, DowntimeMetrics.NumChangeovers };

        //Line<Metric<Value>>
        internal List<List<List<double>>> Trends_Line_MasterDataList_Daily = new List<List<List<double>>>();
        internal List<List<List<double>>> Trends_Line_MasterDataList_Weekly = new List<List<List<double>>>();
        internal List<List<List<double>>> Trends_Line_MasterDataList_Monthly = new List<List<List<double>>>();

        //Metric<Value>
        internal List<List<double>> Trends_Line_MasterDataList_Daily_RollUp = new List<List<double>>();
        internal List<List<double>> Trends_Line_MasterDataList_Weekly_RollUp = new List<List<double>>();
        internal List<List<double>> Trends_Line_MasterDataList_Monthly_RollUp = new List<List<double>>();
        #endregion

        #region GlidePath 
        private bool Trends_IsGlidePathOn = false;
        private CrystalBallAnalysis Trends_GlidePath_ActiveChangelog { get; set; } = null;
        #endregion

        private void initializeLineTrends()
        {
            Trends_Line_MasterDataList_Daily.Clear();
            Trends_Line_MasterDataList_Weekly.Clear();
            Trends_Line_MasterDataList_Monthly.Clear();
            //generate master daily data list
            for (int lineInc = 0; lineInc < Multi_AllSystemReports_Daily.Count; lineInc++)
            {
                var tmpMetricList = new List<List<double>>();
                for (int metricInc = 0; metricInc < Trends_Line_MasterMetricList.Count; metricInc++)
                {
                    var tmpValueList = new List<double>();
                    for (int i = 0; i < Multi_AllSystemReports_Daily[lineInc].Count; i++)
                    {
                        tmpValueList.Add(Multi_AllSystemReports_Daily[lineInc][i].getKPIforMetric(Trends_Line_MasterMetricList[metricInc]));
                    }
                    tmpMetricList.Add(tmpValueList);
                }
                Trends_Line_MasterDataList_Daily.Add(tmpMetricList);
            }
            //weekly
            for (int lineInc = 0; lineInc < Multi_AllSystemReports_Weekly.Count; lineInc++)
            {
                var tmpMetricList = new List<List<double>>();
                for (int metricInc = 0; metricInc < Trends_Line_MasterMetricList.Count; metricInc++)
                {
                    var tmpValueList = new List<double>();
                    for (int i = 0; i < Multi_AllSystemReports_Weekly[lineInc].Count; i++)
                    {
                        tmpValueList.Add(Multi_AllSystemReports_Weekly[lineInc][i].getKPIforMetric(Trends_Line_MasterMetricList[metricInc]));
                    }
                    tmpMetricList.Add(tmpValueList);
                }
                Trends_Line_MasterDataList_Weekly.Add(tmpMetricList);
            }
            //monthly
            for (int lineInc = 0; lineInc < Multi_AllSystemReports_Monthly.Count; lineInc++)
            {
                var tmpMetricList = new List<List<double>>();
                for (int metricInc = 0; metricInc < Trends_Line_MasterMetricList.Count; metricInc++)
                {
                    var tmpValueList = new List<double>();
                    for (int i = 0; i < Multi_AllSystemReports_Monthly[lineInc].Count; i++)
                    {
                        tmpValueList.Add(Multi_AllSystemReports_Monthly[lineInc][i].getKPIforMetric(Trends_Line_MasterMetricList[metricInc]));
                    }
                    tmpMetricList.Add(tmpValueList);
                }
                Trends_Line_MasterDataList_Monthly.Add(tmpMetricList);
            }

            //get the roll up values
            Trends_Line_CalculateRollUpValues();
        }

        //get average or weighted average of each system
        private void Trends_Line_CalculateRollUpValues()
        {
            Trends_Line_MasterDataList_Daily_RollUp.Clear();
            Trends_Line_MasterDataList_Monthly_RollUp.Clear();
            Trends_Line_MasterDataList_Weekly_RollUp.Clear();

            //Daily
            for (int metricInc = 0; metricInc < Trends_Line_MasterMetricList.Count; metricInc++)
            {
                var avgList = new List<double>();
                for (int i = 0; i < Trends_Line_MasterDataList_Daily[0][metricInc].Count; i++)
                {
                    var rawValList = new List<double>();
                    for (int lineInc = 0; lineInc < Multi_CurrentLineNames.Count; lineInc++)
                    {
                        rawValList.Add(Trends_Line_MasterDataList_Daily[lineInc][metricInc][i]);
                    }
                    avgList.Add(rawValList.Average());
                }
                Trends_Line_MasterDataList_Daily_RollUp.Add(avgList);
            }

            //Weekly
            for (int metricInc = 0; metricInc < Trends_Line_MasterMetricList.Count; metricInc++)
            {
                var avgList = new List<double>();
                for (int i = 0; i < Trends_Line_MasterDataList_Weekly[0][metricInc].Count; i++)
                {
                    var rawValList = new List<double>();
                    for (int lineInc = 0; lineInc < Multi_CurrentLineNames.Count; lineInc++)
                    {
                        rawValList.Add(Trends_Line_MasterDataList_Weekly[lineInc][metricInc][i]);
                    }
                    avgList.Add(rawValList.Average());
                }
                Trends_Line_MasterDataList_Weekly_RollUp.Add(avgList);
            }

            //Monthly
            for (int metricInc = 0; metricInc < Trends_Line_MasterMetricList.Count; metricInc++)
            {
                var avgList = new List<double>();
                for (int i = 0; i < Trends_Line_MasterDataList_Monthly[0][metricInc].Count; i++)
                {
                    var rawValList = new List<double>();
                    for (int lineInc = 0; lineInc < Multi_CurrentLineNames.Count; lineInc++)
                    {
                        rawValList.Add(Trends_Line_MasterDataList_Monthly[lineInc][metricInc][i]);
                    }
                    avgList.Add(rawValList.Average());
                }
                Trends_Line_MasterDataList_Monthly_RollUp.Add(avgList);
            }
        }

        #endregion

        #region Failure Mode

        #region Variables
        internal List<string> Trends_Mode_Names_Unplanned = new List<string>();
        internal List<string> Trends_Mode_Names_Planned = new List<string>();

        internal List<DowntimeMetrics> Trends_Mode_MasterMetricList = new List<DowntimeMetrics> { DowntimeMetrics.DTpct, DowntimeMetrics.SPD, DowntimeMetrics.MTBF, DowntimeMetrics.Stops, DowntimeMetrics.MTTR, DowntimeMetrics.DT };

        //Line<Mode<Metric<Value>>>
        internal List<List<List<List<double>>>> Trends_Mode_MasterDataList_Daily_Unplanned = new List<List<List<List<double>>>>();
        internal List<List<List<List<double>>>> Trends_Mode_MasterDataList_Weekly_Unplanned = new List<List<List<List<double>>>>();
        internal List<List<List<List<double>>>> Trends_Mode_MasterDataList_Monthly_Unplanned = new List<List<List<List<double>>>>();

        internal List<List<List<List<double>>>> Trends_Mode_MasterDataList_Daily_Planned = new List<List<List<List<double>>>>();
        internal List<List<List<List<double>>>> Trends_Mode_MasterDataList_Weekly_Planned = new List<List<List<List<double>>>>();
        internal List<List<List<List<double>>>> Trends_Mode_MasterDataList_Monthly_Planned = new List<List<List<List<double>>>>();

        //Mode<Metric<Value>>
        internal List<List<List<double>>> Trends_Mode_MasterDataList_Daily_RollUp_Unplanned = new List<List<List<double>>>();
        internal List<List<List<double>>> Trends_Mode_MasterDataList_Weekly_RollUp_Unplanned = new List<List<List<double>>>();
        internal List<List<List<double>>> Trends_Mode_MasterDataList_Monthly_RollUp_Unplanned = new List<List<List<double>>>();

        internal List<List<List<double>>> Trends_Mode_MasterDataList_Daily_RollUp_Planned = new List<List<List<double>>>();
        internal List<List<List<double>>> Trends_Mode_MasterDataList_Weekly_RollUp_Planned = new List<List<List<double>>>();
        internal List<List<List<double>>> Trends_Mode_MasterDataList_Monthly_RollUp_Planned = new List<List<List<double>>>();

        #endregion

        private void Trends_Mode_SetLossNames()
        {
            Trends_Mode_Names_Unplanned.Clear();
            Trends_Mode_Names_Planned.Clear();

            //find our mode names
            for (int i = 0; i < Multi_CurrentLineNames.Count; i++)
            {
                int currentLineIndex = Multi_AllSystemReports_Names.IndexOf(Multi_CurrentLineNames[i]);
                //unplanned
                for (int j = 0; j < Multi_AllSystemReports[currentLineIndex].DT_Report.MappedDirectory.Count; j++)
                {
                    int tmpIndex = Trends_Mode_Names_Unplanned.IndexOf(Multi_AllSystemReports[currentLineIndex].DT_Report.MappedDirectory[j].Name);
                    if (tmpIndex == -1)
                    {
                        Trends_Mode_Names_Unplanned.Add(Multi_AllSystemReports[currentLineIndex].DT_Report.MappedDirectory[j].Name);
                    }
                }
                //planned
                for (int j = 0; j < Multi_AllSystemReports[currentLineIndex].DT_Report.MappedDirectory_Planned.Count; j++)
                {
                    int tmpIndex = Trends_Mode_Names_Planned.IndexOf(Multi_AllSystemReports[currentLineIndex].DT_Report.MappedDirectory_Planned[j].Name);
                    if (tmpIndex == -1)
                    {
                        Trends_Mode_Names_Planned.Add(Multi_AllSystemReports[currentLineIndex].DT_Report.MappedDirectory_Planned[j].Name);
                    }
                }
            }
        }

        private void initializeModeTrends()
        {
            //reset everything
            Trends_Mode_MasterDataList_Daily_Unplanned.Clear();
            Trends_Mode_MasterDataList_Weekly_Unplanned.Clear();
            Trends_Mode_MasterDataList_Monthly_Unplanned.Clear();

            Trends_Mode_MasterDataList_Daily_Planned.Clear();
            Trends_Mode_MasterDataList_Weekly_Planned.Clear();
            Trends_Mode_MasterDataList_Monthly_Planned.Clear();

            Trends_Mode_SetLossNames(); //set all loss names

            //generate master daily data list
            for (int lineInc = 0; lineInc < Multi_AllSystemReports_Daily.Count; lineInc++)
            {
                //unplanned
                var tmpModeList = new List<List<List<double>>>();
                for (int modeInc = 0; modeInc < Trends_Mode_Names_Unplanned.Count; modeInc++)
                {
                    var tmpMetricList = getDoubleForMetricsFromListOfSystemSummarys(Multi_AllSystemReports_Daily[lineInc], Trends_Mode_MasterMetricList, Trends_Mode_Names_Unplanned[modeInc], true);
                    tmpModeList.Add(tmpMetricList);
                }
                Trends_Mode_MasterDataList_Daily_Unplanned.Add(tmpModeList);

                //planned
                var tmpModeList2 = new List<List<List<double>>>();
                for (int modeInc = 0; modeInc < Trends_Mode_Names_Planned.Count; modeInc++)
                {
                    var tmpMetricList = getDoubleForMetricsFromListOfSystemSummarys(Multi_AllSystemReports_Daily[lineInc], Trends_Mode_MasterMetricList, Trends_Mode_Names_Planned[modeInc], false);
                    tmpModeList2.Add(tmpMetricList);
                }
                Trends_Mode_MasterDataList_Daily_Planned.Add(tmpModeList2);
            }

            //weekly
            for (int lineInc = 0; lineInc < Multi_AllSystemReports_Weekly.Count; lineInc++)
            {
                //unplanned
                var tmpModeList = new List<List<List<double>>>();
                for (int modeInc = 0; modeInc < Trends_Mode_Names_Unplanned.Count; modeInc++)
                {
                    var tmpMetricList = getDoubleForMetricsFromListOfSystemSummarys(Multi_AllSystemReports_Weekly[lineInc], Trends_Mode_MasterMetricList, Trends_Mode_Names_Unplanned[modeInc], true);
                    tmpModeList.Add(tmpMetricList);
                }
                Trends_Mode_MasterDataList_Weekly_Unplanned.Add(tmpModeList);

                //planned
                var tmpModeList2 = new List<List<List<double>>>();
                for (int modeInc = 0; modeInc < Trends_Mode_Names_Planned.Count; modeInc++)
                {
                    var tmpMetricList = getDoubleForMetricsFromListOfSystemSummarys(Multi_AllSystemReports_Weekly[lineInc], Trends_Mode_MasterMetricList, Trends_Mode_Names_Planned[modeInc], false);
                    tmpModeList2.Add(tmpMetricList);
                }
                Trends_Mode_MasterDataList_Weekly_Planned.Add(tmpModeList2);
            }
            //monthly
            for (int lineInc = 0; lineInc < Multi_AllSystemReports_Monthly.Count; lineInc++)
            {
                //unplanned
                var tmpModeList = new List<List<List<double>>>();
                for (int modeInc = 0; modeInc < Trends_Mode_Names_Unplanned.Count; modeInc++)
                {
                    var tmpMetricList = getDoubleForMetricsFromListOfSystemSummarys(Multi_AllSystemReports_Monthly[lineInc], Trends_Mode_MasterMetricList, Trends_Mode_Names_Unplanned[modeInc], true);
                    tmpModeList.Add(tmpMetricList);
                }
                Trends_Mode_MasterDataList_Monthly_Unplanned.Add(tmpModeList);

                //planned
                var tmpModeList2 = new List<List<List<double>>>();
                for (int modeInc = 0; modeInc < Trends_Mode_Names_Planned.Count; modeInc++)
                {
                    var tmpMetricList = getDoubleForMetricsFromListOfSystemSummarys(Multi_AllSystemReports_Monthly[lineInc], Trends_Mode_MasterMetricList, Trends_Mode_Names_Planned[modeInc], false);
                    tmpModeList2.Add(tmpMetricList);
                }
                Trends_Mode_MasterDataList_Monthly_Planned.Add(tmpModeList2);
            }

            //get the roll up values
            Trends_Mode_CalculateRollUpValues();
        }

        private void Trends_Mode_CalculateRollUpValues()
        {
            Trends_Mode_MasterDataList_Daily_RollUp_Unplanned.Clear();
            Trends_Mode_MasterDataList_Monthly_RollUp_Unplanned.Clear();
            Trends_Mode_MasterDataList_Weekly_RollUp_Unplanned.Clear();

            Trends_Mode_MasterDataList_Daily_RollUp_Planned.Clear();
            Trends_Mode_MasterDataList_Monthly_RollUp_Planned.Clear();
            Trends_Mode_MasterDataList_Weekly_RollUp_Planned.Clear();

            //UNPLANNED

            //Daily
            for (int modeInc = 0; modeInc < Trends_Mode_Names_Unplanned.Count; modeInc++)
            {
                var metricList = new List<List<double>>();
                for (int metricInc = 0; metricInc < Trends_Mode_MasterMetricList.Count; metricInc++)
                {
                    var avgList = new List<double>();
                    for (int i = 0; i < Trends_Mode_MasterDataList_Daily_Unplanned[0][modeInc][metricInc].Count; i++)
                    {
                        var rawValList = new List<double>();
                        for (int lineInc = 0; lineInc < Multi_CurrentLineNames.Count; lineInc++)
                        {
                            rawValList.Add(Trends_Mode_MasterDataList_Daily_Unplanned[lineInc][modeInc][metricInc][i]);
                        }
                        avgList.Add(rawValList.Average());
                    }
                    metricList.Add(avgList);
                }
                Trends_Mode_MasterDataList_Daily_RollUp_Unplanned.Add(metricList);
            }

            //Weekly
            for (int modeInc = 0; modeInc < Trends_Mode_Names_Unplanned.Count; modeInc++)
            {
                var metricList = new List<List<double>>();
                for (int metricInc = 0; metricInc < Trends_Mode_MasterMetricList.Count; metricInc++)
                {
                    var avgList = new List<double>();
                    for (int i = 0; i < Trends_Mode_MasterDataList_Weekly_Unplanned[0][modeInc][metricInc].Count; i++)
                    {
                        var rawValList = new List<double>();
                        for (int lineInc = 0; lineInc < Multi_CurrentLineNames.Count; lineInc++)
                        {
                            rawValList.Add(Trends_Mode_MasterDataList_Weekly_Unplanned[lineInc][modeInc][metricInc][i]);
                        }
                        avgList.Add(rawValList.Average());
                    }
                    metricList.Add(avgList);
                }
                Trends_Mode_MasterDataList_Weekly_RollUp_Unplanned.Add(metricList);
            }

            //Monthly
            for (int modeInc = 0; modeInc < Trends_Mode_Names_Unplanned.Count; modeInc++)
            {
                var metricList = new List<List<double>>();
                for (int metricInc = 0; metricInc < Trends_Mode_MasterMetricList.Count; metricInc++)
                {
                    var avgList = new List<double>();
                    for (int i = 0; i < Trends_Mode_MasterDataList_Monthly_Unplanned[0][modeInc][metricInc].Count; i++)
                    {
                        var rawValList = new List<double>();
                        for (int lineInc = 0; lineInc < Multi_CurrentLineNames.Count; lineInc++)
                        {
                            rawValList.Add(Trends_Mode_MasterDataList_Monthly_Unplanned[lineInc][modeInc][metricInc][i]);
                        }
                        avgList.Add(rawValList.Average());
                    }
                    metricList.Add(avgList);
                }
                Trends_Mode_MasterDataList_Monthly_RollUp_Unplanned.Add(metricList);
            }

            //PLANNED

            //Daily
            for (int modeInc = 0; modeInc < Trends_Mode_Names_Planned.Count; modeInc++)
            {
                var metricList = new List<List<double>>();
                for (int metricInc = 0; metricInc < Trends_Mode_MasterMetricList.Count; metricInc++)
                {
                    var avgList = new List<double>();
                    for (int i = 0; i < Trends_Mode_MasterDataList_Daily_Planned[0][modeInc][metricInc].Count; i++)
                    {
                        var rawValList = new List<double>();
                        for (int lineInc = 0; lineInc < Multi_CurrentLineNames.Count; lineInc++)
                        {
                            rawValList.Add(Trends_Mode_MasterDataList_Daily_Planned[lineInc][modeInc][metricInc][i]);
                        }
                        avgList.Add(rawValList.Average());
                    }
                    metricList.Add(avgList);
                }
                Trends_Mode_MasterDataList_Daily_RollUp_Planned.Add(metricList);
            }

            //Weekly
            for (int modeInc = 0; modeInc < Trends_Mode_Names_Planned.Count; modeInc++)
            {
                var metricList = new List<List<double>>();
                for (int metricInc = 0; metricInc < Trends_Mode_MasterMetricList.Count; metricInc++)
                {
                    var avgList = new List<double>();
                    for (int i = 0; i < Trends_Mode_MasterDataList_Weekly_Planned[0][modeInc][metricInc].Count; i++)
                    {
                        var rawValList = new List<double>();
                        for (int lineInc = 0; lineInc < Multi_CurrentLineNames.Count; lineInc++)
                        {
                            rawValList.Add(Trends_Mode_MasterDataList_Weekly_Planned[lineInc][modeInc][metricInc][i]);
                        }
                        avgList.Add(rawValList.Average());
                    }
                    metricList.Add(avgList);
                }
                Trends_Mode_MasterDataList_Weekly_RollUp_Planned.Add(metricList);
            }

            //Monthly
            for (int modeInc = 0; modeInc < Trends_Mode_Names_Planned.Count; modeInc++)
            {
                var metricList = new List<List<double>>();
                for (int metricInc = 0; metricInc < Trends_Mode_MasterMetricList.Count; metricInc++)
                {
                    var avgList = new List<double>();
                    for (int i = 0; i < Trends_Mode_MasterDataList_Monthly_Planned[0][modeInc][metricInc].Count; i++)
                    {
                        var rawValList = new List<double>();
                        for (int lineInc = 0; lineInc < Multi_CurrentLineNames.Count; lineInc++)
                        {
                            rawValList.Add(Trends_Mode_MasterDataList_Monthly_Planned[lineInc][modeInc][metricInc][i]);
                        }
                        avgList.Add(rawValList.Average());
                    }
                    metricList.Add(avgList);
                }
                Trends_Mode_MasterDataList_Monthly_RollUp_Planned.Add(metricList);
            }

        }

        #region Mapping

        internal DowntimeField Trends_Mode_MappingA = DowntimeField.Tier2;
        internal DowntimeField Trends_Mode_MappingB = DowntimeField.NA;

        internal void Trends_Mode_Remap(DowntimeField MappingA, DowntimeField MappingB)
        {
            if ((MappingA != Trends_Mode_MappingA) || (MappingB != Trends_Mode_MappingB))
            {
                Trends_Mode_MappingA = MappingA;
                Trends_Mode_MappingB = MappingB;
                Multi_RemapSystemReports(MappingA, MappingB);
                initializeModeTrends();
            }
        }

        #endregion

        #endregion

        #region Step Change

        #region Variables

        //Line<Mode<Metric<Value>>>
        internal List<List<List<List<double>>>> Trends_Step_MasterDataList_Daily_Unplanned = new List<List<List<List<double>>>>();
        internal List<List<List<List<double>>>> Trends_Step_MasterDataList_Weekly_Unplanned = new List<List<List<List<double>>>>();
        internal List<List<List<List<double>>>> Trends_Step_MasterDataList_Monthly_Unplanned = new List<List<List<List<double>>>>();

        internal List<List<List<List<double>>>> Trends_Step_MasterDataList_Daily_Planned = new List<List<List<List<double>>>>();
        internal List<List<List<List<double>>>> Trends_Step_MasterDataList_Weekly_Planned = new List<List<List<List<double>>>>();
        internal List<List<List<List<double>>>> Trends_Step_MasterDataList_Monthly_Planned = new List<List<List<List<double>>>>();

        //Mode<Metric<Value>>
        internal List<List<List<double>>> Trends_Step_MasterDataList_Daily_RollUp_Unplanned = new List<List<List<double>>>();
        internal List<List<List<double>>> Trends_Step_MasterDataList_Weekly_RollUp_Unplanned = new List<List<List<double>>>();
        internal List<List<List<double>>> Trends_Step_MasterDataList_Monthly_RollUp_Unplanned = new List<List<List<double>>>();

        internal List<List<List<double>>> Trends_Step_MasterDataList_Daily_RollUp_Planned = new List<List<List<double>>>();
        internal List<List<List<double>>> Trends_Step_MasterDataList_Weekly_RollUp_Planned = new List<List<List<double>>>();
        internal List<List<List<double>>> Trends_Step_MasterDataList_Monthly_RollUp_Planned = new List<List<List<double>>>();

        //LINE
        //Line<Metric<Value>>
        internal List<List<List<double>>> Trends_Step_MasterDataList_Daily = new List<List<List<double>>>();
        internal List<List<List<double>>> Trends_Step_MasterDataList_Weekly = new List<List<List<double>>>();
        internal List<List<List<double>>> Trends_Step_MasterDataList_Monthly = new List<List<List<double>>>();

        //Metric<Value>
        internal List<List<double>> Trends_Step_MasterDataList_Daily_RollUp = new List<List<double>>();
        internal List<List<double>> Trends_Step_MasterDataList_Weekly_RollUp = new List<List<double>>();
        internal List<List<double>> Trends_Step_MasterDataList_Monthly_RollUp = new List<List<double>>();

        #endregion

        private void initializeStepTrends()
        {
            //mode trends
            Trends_Step_MasterDataList_Daily_Unplanned = StepChangeAnalysis.getStepChangeForSeries(Trends_Mode_MasterDataList_Daily_Unplanned);
            Trends_Step_MasterDataList_Weekly_Unplanned = StepChangeAnalysis.getStepChangeForSeries(Trends_Mode_MasterDataList_Weekly_Unplanned);
            //  Trends_Step_MasterDataList_Monthly_Unplanned = StepChangeAnalysis.getStepChangeForSeries(Trends_Mode_MasterDataList_Monthly_Unplanned);

            Trends_Step_MasterDataList_Daily_Planned = StepChangeAnalysis.getStepChangeForSeries(Trends_Mode_MasterDataList_Daily_Planned);
            Trends_Step_MasterDataList_Weekly_Planned = StepChangeAnalysis.getStepChangeForSeries(Trends_Mode_MasterDataList_Weekly_Planned);
            //   Trends_Step_MasterDataList_Monthly_Planned = StepChangeAnalysis.getStepChangeForSeries(Trends_Mode_MasterDataList_Monthly_Planned);

            Trends_Step_MasterDataList_Daily_RollUp_Unplanned = StepChangeAnalysis.getStepChangeForSeries(Trends_Mode_MasterDataList_Daily_RollUp_Unplanned);
            Trends_Step_MasterDataList_Weekly_RollUp_Unplanned = StepChangeAnalysis.getStepChangeForSeries(Trends_Mode_MasterDataList_Weekly_RollUp_Unplanned);
            //   Trends_Step_MasterDataList_Monthly_RollUp_Unplanned = StepChangeAnalysis.getStepChangeForSeries(Trends_Mode_MasterDataList_Monthly_RollUp_Unplanned);

            Trends_Step_MasterDataList_Daily_RollUp_Planned = StepChangeAnalysis.getStepChangeForSeries(Trends_Mode_MasterDataList_Daily_RollUp_Planned);
            Trends_Step_MasterDataList_Weekly_RollUp_Planned = StepChangeAnalysis.getStepChangeForSeries(Trends_Mode_MasterDataList_Weekly_RollUp_Planned);
            //  Trends_Step_MasterDataList_Monthly_RollUp_Planned = StepChangeAnalysis.getStepChangeForSeries(Trends_Mode_MasterDataList_Monthly_RollUp_Planned);

            //line trends
            Trends_Step_MasterDataList_Daily = StepChangeAnalysis.getStepChangeForSeries(Trends_Line_MasterDataList_Daily);
            Trends_Step_MasterDataList_Weekly = StepChangeAnalysis.getStepChangeForSeries(Trends_Line_MasterDataList_Weekly);
            //  Trends_Step_MasterDataList_Monthly = StepChangeAnalysis.getStepChangeForSeries(Trends_Line_MasterDataList_Monthly);

            Trends_Step_MasterDataList_Daily_RollUp = StepChangeAnalysis.getStepChangeForSeries(Trends_Line_MasterDataList_Daily_RollUp);
            Trends_Step_MasterDataList_Weekly_RollUp = StepChangeAnalysis.getStepChangeForSeries(Trends_Line_MasterDataList_Weekly_RollUp);
            //  Trends_Step_MasterDataList_Monthly_RollUp = StepChangeAnalysis.getStepChangeForSeries(Trends_Line_MasterDataList_Monthly_RollUp);
        }

        #region Mapping

        //   internal DowntimeField Trends_Mode_MappingA = DowntimeField.Tier2;
        //   internal DowntimeField Trends_Mode_MappingB = DowntimeField.NA;

        internal void Trends_Step_Remap(DowntimeField MappingA, DowntimeField MappingB)
        {
            if ((MappingA != Trends_Mode_MappingA) || (MappingB != Trends_Mode_MappingB))
            {
                Trends_Mode_MappingA = MappingA;
                Trends_Mode_MappingB = MappingB;
                Multi_RemapSystemReports(MappingA, MappingB);
                initializeModeTrends();
                initializeStepTrends();
            }
        }

        #endregion

        #endregion

        #region GlidePath
        internal List<double> Trends_GlidePath_CurrentGlidePath;
        private void Trends_GlidePath_UpdateCurrentFromActive()
        {
            /*  List<Tuple<double, string>> rawSeries = Trends_GlidePath_ActiveChangelog.getGlidepath(Trends_Mode_getDates(1));
              List<double> GlideSeries = new List<double>();
              List<double> actSeries = Trends_Line_MasterDataList_Daily[0][0];//<- TEMP!!! getDoubleForMetricFromDailyReports(DowntimeMetrics.OEE);
              for (int i = 0; i < rawSeries.Count; i++)
              {
                  if (rawSeries[i].Item1 < 0) { GlideSeries.Add(actSeries[i]); }
                  else { GlideSeries.Add(rawSeries[i].Item1 * 100); }
              }

              Trends_GlidePath_CurrentGlidePath = GlideSeries; */
        }

        public void Trends_Glidepath_TurnOn()
        { //bug where if you do glidepath then make change it wont show up?
            if (object.ReferenceEquals(null, Trends_GlidePath_ActiveChangelog)) { Trends_GlidePath_ActiveChangelog = CrystalBall_Changelog; }
            Trends_GlidePath_UpdateCurrentFromActive();
            Trends_IsGlidePathOn = true;
        }

        public void Trends_Glidepath_SetCurrentSimAsActive()
        {
            Trends_GlidePath_ActiveChangelog = CrystalBall_Changelog;
            Trends_GlidePath_UpdateCurrentFromActive();
        }

        public void Trends_Glidepath_TurnOff()
        {
            Trends_IsGlidePathOn = false;
        }

        public void Trends_Glidepath_SetCurrent(string FileName)
        {
            Trends_GlidePath_ActiveChangelog = CrystalBall_Changelog_Import(FileName);
            Trends_GlidePath_UpdateCurrentFromActive();
            //if its create new, prompt to save old one and reset current simulation

        }
        public void Trends_Glidepath_SaveAsCurrent(string FileName)
        {
            CrystalBall_Changelog_Export(Trends_GlidePath_ActiveChangelog, FileName);
            Trends_GlidePath_UpdateCurrentFromActive();
        }
        #endregion

        #endregion

    }
}
