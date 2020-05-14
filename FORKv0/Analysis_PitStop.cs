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

        #region Pit Stop
        public void PitStop_initialize()
        {
            PitStop_RawReport = rawData.getSubset(this.startTime, this.endTime);
            PitStop_RawReport.reMapDowntime(PitStop_SU_Mapping_A, PitStop_SU_Mapping_B);
            PitStop_RunTime_AnalysisData = rawData.getSubset(this.startTime, this.endTime);
            PitStop_RunTime_AnalysisData.reMapDowntime(PitStop_RT_Mapping_A, PitStop_RT_Mapping_B);
            PitStop_SU_initialize();
            PitStop_RT_initialize();
        }

        #region Start Up
        private SystemSummaryReport PitStop_RawReport;
        #region Intermediate Sheet
        public double PitStop_SU_OverallMaxScore { get; set; }
        public List<double> PitStop_SU_CheckeredFlagPositions { get; set; }
        public List<double> PitStop_SU_YellowFlagPositions { get; set; }
        public List<string> PitStop_SU_LossNames { get; set; }
        public List<List<double>> PitStop_SU_LossScores { get; set; }

        // Item1: event start time, Item2: minutes before first stop, Item3: # stops in x min, Item4: OEE, Item 5: Event Duration
        public List<List<Tuple<DateTime, double, double, double, double>>> PitStop_SU_CarInfo { get; set; }


        public List<Tuple<double, double, double, double>> PitStop_SU_YellowFlagInfo { get; set; }
        public List<Tuple<double, double, double, double>> PitStop_SU_CheckeredFlagInfo { get; set; }

        #endregion
        private void PitStop_SU_initialize()
        {
            PitStop_SU_CheckeredFlagPositions = new List<double>();
            PitStop_SU_YellowFlagPositions = new List<double>();
            PitStop_SU_LossNames = new List<string>();
            PitStop_SU_LossScores = new List<List<double>>();
            PitStop_SU_CarInfo = new List<List<Tuple<DateTime, double, double, double, double>>>();
            PitStop_SU_YellowFlagInfo = new List<Tuple<double, double, double, double>>();
            PitStop_SU_CheckeredFlagInfo = new List<Tuple<double, double, double, double>>();
            PitStop_SU_OverallMaxScore = 0;
            PitStop_SU_setUpFromRawReport();
        }
        private void PitStop_SU_setUpFromRawReport()
        {
            // First, figure out what losses we are looking for
            PitStop_SU_LossNames.Clear();
            PitStop_SU_OverallMaxScore = 0;
            PitStop_SU_LossScores.Clear();
            PitStop_SU_CheckeredFlagPositions.Clear();
            PitStop_SU_YellowFlagPositions.Clear();
            PitStop_SU_CarInfo.Clear();
            PitStop_SU_YellowFlagInfo.Clear();
            PitStop_SU_CheckeredFlagInfo.Clear();
            for (int i = 0; i < PitStop_RawReport.DT_Report.MappedDirectory_Planned.Count; i++)
            {
                PitStop_SU_LossNames.Add(PitStop_RawReport.DT_Report.MappedDirectory_Planned[i].Name);
            }
            //find the scores! -- THIS IS REDUNDANT BUT GOOD FOR DEBUGGING
            for (int i = 0; i < PitStop_RawReport.DT_Report.MappedDirectory_Planned.Count; i++)
            {
                PitStop_SU_setScoreForMode(PitStop_SU_LossNames[i], (int)PitStop_RawReport.DT_Report.MappedDirectory_Planned[i].Stops);
            }
            PitStop_SU_sortActiveLists();
        }
        private void PitStop_SU_sortActiveLists()
        {
            var gapList = new List<Tuple<double, int>>();
            double tmpSum;
            for (int i = 0; i < PitStop_SU_CheckeredFlagPositions.Count; i++)
            { //negative is bad, positive is good
                tmpSum = PitStop_SU_CheckeredFlagPositions[i] - PitStop_SU_YellowFlagPositions[i];
                gapList.Add(new Tuple<double, int>(tmpSum, i));
            }
            List<Tuple<double, int>> result = gapList.OrderBy(x => x.Item1).ToList();

            //below is ugly/long but need to reorder everything
            var tmpNames = new List<string>();
            var tmpYellow = new List<double>();
            var tmpCheckered = new List<double>();
            var tmpScores = new List<List<double>>();
            var tmpCarInfo = new List<List<Tuple<DateTime, double, double, double, double>>>();

            int j;

            for (int i = 0; i < result.Count; i++)
            {
                j = result[result.Count - i - 1].Item2;
                tmpNames.Add(PitStop_SU_LossNames[j]);
                tmpYellow.Add(PitStop_SU_YellowFlagPositions[j]);
                tmpCheckered.Add(PitStop_SU_CheckeredFlagPositions[j]);
                tmpScores.Add(PitStop_SU_LossScores[j]);
                tmpCarInfo.Add(PitStop_SU_CarInfo[j]);
            }

            PitStop_SU_LossNames = tmpNames;
            PitStop_SU_YellowFlagPositions = tmpYellow;
            PitStop_SU_CheckeredFlagPositions = tmpCheckered;
            PitStop_SU_LossScores = tmpScores;
            PitStop_SU_CarInfo = tmpCarInfo;
        }


        private void PitStop_SU_setScoreForMode(string LossName, int analysisInstances)
        {
            var DataEngine = new PitStopStartupAnalysis();
            var tmpScores = new List<double>();
            var tmpCardInfo = new List<Tuple<DateTime, double, double, double, double>>();
            var periodScores = new List<double>();
            SystemSummaryReport tmpReport;
            DateTime tmpStartTime;
            DTevent tmpEvent;
            for (int i = 0; i < rawData.DT_Report.rawDTdata.rawConstraintData.Count; i++)
            {
                tmpEvent = rawData.DT_Report.rawDTdata.rawConstraintData[i];
                if (tmpEvent.isPlanned && tmpEvent.getFieldFromInteger(PitStop_SU_Mapping_A, PitStop_SU_Mapping_B) == LossName /*we find the target event*/)
                {
                    tmpStartTime = tmpEvent.startTime;
                    tmpReport = rawData.getSubset(tmpStartTime, tmpStartTime.AddMinutes(PITSTOP_SU_ANALYSISPERIOD_MIN));
                    tmpScores.Add(DataEngine.getScoreForPeriod(tmpReport, PITSTOP_SU_ANALYSISPERIOD_MIN));
                    tmpCardInfo.Add(new Tuple<DateTime, double, double, double, double>(tmpStartTime, tmpReport.OEE, tmpReport.Stops, DataEngine.KPI_D_raw, tmpEvent.DT));
                }
            }
            periodScores = tmpScores.GetRange(tmpScores.Count - analysisInstances, analysisInstances);
            PitStop_SU_LossScores.Add(periodScores);
            PitStop_SU_YellowFlagPositions.Add(periodScores.Average());
            PitStop_SU_OverallMaxScore = Math.Max(PitStop_SU_OverallMaxScore, periodScores.Max());
            PitStop_SU_CheckeredFlagPositions.Add(tmpScores.Average());

            //set car & flag info
            PitStop_SU_CarInfo.Add(tmpCardInfo.GetRange(tmpCardInfo.Count - analysisInstances, analysisInstances));

            var tmpItem1 = new List<double>();
            var tmpItem2 = new List<double>();
            var tmpItem3 = new List<double>();
            var tmpItem4 = new List<double>();
            int ix = PitStop_SU_CarInfo.Count - 1;
            for (int j = 0; j < PitStop_SU_CarInfo[ix].Count; j++)
            {
                tmpItem1.Add(PitStop_SU_CarInfo[ix][j].Item2);
                tmpItem2.Add(PitStop_SU_CarInfo[ix][j].Item3);
                tmpItem3.Add(PitStop_SU_CarInfo[ix][j].Item4);
                tmpItem4.Add(PitStop_SU_CarInfo[ix][j].Item5);
            }
            PitStop_SU_YellowFlagInfo.Add(new Tuple<double, double, double, double>(tmpItem1.Average(), tmpItem2.Average(), tmpItem3.Average(), tmpItem4.Average()));


            tmpItem1.Clear();
            tmpItem2.Clear();
            tmpItem3.Clear();
            tmpItem4.Clear();
            for (int j = 0; j < tmpCardInfo.Count; j++)
            {
                tmpItem1.Add(tmpCardInfo[j].Item2);
                tmpItem2.Add(tmpCardInfo[j].Item3);
                tmpItem3.Add(tmpCardInfo[j].Item4);
                tmpItem4.Add(tmpCardInfo[j].Item5);
            }
            PitStop_SU_CheckeredFlagInfo.Add(new Tuple<double, double, double, double>(tmpItem1.Average(), tmpItem2.Average(), tmpItem3.Average(), tmpItem4.Average()));

        }


        #region Mapping
        public string PitStop_SU_Mapping_A_String { get { return getStringForEnum(PitStop_SU_Mapping_A); } }
        public string PitStop_SU_Mapping_B_String { get { return getStringForEnum(PitStop_SU_Mapping_B); } }
        public DowntimeField PitStop_SU_Mapping_A { get; set; } = DowntimeField.Tier2;
        public DowntimeField PitStop_SU_Mapping_B { get; set; } = DowntimeField.NA;
        public string PitStop_SU_Mapping_A_string { get { return getStringForEnum(PitStop_SU_Mapping_A); } }
        public string PitStop_SU_Mapping_B_string { get { return getStringForEnum(PitStop_SU_Mapping_B); } }

        public void PitStop_SU_ReMap(string MappingA, string MappingB = "")
        {
            PitStop_SU_ReMap(getEnumForString(MappingA), getEnumForString(MappingB));
        }
        private void PitStop_SU_ReMap(DowntimeField MappingA, DowntimeField MappingB) //STUB
        {
            PitStop_SU_Mapping_A = MappingA;
            PitStop_SU_Mapping_B = MappingB;
            PitStop_RawReport.DT_Report.reMapDataSet(MappingA, MappingB);
            PitStop_SU_setUpFromRawReport();
        }
        #endregion

        #endregion

        #region Run Time
        private SystemSummaryReport PitStop_RunTime_AnalysisData;
        /* INTERMEDIATE SHEET VARS */
        public double[] PitStop_RT_LineCDF;
        public double[] PitStop_RT_Xaxis;
        public List<List<double>> PitStop_RT_ModeCDF = new List<List<double>>();
        public List<string> PitStop_RT_ModeNames;
        private double[,] PitStop_RT_ModeCDF_Raw;
        private List<string> PitStop_RT_ModeNames_Raw;
        /* *** *** *** *** *** *** */
        public string PitStop_RT_SYSTEMNAME = "System";
        private SurvivalAnalysis PitStop_RT_Engine;
        private void PitStop_RT_initialize()
        {
            PitStop_RT_execute();
        }
        private void PitStop_RT_execute()
        {
            var EventData = new List<Tuple<string, double>>();
            for (int i = 0; i < rawData.rawData.Count; i++)
            {
                EventData.Add(new Tuple<string, double>(rawData.rawData[i].getFieldFromInteger(PitStop_RT_Mapping_A, PitStop_RT_Mapping_B), rawData.rawData[i].UT));
            }
            PitStop_RT_Engine = new SurvivalAnalysis(EventData);
            PitStop_RT_LineCDF = PitStop_RT_Engine.netCDF;
            PitStop_RT_Xaxis = PitStop_RT_Engine.uptimeGroupList;
            PitStop_RT_ModeCDF_Raw = PitStop_RT_Engine.survivaltable;
            PitStop_RT_ModeNames_Raw = PitStop_RT_Engine.selectedfailuremodeList;

            PitStop_RT_CleanUpData();
        }

        private void PitStop_RT_CleanUpData()
        {
            PitStop_RT_ModeNames = new List<string>();
            PitStop_RT_ModeNames.Add(PitStop_RT_SYSTEMNAME);

            for (int i = 0; i < PitStop_RT_ModeNames_Raw.Count; i++)
            {
                if (PitStop_RT_ModeCDF_Raw[i, 0] > 0.9)
                {
                    var tmpDoubleList = new List<double>();
                    PitStop_RT_ModeNames.Add(PitStop_RT_ModeNames_Raw[i]);
                    int x = PitStop_RT_ModeCDF_Raw.GetLength(1);
                    for (int j = 0; j < x; j++)
                    {
                        tmpDoubleList.Add(PitStop_RT_ModeCDF_Raw[i, j]);
                    }
                    PitStop_RT_ModeCDF.Add(tmpDoubleList);
                }
            }
        }


        #region Mapping
        public DowntimeField PitStop_RT_Mapping_A { get; set; } = DowntimeField.Tier2;
        public DowntimeField PitStop_RT_Mapping_B { get; set; } = DowntimeField.NA;

        internal void PitStop_RT_ReMap(DowntimeField MappingA, DowntimeField MappingB) //STUB
        {
            PitStop_RT_Mapping_A = MappingA;
            PitStop_RT_Mapping_B = MappingB;
            PitStop_RT_execute();
        }
        #endregion

        #endregion

        #region Wear Out
        public DowntimeField PitStop_WO_Mapping_A { get; set; } = DowntimeField.Tier2;
        public DowntimeField PitStop_WO_Mapping_B { get; set; } = DowntimeField.NA;
        public void PitStop_WO_ReMap(string MappingA, string MappingB = "")
        {
            PitStop_WO_ReMap(getEnumForString(MappingA), getEnumForString(MappingB));
        }
        private void PitStop_WO_ReMap(DowntimeField MappingA, DowntimeField MappingB) //STUB
        {
            PitStop_WO_Mapping_A = MappingA;
            PitStop_WO_Mapping_B = MappingB;
        }

        public List<string> PitStop_WO_LossNames { get; set; }
        public List<List<double>> PitStop_WO_ChartValues { get; set; }
        #endregion
        #endregion

    }
}
