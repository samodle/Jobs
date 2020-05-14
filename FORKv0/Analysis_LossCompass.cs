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

        #region Loss Compass

        #region Loss Compass UI Variables
        public List<CrystalBallSimulation> LossCompass_CrystalBall_Changelog { get { return CrystalBall_Changelog.Changeset; } }
        public string LossCompass_CrystalBall_GetCardNameForSelectedSimulation(CrystalBallSimulation SelectedSim)
        {
            int i = SelectedSim.getCurrentLevel(TierA_Level);
            switch (i)
            {
                case 0:
                    return "A";
                case 1:
                    return "B";
                default:
                    return "C";
            }
        }
        public int LossCompass_CrystalBall_GetPositionForSelectedSimulation(CrystalBallSimulation SelectedSim, CardTier SelectedTier)
        {
            switch (SelectedTier)
            {
                case CardTier.A:
                    return TierA_Current.IndexOf(new DTeventSummary(SelectedSim.Name));
                case CardTier.B:
                    return TierB_Current.IndexOf(new DTeventSummary(SelectedSim.Name));
                //case CardTier.A:
                default:
                    return TierC_Current.IndexOf(new DTeventSummary(SelectedSim.Name));
            }
            // return 0;
        }
        public DateTime LossCompass_CrystalBall_GetDueDate(CardTier CardName, string LossName)
        {
            switch (CardName)
            {
                case CardTier.A:
                    return LossCompass_CrystalBall_GetDueDate_Helper(TierA_Level, LossName);
                case CardTier.B:
                    return LossCompass_CrystalBall_GetDueDate_Helper(TierB_Level, LossName);
                default: //"C"
                    return LossCompass_CrystalBall_GetDueDate_Helper(TierC_Level, LossName);
            }
            //  return LossCompass_CrystalBall_GetDueDate(SelectedSim.getCurrentLevel(TierA_Level), LossName);
        }
        private DateTime LossCompass_CrystalBall_GetDueDate_Helper(int targetLevel, string LossName) //STUB
        {
            for (int i = 0; i < LossCompass_CrystalBall_Changelog.Count; i++)
            {
                if (targetLevel == LossCompass_CrystalBall_Changelog[i].getCurrentLevel(TierA_Level) && LossName == LossCompass_CrystalBall_Changelog[i].Name)
                {
                    return LossCompass_CrystalBall_Changelog[i].DueDate;
                }
            }
            return new DateTime(0);
        }

        //Bottom Line - for the net OEE bar at the bottom
        public double LossCompass_HeaderKPI_2 { get; set; }
        public double LossCompass_HeaderKPI_3 { get; set; }
        public double LossCompass_HeaderKPI_2_Sim { get; set; }
        public double LossCompass_HeaderKPI_3_Sim { get; set; }

        public double LossCompass_OEE { get; set; }
        public double LossCompass_UPDT { get; set; }
        public double LossCompass_PDT { get; set; }
        public double LossCompass_OEE_Sim { get; set; }
        public double LossCompass_UPDT_Sim { get; set; }
        public double LossCompass_PDT_Sim { get; set; }
        public double LossCompass_RateLoss { get; set; }
        public double LossCompass_ScrapLoss { get; set; }
        public List<double> LossCompass_TopLineResults_Values { get; set; } = new List<double>();
        //public List<double> LossCompass_TopLineResults_Values_Sim { get; set; } = new List<double>();
        public List<string> LossCompass_TopLineResults_Names { get; set; } = new List<string>();// = { "UPDT", "PDT", "Rate Loss", "OEE" };
        //Criteria 1 KPIs - these are displayed along the right edge
        public double LossCompass_KPI1 { get; set; }
        public double LossCompass_KPI2 { get; set; }
        public double LossCompass_KPI3 { get; set; }
        public double LossCompass_KPI4 { get; set; }
        public double LossCompass_KPI5 { get; set; }
        //Content for Card Headers
        public string TierA_Header { get { return (TierA_Level == 0) ? LossCompass_DefaultHeader : LossCompass_ActiveNames[TierA_Level - 1]; } }
        public string TierB_Header { get { return LossCompass_ActiveNames[TierA_Level]; } }
        public string TierC_Header { get { return LossCompass_ActiveNames[TierB_Level]; } }
        //Lists - Tiers A, B & C
        public string[] TierA_Names = new string[TierA_MAXBARS] { "", "", "", "", "", "" }; //downtime field name
        public string[] TierB_Names = new string[TierB_MAXBARS] { "", "", "", "", "", "" };
        public string[] TierC_Names = new string[TierC_MAXBARS] { "", "", "", "", "", "", "", "" };

        public double[] TierA_Values = new double[TierA_MAXBARS] { 0, 0, 0, 0, 0, 0 };      //primary KPI values
        public double[] TierB_Values = new double[TierB_MAXBARS] { 0, 0, 0, 0, 0, 0 };
        public double[] TierC_Values = new double[TierC_MAXBARS] { 0, 0, 0, 0, 0, 0, 0, 0 };

        public double[] TierA_Values_Sim = new double[TierA_MAXBARS] { 0, 0, 0, 0, 0, 0 };
        public double[] TierB_Values_Sim = new double[TierB_MAXBARS] { 0, 0, 0, 0, 0, 0 };
        public double[] TierC_Values_Sim = new double[TierC_MAXBARS] { 0, 0, 0, 0, 0, 0, 0, 0 };

        public double[] TierA_Values_2 = new double[TierA_MAXBARS] { 0, 0, 0, 0, 0, 0 }; //secondary KPI values
        public double[] TierB_Values_2 = new double[TierB_MAXBARS] { 0, 0, 0, 0, 0, 0 };
        public double[] TierC_Values_2 = new double[TierC_MAXBARS] { 0, 0, 0, 0, 0, 0, 0, 0 };

        public double[] TierA_Values_2_Sim = new double[TierA_MAXBARS] { 0, 0, 0, 0, 0, 0 };
        public double[] TierB_Values_2_Sim = new double[TierB_MAXBARS] { 0, 0, 0, 0, 0, 0 };
        public double[] TierC_Values_2_Sim = new double[TierC_MAXBARS] { 0, 0, 0, 0, 0, 0, 0, 0 };

        public double[] TierA_Values_Color = new double[TierA_MAXBARS] { 0, 0, 0, 0, 0, 0 };      //0 = default, 1-4 are prioritization colors
        public double[] TierB_Values_Color = new double[TierB_MAXBARS] { 0, 0, 0, 0, 0, 0 };       //1 is good/lowest and 4 is worst/highest
        public double[] TierC_Values_Color = new double[TierC_MAXBARS] { 0, 0, 0, 0, 0, 0, 0, 0 };

        public double TierA_Max { get { return !LossCompass_isSimulationMode ? TierA_Values.Max() : Math.Max(TierA_Values.Max(), TierA_Values_Sim.Max()); } }
        public double TierB_Max { get { return !LossCompass_isSimulationMode ? TierB_Values.Max() : Math.Max(TierB_Values.Max(), TierB_Values_Sim.Max()); } }
        public double TierC_Max { get { return !LossCompass_isSimulationMode ? TierC_Values.Max() : Math.Max(TierC_Values.Max(), TierC_Values_Sim.Max()); } }
        public double TierA_Max_2 { get { return !LossCompass_isSimulationMode ? TierA_Values_2.Max() : Math.Max(TierA_Values_2.Max(), TierA_Values_2_Sim.Max()); } }
        public double TierB_Max_2 { get { return !LossCompass_isSimulationMode ? TierB_Values_2.Max() : Math.Max(TierB_Values_2.Max(), TierB_Values_2_Sim.Max()); } }
        public double TierC_Max_2 { get { return !LossCompass_isSimulationMode ? TierC_Values_2.Max() : Math.Max(TierC_Values_2.Max(), TierC_Values_2_Sim.Max()); } }
        public double LossCompass_Tiers_NetMax { get { return Math.Max(TierA_Max, Math.Max(TierB_Max, TierC_Max)); } }
        public double LossCompass_Tiers_NetMax_2 { get { return Math.Max(TierA_Max_2, Math.Max(TierB_Max_2, TierC_Max_2)); } }

        public int TierA_NumberOfItems { get { return TierA_Master.Count(); } } //number of failure modes in each tier
        public int TierB_NumberOfItems { get { return TierB_Master.Count(); } }
        public int TierC_NumberOfItems { get { return TierC_Master.Count(); } }

        public int LossCompass_TierA_Level { get; set; } = 1;
        private int TierA_Level { get { return LossCompass_TierA_Level - 1; } }
        private int TierB_Level { get { return TierA_Level + 1; } }
        private int TierC_Level { get { return TierB_Level + 1; } }
        private int LossCompass_TierA_LevelVisible_Prior { get; set; } = 0;

        /* Spark Chart */
        public List<double> LossCompass_SparkData_Values { get; set; } = new List<double>();

        //launchmodekpigrid -- modekpiselected -- barclicked
        public void LossCompass_SparkData_Update(string LossName, CardTier Card, DowntimeMetrics Metric = DowntimeMetrics.UPDTpct, int timePeriodInDays = 1, int periodsToDisplay = 10)
        {
            LossCompass_SparkData_Values.Clear();
            var tmpList = new List<string>();
            var tmpData = new List<double>();
            tmpList.Add(LossName);
            var tmpList2 = new List<DowntimeMetrics>();
            tmpList2.Add(Metric);
            //first, make trends is mapped right
            /*   switch (Card)
               {
                   case CardTier.A:
                       Trends_MappingA = TierA_Mapping_A;
                       Trends_MappingB = TierA_Mapping_B;
                       break;
                   case CardTier.B:
                       Trends_MappingA = TierB_Mapping_A;
                       Trends_MappingB = TierB_Mapping_B;
                       break;
                   case CardTier.C:
                       Trends_MappingA = TierC_Mapping_A;
                       Trends_MappingB = TierC_Mapping_B;
                       break;
               } */
            // Trends_Mode_UpdateMappedList();
            //now get the actual raw data
            //   Trends_Mode_generateKPIchart_Unplanned(timePeriodInDays, tmpList2, tmpList); //THIS IS ACTUALLY A PROBLEM
            //    tmpData = Trends_Mode_ChartValues[0][0];
            int j;
            for (int i = 0; i < periodsToDisplay; i++)
            {
                j = periodsToDisplay - i;
                LossCompass_SparkData_Values.Add(tmpData[tmpData.Count - j]);
            }
        }
        #endregion

        #region Loss Compass
        /* Loss Compass Master Drilldown Data */
        private List<List<DTeventSummary>> LossCompass_MasterEventList = new List<List<DTeventSummary>>();
        private List<DowntimeField> LossCompass_MasterMappingAList = new List<DowntimeField>();
        private List<DowntimeField> LossCompass_MasterMappingBList = new List<DowntimeField>();
        private List<string> LossCompass_ActiveNames = new List<string>();
        private int LossCompass_NumActiveLevels { get { return LossCompass_MasterEventList.Count; } }
        private bool LossCompass_isSimulationMode { get; set; } = false;
        private CrystalBallAnalysis CrystalBall_Changelog { get; set; }
        /* Loss Compass Display Data */
        private List<DTeventSummary> TierA_Master = new List<DTeventSummary>();
        private List<DTeventSummary> TierB_Master = new List<DTeventSummary>();
        private List<DTeventSummary> TierC_Master = new List<DTeventSummary>();

        private List<DTeventSummary> TierA_Current = new List<DTeventSummary>();
        private List<DTeventSummary> TierB_Current = new List<DTeventSummary>();
        private List<DTeventSummary> TierC_Current = new List<DTeventSummary>();


        private DowntimeField TierA_Mapping_A { get { return LossCompass_MasterMappingAList[TierA_Level]; } }
        private DowntimeField TierB_Mapping_A { get { return LossCompass_MasterMappingAList[TierB_Level]; } }
        private DowntimeField TierC_Mapping_A { get { return (TierC_Level >= LossCompass_MasterMappingAList.Count) ? DowntimeField.NA : LossCompass_MasterMappingAList[TierC_Level]; } }
        private DowntimeField TierA_Mapping_B { get { return LossCompass_MasterMappingBList[TierA_Level]; } }
        private DowntimeField TierB_Mapping_B { get { return LossCompass_MasterMappingBList[TierB_Level]; } }
        private DowntimeField TierC_Mapping_B { get { return LossCompass_MasterMappingBList[TierC_Level]; } }

        private const DowntimeField TierA_Mapping_Default = DowntimeField.Tier1;
        private const DowntimeField TierB_Mapping_Default = DowntimeField.Tier2;
        private const DowntimeField TierC_Mapping_Default = DowntimeField.Tier3;

        private KPIs current_Criteria1_KPI { get; set; } = KPIs.NA; //KPI selected in bar to the right
        private DowntimeMetrics current_Criteria1_Display { get; set; } = DowntimeMetrics.DTpct;
        private DowntimeMetrics current_Criteria2_KPI { get; set; } = DowntimeMetrics.NA; //secondary KPI selected
        private DowntimeMetrics SortKPI { get; set; } = DowntimeMetrics.DT;
        private bool isSortLocked { get; set; } = false;

        private TopLevelSelected TopLevelSelection { get; set; } = TopLevelSelected.Unplanned;
        private string LossCompass_DefaultHeader
        {
            get
            {
                switch (TopLevelSelection)
                {
                    case TopLevelSelected.Unplanned:
                        return "UPDT";
                    case TopLevelSelected.Planned:
                        return "PDT";
                    case TopLevelSelected.RateLoss:
                        return "Rate Loss";
                    default: return "oops...";
                }
            }
        }
        #endregion

        internal void initializeLossCompass()
        {
            TierA_Master.Clear(); //added for multiline

            //Bottom Line
            this.LossCompass_OEE = AnalysisPeriodData.OEE;
            this.LossCompass_PDT = AnalysisPeriodData.PDTpct;
            this.LossCompass_UPDT = AnalysisPeriodData.UPDTpct;
            this.LossCompass_RateLoss = AnalysisPeriodData.RateLossPct;

            this.LossCompass_HeaderKPI_2 = AnalysisPeriodData.SPD;
            this.LossCompass_HeaderKPI_3 = AnalysisPeriodData.ActualCases;
            this.LossCompass_HeaderKPI_2_Sim = AnalysisPeriodData.SPD;
            this.LossCompass_HeaderKPI_3_Sim = AnalysisPeriodData.ActualCases;

            LossCompass_TopLineResults_Values.Clear();
            LossCompass_TopLineResults_Values.Add(AnalysisPeriodData.UPDTpct);
            LossCompass_TopLineResults_Values.Add(AnalysisPeriodData.PDTpct);
            LossCompass_TopLineResults_Values.Add(0.00);
            LossCompass_TopLineResults_Values.Add(AnalysisPeriodData.OEE);
            LossCompass_TopLineResults_Names.Add("UPDT");
            LossCompass_TopLineResults_Names.Add("PDT");
            LossCompass_TopLineResults_Names.Add("Rate Loss");
            LossCompass_TopLineResults_Names.Add("OEE");


            //clear all master lists
            LossCompass_MasterEventList.Clear();
            LossCompass_MasterMappingAList.Clear();
            LossCompass_MasterMappingBList.Clear();
            LossCompass_ActiveNames.Clear();

            LossCompass_MasterMappingAList.Add(TierA_Mapping_Default);
            LossCompass_MasterMappingAList.Add(TierB_Mapping_Default);
            LossCompass_MasterMappingAList.Add(TierC_Mapping_Default);
            LossCompass_MasterMappingAList.Add(DowntimeField.Fault);

            LossCompass_MasterMappingBList.Add(DowntimeField.NA);
            LossCompass_MasterMappingBList.Add(DowntimeField.NA);
            LossCompass_MasterMappingBList.Add(DowntimeField.NA);
            LossCompass_MasterMappingBList.Add(DowntimeField.NA);


            //create the master list
            List<DTeventSummary> tmpList;
            if (TopLevelSelection == TopLevelSelected.Unplanned)
            {
                tmpList = AnalysisPeriodData.DT_Report.getUnplannedEventDirectory((int)DowntimeField.Tier1);
            }
            else //(TopLevelSelection = TopLevelSelected.Planned)
            {
                tmpList = AnalysisPeriodData.DT_Report.getPlannedEventDirectory((int)DowntimeField.Tier1);
            }
            for (int i = 0; i < tmpList.Count; i++)
            {
                TierA_Master.Add(tmpList[i]);
                TierA_Master[i].SchedTime = AnalysisPeriodData.schedTime;
                TierA_Master[i].UT = AnalysisPeriodData.UT;
            }
            sortTierA();
            LossCompass_MasterEventList.Add(TierA_Master);
            updateCurrentFromMaster(CardTier.A);

            //this probably needs to be changes
            LossCompass_CrystalBall_TurnOn();



            //Criteria 1 KPIs
            this.LossCompass_KPI1 = AnalysisPeriodData.UPDTpct; //DTpct
            this.LossCompass_KPI2 = AnalysisPeriodData.UPDT + AnalysisPeriodData.PDT;//DT
            this.LossCompass_KPI3 = AnalysisPeriodData.SPD; //SPD
            this.LossCompass_KPI4 = AnalysisPeriodData.MTBF; //MTBF
            this.LossCompass_KPI5 = AnalysisPeriodData.Stops; //Stops

            //Top KPIs
            this.primaryKPI1_Value = AnalysisPeriodData.OEE;
            this.primaryKPI2_Value = AnalysisPeriodData.Stops;
            this.primaryKPI3_Value = AnalysisPeriodData.ActualCases;
        }

        #region Crystal Ball
        #region Floating Sim Values
        //getting data for floating editor
        /*  public Tuple<double, double, double, double> LossCompass_CrystalBall_GetTooltipDataForFailureMode(string FailureModeName, string CardName)
          {
              double tmpVal = 0;
              int MasterIndex = 0;
              switch (getEnumForString_Card(CardName))
              {
                  case CardTier.A:
                      MasterIndex = TierA_Master.IndexOf(new DTeventSummary(FailureModeName));
                      tmpVal = TierA_Master[MasterIndex].SPD;
                      break;
                  case CardTier.B:
                      MasterIndex = TierB_Master.IndexOf(new DTeventSummary(FailureModeName));
                      tmpVal = TierB_Master[MasterIndex].SPD;
                      break;
                  case CardTier.C:
                      MasterIndex = TierC_Master.IndexOf(new DTeventSummary(FailureModeName));
                      tmpVal = TierC_Master[MasterIndex].SPD;
                      break;
              }
              return Tuple.Create<0,0,0,0>;
          }*/
        public double LossCompass_CrystalBall_OriginalStopsForFailureMode(string FailureModeName, CardTier CardName)
        {
            double tmpVal = 0;
            int MasterIndex = 0;
            switch (CardName)
            {
                case CardTier.A:
                    MasterIndex = TierA_Master.IndexOf(new DTeventSummary(FailureModeName));
                    tmpVal = TierA_Master[MasterIndex].SPD;
                    break;
                case CardTier.B:
                    MasterIndex = TierB_Master.IndexOf(new DTeventSummary(FailureModeName));
                    tmpVal = TierB_Master[MasterIndex].SPD;
                    break;
                case CardTier.C:
                    MasterIndex = TierC_Master.IndexOf(new DTeventSummary(FailureModeName));
                    tmpVal = TierC_Master[MasterIndex].SPD;
                    break;
            }
            return Math.Round(tmpVal, 2);
        }
        public double LossCompass_CrystalBall_OriginalMTTRForFailureMode(string FailureModeName, CardTier CardName)
        {
            double tmpVal = 0;
            int MasterIndex = 0;
            switch (CardName)
            {
                case CardTier.A:
                    MasterIndex = TierA_Master.IndexOf(new DTeventSummary(FailureModeName));
                    tmpVal = TierA_Master[MasterIndex].MTTR;
                    break;
                case CardTier.B:
                    MasterIndex = TierB_Master.IndexOf(new DTeventSummary(FailureModeName));
                    tmpVal = TierB_Master[MasterIndex].MTTR;
                    break;
                case CardTier.C:
                    MasterIndex = TierC_Master.IndexOf(new DTeventSummary(FailureModeName));
                    tmpVal = TierC_Master[MasterIndex].MTTR;
                    break;
            }
            return Math.Round(tmpVal, 2);
        }
        public double LossCompass_CrystalBall_NewStopsForFailureMode(string FailureModeName, CardTier CardName)
        {
            double tmpVal = 0;
            int MasterIndex = 0;
            switch (CardName)
            {
                case CardTier.A:
                    MasterIndex = TierA_Master.IndexOf(new DTeventSummary(FailureModeName));
                    tmpVal = TierA_Master[MasterIndex].SPDsim;
                    break;
                case CardTier.B:
                    MasterIndex = TierB_Master.IndexOf(new DTeventSummary(FailureModeName));
                    tmpVal = TierB_Master[MasterIndex].SPDsim;
                    break;
                case CardTier.C:
                    MasterIndex = TierC_Master.IndexOf(new DTeventSummary(FailureModeName));
                    tmpVal = TierC_Master[MasterIndex].SPDsim;
                    break;
            }
            return Math.Round(tmpVal, 2);
        }
        public double LossCompass_CrystalBall_NewMTTRForFailureMode(string FailureModeName, CardTier CardName)
        {
            double tmpVal = 0;
            int MasterIndex = 0;
            switch (CardName)
            {
                case CardTier.A:
                    MasterIndex = TierA_Master.IndexOf(new DTeventSummary(FailureModeName));
                    tmpVal = TierA_Master[MasterIndex].MTTRsim;
                    break;
                case CardTier.B:
                    MasterIndex = TierB_Master.IndexOf(new DTeventSummary(FailureModeName));
                    tmpVal = TierB_Master[MasterIndex].MTTRsim;
                    break;
                case CardTier.C:
                    MasterIndex = TierC_Master.IndexOf(new DTeventSummary(FailureModeName));
                    tmpVal = TierC_Master[MasterIndex].MTTRsim;
                    break;
            }
            return Math.Round(tmpVal, 2);
        }
        #endregion
        //simulation new system properties
        public void LossCompass_CrystalBall_Simulate(string FailureModeName, CardTier Tier, double rawNewStops, double rawNewDT, DateTime newDate)
        {
            double newStops = 1; //these are the actual scaling factors calculated from the raw inputs
            double newDT = 1;
            int MasterIndex;
            int CurrentIndex;
            double SchedTimeDelta = 0;
            switch (Tier)
            {
                case CardTier.A:
                    MasterIndex = TierA_Master.IndexOf(new DTeventSummary(FailureModeName));
                    CurrentIndex = TierA_Current.IndexOf(new DTeventSummary(FailureModeName));
                    newDT = rawNewDT / Math.Round(TierA_Master[MasterIndex].MTTRsim, 2);
                    newStops = rawNewStops / Math.Round(TierA_Master[MasterIndex].SPDsim, 2);
                    SchedTimeDelta = TierA_Master[MasterIndex].CrystalBall_simNewScaleFactors(newStops, newDT);
                    //  TierA_Current[CurrentIndex].CrystalBall_simNewScaleFactors(newStops, newDT);
                    updateIntermediateFromCurrent_TierA();
                    break;
                case CardTier.B:
                    MasterIndex = TierB_Master.IndexOf(new DTeventSummary(FailureModeName));
                    CurrentIndex = TierB_Current.IndexOf(new DTeventSummary(FailureModeName));
                    newDT = rawNewDT / Math.Round(TierB_Master[MasterIndex].MTTRsim, 2);
                    newStops = rawNewStops / Math.Round(TierB_Master[MasterIndex].SPDsim, 2);
                    SchedTimeDelta = TierB_Master[MasterIndex].CrystalBall_simNewScaleFactors(newStops, newDT);
                    // TierB_Current[CurrentIndex].CrystalBall_simNewScaleFactors(newStops, newDT);
                    updateIntermediateFromCurrent_TierB();
                    //now we need to roll down to tier C and up to tier A
                    TierA_Master[TierA_Master.IndexOf(new DTeventSummary(LossCompass_ActiveNames[TierA_Level]))].DTsim += SchedTimeDelta;
                    break;
                case CardTier.C:
                    MasterIndex = TierC_Master.IndexOf(new DTeventSummary(FailureModeName));
                    CurrentIndex = TierC_Current.IndexOf(new DTeventSummary(FailureModeName));
                    newDT = rawNewDT / Math.Round(TierC_Master[MasterIndex].MTTRsim, 2);
                    newStops = rawNewStops / Math.Round(TierC_Master[MasterIndex].SPDsim, 2);
                    SchedTimeDelta = TierC_Master[MasterIndex].CrystalBall_simNewScaleFactors(newStops, newDT);
                    //  TierC_Current[CurrentIndex].CrystalBall_simNewScaleFactors(newStops, newDT);
                    updateIntermediateFromCurrent_TierC();

                    TierA_Master[TierA_Master.IndexOf(new DTeventSummary(LossCompass_ActiveNames[TierA_Level]))].DTsim += SchedTimeDelta;
                    TierB_Master[TierB_Master.IndexOf(new DTeventSummary(LossCompass_ActiveNames[TierB_Level]))].DTsim += SchedTimeDelta;
                    break;
            }

            CrystalBall_Changelog.addNewSimulation(FailureModeName, LossCompass_ActiveNames, newDate, Tier, newStops, newDT);
            CrystalBall_Changelog.OEE_Steps.Add(AnalysisPeriodData.UT / (AnalysisPeriodData.schedTime + SchedTimeDelta));
            LossCompass_OEE_Sim = AnalysisPeriodData.UT / (AnalysisPeriodData.schedTime + SchedTimeDelta);
            LossCompass_HeaderKPI_2_Sim = newStops;
            //NEED TO SCALE THE CHANGE FIRST!!!

            //RECALC SCHED TIME 
            //initialize all the raw data - future state: initialize from saved simulation
            for (int i = 0; i < TierA_Master.Count; i++) { TierA_Master[i].SchedTimeSim += SchedTimeDelta; }
            for (int i = 0; i < TierB_Master.Count; i++) { TierB_Master[i].SchedTimeSim += SchedTimeDelta; }
            for (int i = 0; i < TierC_Master.Count; i++) { TierC_Master[i].SchedTimeSim += SchedTimeDelta; }
            for (int i = 0; i < TierA_Current.Count; i++) { TierA_Current[i].SchedTimeSim += SchedTimeDelta; }
            for (int i = 0; i < TierB_Current.Count; i++) { TierB_Current[i].SchedTimeSim += SchedTimeDelta; }
            for (int i = 0; i < TierC_Current.Count; i++) { TierC_Current[i].SchedTimeSim += SchedTimeDelta; }
            //put values in the intermediate sheet
            updateIntermediateFromCurrent_TierA();
            updateIntermediateFromCurrent_TierB();
            updateIntermediateFromCurrent_TierC();
        }
        public void LossCompass_CrystalBall_ClearSimulation(string FailureModeName, CardTier CardTier)
        {
            CrystalBall_Changelog.RemoveSimulation(FailureModeName, CardTier);

        }
        public void LossCompass_CrystalBall_ClearSimulation_All()
        {
            CrystalBall_Changelog.ClearAllSimulations();
        }
        private void LossCompass_CheckListForSimulation(CardTier CardName)
        {
            if (false) //this is check w/ sim database
            { }
            else //just reset back to base value
            {
                switch (CardName)
                {
                    case CardTier.A:
                        for (int i = 0; i < TierA_Master.Count; i++)
                        {
                            TierA_Master[i].CrystalBall_initialize();
                        }
                        break;
                    case CardTier.B:
                        for (int i = 0; i < TierB_Master.Count; i++)
                        {
                            TierB_Master[i].CrystalBall_initialize();
                        }
                        break;

                    case CardTier.C:
                        for (int i = 0; i < TierC_Master.Count; i++)
                        {
                            TierC_Master[i].CrystalBall_initialize();
                        }
                        break;
                }
            }
        }

        //turning it on and off
        public void LossCompass_CrystalBall_TurnOn()
        {
            LossCompass_OEE_Sim = LossCompass_OEE;
            LossCompass_isSimulationMode = true;
            CrystalBall_Changelog = new CrystalBallAnalysis("test", DateTime.Now, LossCompass_MasterMappingAList, LossCompass_MasterMappingBList);
            CrystalBall_Changelog.OEE_Steps.Add(LossCompass_OEE);
            //initialize all the raw data - future state: initialize from saved simulation
            for (int i = 0; i < TierA_Master.Count; i++) { TierA_Master[i].CrystalBall_initialize(); }
            for (int i = 0; i < TierB_Master.Count; i++) { TierB_Master[i].CrystalBall_initialize(); }
            for (int i = 0; i < TierC_Master.Count; i++) { TierC_Master[i].CrystalBall_initialize(); }
            for (int i = 0; i < TierA_Current.Count; i++) { TierA_Current[i].CrystalBall_initialize(); }
            for (int i = 0; i < TierB_Current.Count; i++) { TierB_Current[i].CrystalBall_initialize(); }
            for (int i = 0; i < TierC_Current.Count; i++) { TierC_Current[i].CrystalBall_initialize(); }
            //put values in the intermediate sheet
            updateIntermediateFromCurrent_TierA();
            updateIntermediateFromCurrent_TierB();
            updateIntermediateFromCurrent_TierC();
        }
        public void LossCompass_CrystalBall_TurnOff()
        {
            LossCompass_isSimulationMode = false;
        }
        #endregion

        #region Filtering
        /* Filter & Funnel */
        public List<double> LossCompass_Funnel_KPI1_Values = new List<double>();
        public List<double> LossCompass_Funnel_KPI2_Values = new List<double>();
        public List<double> LossCompass_Funnel_KPI3_Values = new List<double>();
        public DowntimeMetrics LossCompass_Funnel_KPI1 = DowntimeMetrics.OEE;
        public DowntimeMetrics LossCompass_Funnel_KPI2 = DowntimeMetrics.MTBF;
        public DowntimeMetrics LossCompass_Funnel_KPI3 = DowntimeMetrics.UPDTpct;

        //count of number of filters applied
        public int LossCompass_Funnel_NumberOfActiveFilters { get { return LossCompass_Funnel_ActiveFilters.Count; } }
        //lists of fields that user has selected to filter by
        public void LossCompass_Funnel_ApplyFunnel(string FieldName, List<string> selectedFieldsForFunnel)
        {
            LossCompass_Funnel_IncludedFields.Add(getEnumForString(FieldName));
            LossCompass_Funnel_ActiveFilters.Add(selectedFieldsForFunnel);
            LossCompass_Funnel_FilterActiveReport();
        }

        private List<DowntimeField> LossCompass_Funnel_IncludedFields { get; set; } = new List<DowntimeField>();
        //corresponding lists of skus/teams/etc that were selected by the user
        public List<List<string>> LossCompass_Funnel_ActiveFilters { get; set; } = new List<List<string>>();
        //remove a specific filter from analysis
        public void LossCompass_Funnel_ClearFilter(int LevelToClear)
        {
            LossCompass_Funnel_ActiveFilters.RemoveAt(LevelToClear);
            LossCompass_Funnel_IncludedFields.RemoveAt(LevelToClear);
            LossCompass_Funnel_KPI1_Values.RemoveAt(LevelToClear);
            LossCompass_Funnel_KPI2_Values.RemoveAt(LevelToClear);
            LossCompass_Funnel_KPI3_Values.RemoveAt(LevelToClear);
            LossCompass_Funnel_FilterActiveReport();
        }
        //analyze
        private void LossCompass_Funnel_FilterActiveReport()
        {
            AnalysisPeriodData.DT_Report.reFilterData_ClearAllFilters();
            LossCompass_Funnel_KPI1_Values.Clear();
            LossCompass_Funnel_KPI2_Values.Clear();
            LossCompass_Funnel_KPI3_Values.Clear();
            for (int i = 0; i < LossCompass_Funnel_IncludedFields.Count; i++)
            {
                LossCompass_Funnel_KPI1_Values.Add(AnalysisPeriodData.getKPIforMetric(LossCompass_Funnel_KPI1));
                LossCompass_Funnel_KPI2_Values.Add(AnalysisPeriodData.getKPIforMetric(LossCompass_Funnel_KPI2));
                LossCompass_Funnel_KPI3_Values.Add(AnalysisPeriodData.getKPIforMetric(LossCompass_Funnel_KPI3));
                AnalysisPeriodData.FilterDowntimeByField(LossCompass_Funnel_IncludedFields[i], LossCompass_Funnel_ActiveFilters[i]);
            }
            LossCompass_Funnel_KPI1_Values.Add(AnalysisPeriodData.getKPIforMetric(LossCompass_Funnel_KPI1));
            LossCompass_Funnel_KPI2_Values.Add(AnalysisPeriodData.getKPIforMetric(LossCompass_Funnel_KPI2));
            LossCompass_Funnel_KPI3_Values.Add(AnalysisPeriodData.getKPIforMetric(LossCompass_Funnel_KPI3));

            initializeLossCompass();
            updateCurrentFromMaster(CardTier.A);
            //   updateCurrentFromMaster(CardTier.B);
            //  updateCurrentFromMaster(CardTier.C);
        }
        //choices for different fields that could be used
        public List<string> LossCompass_Funnel_GetListForFieldsThatCanBeFiltered()
        {
            var tmpList = new List<string>();
            List<DowntimeField> fieldList = LossCompass_Funnel_GetListOfFieldsThatCanBeFiltered();
            for (int i = 0; i < fieldList.Count; i++)
            {
                tmpList.Add(getStringForEnum(fieldList[i]));
            }
            return tmpList;
        }
        private List<DowntimeField> LossCompass_Funnel_GetListOfFieldsThatCanBeFiltered()
        {
            //ADD FIELDS AVAILABLE FOR FILTERING IN THIS RELEASE OF FORK
            var tmpList = new List<DowntimeField>();
            tmpList.Add(DowntimeField.Team);
            tmpList.Add(DowntimeField.Product);
            return tmpList;
        }
        //   public List<DowntimeField> LossCompass_GetListOfFieldsThatAreFiltered() { return LossCompass_Funnel_IncludedFields; }
        public List<string> LossCompass_Funnel_GetListOfAllItmesForGivenField(string dtField)
        {
            return LossCompass_Funnel_GetListOfAllItemsForGivenField(getEnumForString(dtField));
        }
        private List<string> LossCompass_Funnel_GetListOfAllItemsForGivenField(DowntimeField Field)
        { return AnalysisPeriodData.DT_Report.getFilterList(Field); }

        public List<string> LossCompass_Funnel_GetListOfIncludedItemsForGivenField(DowntimeField Field)
        {
            int i = LossCompass_Funnel_IncludedFields.IndexOf(Field);
            return LossCompass_Funnel_ActiveFilters[i];
        }
        // public void LossCompass_Funnel_FilterItemsForAGivenField(DowntimeField Field, List<string> InclusionList) { }
        #endregion

        #region Drill-Down
        private bool LossCompass_didWeDrillUp()
        {
            if (TierA_Level < LossCompass_TierA_LevelVisible_Prior) { LossCompass_TierA_LevelVisible_Prior = TierA_Level; return true; }
            else { LossCompass_TierA_LevelVisible_Prior = TierA_Level; return false; }
        }
        private void LossCompass_DrillUp()
        {
            TierA_Master = LossCompass_MasterEventList[TierA_Level];
            TierB_Master = LossCompass_MasterEventList[TierB_Level];
            TierC_Master = LossCompass_MasterEventList[TierC_Level];
            for (int i = TierC_Level; i < LossCompass_MasterEventList.Count; i++) { LossCompass_MasterEventList.RemoveAt(i); }
            bool lockState = isSortLocked;
            isSortLocked = false;
            updateCurrentFromMaster(CardTier.A);
            updateCurrentFromMaster(CardTier.B);
            updateCurrentFromMaster(CardTier.C);
            Criteria_1_SelectionChanged(SortKPI);
            isSortLocked = lockState;
        }

        public void LossCompass_TopLineRefresh(int IndexSelected) { LossCompass_TopLineRefresh((TopLevelSelected)IndexSelected); }
        public void LossCompass_TopLineRefresh(TopLevelSelected Selection)
        {
            if (Selection != TopLevelSelection)
            {
                switch (Selection)
                {
                    case TopLevelSelected.Unplanned:
                        TopLevelSelection = Selection;
                        TierA_Master.Clear();
                        clearCurrentLists();
                        initializeLossCompass();
                        break;
                    case TopLevelSelected.Planned:
                        TopLevelSelection = Selection;
                        TierA_Master.Clear();
                        clearCurrentLists();
                        initializeLossCompass();
                        break;
                }
            }
        }

        public bool LossCompass_drillDown(string LossName = "", CardTier selectedTier = CardTier.NA)
        {
            if (LossCompass_didWeDrillUp())
            {
                //step up!
                LossCompass_DrillUp();
                return true;
            }
            else
            {
                switch (selectedTier)
                {
                    case CardTier.A:
                        if (getNextDowntimeField(TierA_Mapping_A) == DowntimeField.NA) { LossCompass_AddNameToMasterList(selectedTier, LossName); return false; }
                        break;
                    case CardTier.B:
                        if (getNextDowntimeField(TierB_Mapping_A) == DowntimeField.NA) { LossCompass_AddNameToMasterList(selectedTier, LossName); return false; }
                        break;
                    case CardTier.C:
                        if (getNextDowntimeField(TierC_Mapping_A) == DowntimeField.NA)
                        {
                            LossCompass_AddNameToMasterList(selectedTier, LossName);
                            return false;
                        }
                        break;
                    default: return false;
                }
                LossCompass_drillDown_Helper(LossName, selectedTier);
                return true;
            }
        }
        private void LossCompass_drillDown_Helper(string LossName, CardTier selectedTier)
        {
            List<DTeventSummary> tmpList;
            LossCompass_AddNameToMasterList(selectedTier, LossName);
            switch (selectedTier)
            {
                case CardTier.A:
                    if (TopLevelSelection == TopLevelSelected.Unplanned)
                    {
                        tmpList = AnalysisPeriodData.DT_Report.getMappedSubdirectoryForGivenHierarchy(LossCompass_MasterMappingAList.GetRange(0, TierA_Level + 1), LossCompass_MasterMappingBList.GetRange(0, TierA_Level + 1), LossCompass_ActiveNames.GetRange(0, TierA_Level + 1), LossCompass_MasterMappingAList[TierB_Level], LossCompass_MasterMappingBList[TierB_Level]);
                    }
                    else
                    {
                        tmpList = AnalysisPeriodData.DT_Report.getMappedSubdirectoryForGivenHierarchy_Planned(LossCompass_MasterMappingAList.GetRange(0, TierA_Level + 1), LossCompass_MasterMappingBList.GetRange(0, TierA_Level + 1), LossCompass_ActiveNames.GetRange(0, TierA_Level + 1), LossCompass_MasterMappingAList[TierB_Level], LossCompass_MasterMappingBList[TierB_Level]);
                    }
                    LossCompass_AddEventListToMasterList(CardTier.B, tmpList);
                    TierB_Master.Clear();
                    for (int i = 0; i < tmpList.Count; i++)
                    {
                        TierB_Master.Add(tmpList[i]);
                        TierB_Master[i].SchedTime = AnalysisPeriodData.schedTime;
                        TierB_Master[i].UT = AnalysisPeriodData.UT;
                    }
                    sortTierB();
                    LossCompass_CheckListForSimulation(CardTier.B);
                    updateCurrentFromMaster(CardTier.B);
                    break;
                case CardTier.B:
                    TierC_Master.Clear();
                    if (TopLevelSelection == TopLevelSelected.Unplanned)
                    {
                        tmpList = AnalysisPeriodData.DT_Report.getMappedSubdirectoryForGivenHierarchy(LossCompass_MasterMappingAList.GetRange(0, TierB_Level + 1), LossCompass_MasterMappingBList.GetRange(0, TierB_Level + 1), LossCompass_ActiveNames.GetRange(0, TierB_Level + 1), LossCompass_MasterMappingAList[TierC_Level], LossCompass_MasterMappingBList[TierC_Level]);
                    }
                    else
                    {
                        tmpList = AnalysisPeriodData.DT_Report.getMappedSubdirectoryForGivenHierarchy_Planned(LossCompass_MasterMappingAList.GetRange(0, TierB_Level + 1), LossCompass_MasterMappingBList.GetRange(0, TierB_Level + 1), LossCompass_ActiveNames.GetRange(0, TierB_Level + 1), LossCompass_MasterMappingAList[TierC_Level], LossCompass_MasterMappingBList[TierC_Level]);
                    }
                    LossCompass_AddEventListToMasterList(CardTier.C, tmpList);
                    for (int i = 0; i < tmpList.Count; i++)
                    {
                        TierC_Master.Add(tmpList[i]);
                        TierC_Master[i].SchedTime = AnalysisPeriodData.schedTime;
                        TierC_Master[i].UT = AnalysisPeriodData.UT;
                    }
                    sortTierC();
                    LossCompass_CheckListForSimulation(CardTier.C);
                    updateCurrentFromMaster(CardTier.C);
                    break;
                case CardTier.C: //we're going to at least 4 tiers!
                    TierA_Master = TierB_Master;
                    updateCurrentFromMaster(CardTier.A);
                    TierB_Master = TierC_Master;
                    updateCurrentFromMaster(CardTier.B);
                    TierC_Master.Clear();
                    if (TopLevelSelection == TopLevelSelected.Unplanned)
                    {
                        tmpList = AnalysisPeriodData.DT_Report.getMappedSubdirectoryForGivenHierarchy(LossCompass_MasterMappingAList.GetRange(0, TierC_Level + 1), LossCompass_MasterMappingBList.GetRange(0, TierC_Level + 1), LossCompass_ActiveNames.GetRange(0, TierC_Level), LossCompass_MasterMappingAList[TierC_Level], LossCompass_MasterMappingBList[TierC_Level]);
                    }
                    else
                    {
                        tmpList = AnalysisPeriodData.DT_Report.getMappedSubdirectoryForGivenHierarchy_Planned(LossCompass_MasterMappingAList.GetRange(0, TierC_Level + 1), LossCompass_MasterMappingBList.GetRange(0, TierC_Level + 1), LossCompass_ActiveNames.GetRange(0, TierC_Level), LossCompass_MasterMappingAList[TierC_Level], LossCompass_MasterMappingBList[TierC_Level]);
                    }
                    LossCompass_MasterEventList.Add(tmpList);
                    // LossCompass_AddEventListToMasterList(selectedTier, tmpList);
                    for (int i = 0; i < tmpList.Count; i++)
                    {
                        TierC_Master.Add(tmpList[i]);
                        TierC_Master[i].SchedTime = AnalysisPeriodData.schedTime;
                        TierC_Master[i].UT = AnalysisPeriodData.UT;
                    }
                    sortTierC();
                    LossCompass_CheckListForSimulation(CardTier.C);
                    updateCurrentFromMaster(CardTier.C);
                    break;
            }
        }
        #endregion

        #region Mapping / Remapping
        public List<string> LossCompass_getMappingFieldList(CardTier CardName, bool includeCurrentTier = true)
        {
            var tmpList = new List<DowntimeField>();
            var tmpStringList = new List<string>();
            tmpList = LossCompass_getMappingFieldList_Helper(CardName, includeCurrentTier);
            for (int i = 0; i < tmpList.Count; i++)
            {
                tmpStringList.Add(getStringForEnum(tmpList[i]));
            }
            tmpStringList.Add("");
            return tmpStringList;
        }
        internal List<DowntimeField> LossCompass_getMappingFieldList_Helper(CardTier CardName, bool includeCurrentTier)
        {
            var tmpList = new List<DowntimeField>();
            DowntimeField tmpField;// = getNextDowntimeField()
            switch (CardName) // get the "first next" downtime field
            {
                case CardTier.A:
                    if (includeCurrentTier) { tmpList.Add(TierA_Mapping_Default); }// tmpList.Add(TierA_Mapping); }
                    tmpField = TierA_Mapping_Default; break;// getNextDowntimeField(TierA_Mapping); break;
                case CardTier.B:
                    if (includeCurrentTier) { tmpList.Add(TierB_Mapping_A); }
                    tmpField = getNextDowntimeField(TierA_Mapping_Default); break;
                case CardTier.C:
                    if (includeCurrentTier) { tmpList.Add(TierC_Mapping_A); }
                    tmpField = getNextDowntimeField(TierA_Mapping_Default); break;
                default: tmpField = DowntimeField.NA; break;
            }
            while (tmpField != DowntimeField.NA)// && tmpList.IndexOf(tmpField) > -1)
            {
                if (tmpList.IndexOf(tmpField) == -1) { tmpList.Add(tmpField); }
                switch (CardName) // get the "first next" downtime field
                {
                    case CardTier.A: tmpField = getNextDowntimeField(tmpField); break;
                    case CardTier.B: tmpField = getNextDowntimeField(tmpField); break;
                    case CardTier.C: tmpField = getNextDowntimeField(tmpField); break;
                    default: tmpField = DowntimeField.NA; break;
                }
            }
            return tmpList;
        }

        public void LossCompass_CardRemap(CardTier CardName, string MappingA, string MappingB)
        {
            if (MappingA == MappingB || MappingB == "")
            {
                LossCompass_RemapTier(CardName, getEnumForString(MappingA), DowntimeField.NA);
            }
            else
            {
                LossCompass_RemapTier(CardName, getEnumForString(MappingA), getEnumForString(MappingB));
            }
        }

        public string LossCompass_GetMapping_A(CardTier CardName)
        {
            switch (CardName)
            {
                case CardTier.A:
                    return getStringForEnum(LossCompass_MasterMappingAList[TierA_Level]);
                case CardTier.B:
                    return getStringForEnum(LossCompass_MasterMappingAList[TierB_Level]);
                case CardTier.C:
                    return getStringForEnum(LossCompass_MasterMappingAList[TierC_Level]);
                default: return "";
            }
        }
        public string LossCompass_GetMapping_B(CardTier CardName)
        {
            switch (CardName)
            {
                case CardTier.A:
                    return getStringForEnum(LossCompass_MasterMappingBList[TierA_Level]);
                case CardTier.B:
                    return getStringForEnum(LossCompass_MasterMappingBList[TierB_Level]);
                case CardTier.C:
                    return getStringForEnum(LossCompass_MasterMappingBList[TierC_Level]);
                default: return "";
            }
        }

        public void LossCompass_RemapTier(CardTier Card, DowntimeField MappingA, DowntimeField MappingB)
        {
            switch (Card)
            {
                case CardTier.A:
                    //adjust master mapping list
                    LossCompass_MasterMappingAList[TierA_Level] = MappingA;
                    LossCompass_MasterMappingAList[TierB_Level] = getNextDowntimeField(MappingA);
                    LossCompass_MasterMappingAList[TierC_Level] = getNextDowntimeField(getNextDowntimeField(MappingA));
                    LossCompass_MasterMappingBList[TierA_Level] = MappingB;
                    LossCompass_MasterMappingBList[TierB_Level] = getNextDowntimeField(MappingB);
                    LossCompass_MasterMappingBList[TierC_Level] = getNextDowntimeField(getNextDowntimeField(MappingB));
                    //get new data
                    if (TopLevelSelection == TopLevelSelected.Unplanned)
                    {
                        TierA_Master = AnalysisPeriodData.DT_Report.getMappedSubdirectoryForGivenHierarchy(LossCompass_MasterMappingAList.GetRange(0, TierA_Level), LossCompass_MasterMappingBList.GetRange(0, TierA_Level), LossCompass_ActiveNames.GetRange(0, TierA_Level), MappingA, MappingB);
                    }
                    else
                    {
                        TierA_Master = AnalysisPeriodData.DT_Report.getMappedSubdirectoryForGivenHierarchy_Planned(LossCompass_MasterMappingAList.GetRange(0, TierA_Level), LossCompass_MasterMappingBList.GetRange(0, TierA_Level), LossCompass_ActiveNames.GetRange(0, TierA_Level), MappingA, MappingB);
                    }
                    for (int i = 0; i < TierA_Master.Count; i++)
                    {
                        TierA_Master[i].SchedTime = AnalysisPeriodData.schedTime;
                        TierA_Master[i].UT = AnalysisPeriodData.UT;
                    }
                    break;
                case CardTier.B:
                    //adjust the master mapping list
                    LossCompass_MasterMappingAList[TierB_Level] = MappingA;
                    LossCompass_MasterMappingAList[TierC_Level] = getNextDowntimeField(MappingA);
                    LossCompass_MasterMappingBList[TierB_Level] = MappingB;
                    LossCompass_MasterMappingBList[TierC_Level] = getNextDowntimeField(MappingB);
                    //find new data
                    if (TopLevelSelection == TopLevelSelected.Unplanned)
                    {
                        TierB_Master = AnalysisPeriodData.DT_Report.getMappedSubdirectoryForGivenHierarchy(LossCompass_MasterMappingAList.GetRange(0, TierB_Level), LossCompass_MasterMappingBList.GetRange(0, TierB_Level), LossCompass_ActiveNames.GetRange(0, TierB_Level), MappingA, MappingB);
                    }
                    else
                    {
                        TierB_Master = AnalysisPeriodData.DT_Report.getMappedSubdirectoryForGivenHierarchy_Planned(LossCompass_MasterMappingAList.GetRange(0, TierB_Level), LossCompass_MasterMappingBList.GetRange(0, TierB_Level), LossCompass_ActiveNames.GetRange(0, TierB_Level), MappingA, MappingB);
                    }
                    for (int i = 0; i < TierB_Master.Count; i++)
                    {
                        TierB_Master[i].SchedTime = AnalysisPeriodData.schedTime;
                        TierB_Master[i].UT = AnalysisPeriodData.UT;
                    }
                    break;
                case CardTier.C:
                    //adjust the master mapping list
                    LossCompass_MasterMappingAList[TierC_Level] = MappingA;
                    LossCompass_MasterMappingAList[TierC_Level + 1] = getNextDowntimeField(MappingA);
                    LossCompass_MasterMappingBList[TierC_Level] = MappingB;
                    LossCompass_MasterMappingBList[TierC_Level + 1] = getNextDowntimeField(MappingB);
                    //find new data
                    if (TopLevelSelection == TopLevelSelected.Unplanned)
                    {
                        TierC_Master = AnalysisPeriodData.DT_Report.getMappedSubdirectoryForGivenHierarchy(LossCompass_MasterMappingAList.GetRange(0, TierC_Level), LossCompass_MasterMappingBList.GetRange(0, TierC_Level), LossCompass_ActiveNames.GetRange(0, TierC_Level), MappingA, MappingB);
                    }
                    else
                    {
                        TierC_Master = AnalysisPeriodData.DT_Report.getMappedSubdirectoryForGivenHierarchy_Planned(LossCompass_MasterMappingAList.GetRange(0, TierC_Level), LossCompass_MasterMappingBList.GetRange(0, TierC_Level), LossCompass_ActiveNames.GetRange(0, TierC_Level), MappingA, MappingB);
                    }
                    for (int i = 0; i < TierC_Master.Count; i++)
                    {
                        TierC_Master[i].SchedTime = AnalysisPeriodData.schedTime;
                        TierC_Master[i].UT = AnalysisPeriodData.UT;
                    }
                    break;
            }
            sortTier(Card);
            updateCurrentFromMaster(Card);
        }
        #endregion

        #region Scrolling
        /*UI function for scrolling any of the cards*/
        public void LossCompass_Scroll(CardTier Card, int scrollAmount)
        {
            updateCurrentFromMaster(Card, scrollAmount);
        }

        #endregion

        #region KPI Criteria 1 
        public void LossCompass_Criteria_1_Unlock() { isSortLocked = false; }
        public void LossCompass_Criteria_1_Lock(int lockedKPI) { LossCompass_Criteria_1_Lock((KPIs)lockedKPI); }
        public void LossCompass_Criteria_1_Lock(KPIs lockedKPI)
        {
            if (lockedKPI != current_Criteria1_KPI)
            {
                switch (lockedKPI)
                {
                    case KPIs.One: //%
                        SortKPI = DowntimeMetrics.DTpct;
                        break;
                    case KPIs.Two:
                        SortKPI = DowntimeMetrics.DT;
                        break;
                    case KPIs.Three:
                        SortKPI = DowntimeMetrics.SPD;
                        break;
                    case KPIs.Four:
                        SortKPI = DowntimeMetrics.MTBF;
                        break;
                    case KPIs.Five:
                        SortKPI = DowntimeMetrics.Stops;
                        break;
                }
                sortAllTiers(SortKPI);
                isSortLocked = true;
            }
        }


        public void Criteria_1_SelectionChanged(int newKPI)
        {
            KPIs tmpKPI = (KPIs)newKPI;
            if (tmpKPI != current_Criteria1_KPI)
            {
                switch (tmpKPI)
                {
                    case KPIs.One:
                        Criteria_1_SelectionChanged(DowntimeMetrics.DTpct);
                        break;
                    case KPIs.Two:
                        Criteria_1_SelectionChanged(DowntimeMetrics.DT);
                        break;
                    case KPIs.Three:
                        Criteria_1_SelectionChanged(DowntimeMetrics.SPD);
                        break;
                    case KPIs.Four:
                        Criteria_1_SelectionChanged(DowntimeMetrics.MTBF);
                        break;
                    case KPIs.Five:
                        Criteria_1_SelectionChanged(DowntimeMetrics.Stops);
                        break;
                    default:
                        System.Diagnostics.Debugger.Break();
                        break;
                }
            }
        }
        private void Criteria_1_SelectionChanged(DowntimeMetrics newKPI)
        {
            current_Criteria1_Display = newKPI;
            if (!isSortLocked)
            {
                sortAllTiers_Current(newKPI);
                clearCurrentLists();
            }
            for (int i = 0; i < TierA_MAXBARS; i++)
            {
                if (i < TierA_Current.Count)
                {
                    TierA_Values[i] = TierA_Current[i].getKPI(newKPI);
                    TierA_Values_2[i] = TierA_Current[i].getKPI(current_Criteria2_KPI);
                    TierA_Names[i] = TierA_Current[i].Name;
                }
                else { TierA_Values[i] = 0; TierA_Values_2[i] = 0; }
            }
            for (int i = 0; i < TierB_MAXBARS; i++)
            {
                if (i < TierB_Current.Count)
                {
                    TierB_Values[i] = TierB_Current[i].getKPI(newKPI);
                    TierB_Values_2[i] = TierB_Current[i].getKPI(current_Criteria2_KPI);
                    TierB_Names[i] = TierB_Current[i].Name;
                }
                else { TierB_Values[i] = 0; TierB_Values_2[i] = 0; }
            }
            for (int i = 0; i < TierC_MAXBARS; i++)
            {
                if (i < TierC_Current.Count)
                {
                    TierC_Values[i] = TierC_Current[i].getKPI(newKPI);
                    TierC_Names[i] = TierC_Current[i].Name;
                    TierC_Values_2[i] = TierC_Current[i].getKPI(current_Criteria2_KPI);
                }
                else { TierC_Values[i] = 0; TierC_Values_2[i] = 0; }
            }
        }

        #endregion

        #region Prioritization
        public void LossCompass_SetPrioritizationKPI(DowntimeMetrics KPI)
        {
            switch (KPI)
            {
                case DowntimeMetrics.Survivability:
                    break;
                case DowntimeMetrics.Chronicity:
                    break;
                case DowntimeMetrics.NA:
                    LossCompass_Priority_Reset();
                    break;
            }
        }

        private void LossCompass_Priority_Reset()
        {

        }
        #endregion

        #region KPI Criteria 2
        public void LossCompass_SetSecondaryKPI(DowntimeMetrics KPI)
        {
            if (KPI != current_Criteria2_KPI)
            {
                current_Criteria2_KPI = KPI;
                for (int i = 0; i < TierA_MAXBARS; i++)
                {
                    if (i < TierA_Current.Count)
                    {
                        TierA_Values_2[i] = TierA_Current[i].getKPI(current_Criteria2_KPI);
                    }
                    else
                    {
                        TierA_Values_2[i] = 0;
                    }
                }
                for (int i = 0; i < TierB_MAXBARS; i++)
                {
                    if (i < TierB_Current.Count)
                    {
                        TierB_Values_2[i] = TierB_Current[i].getKPI(current_Criteria2_KPI);
                    }
                    else
                    {
                        TierB_Values_2[i] = 0;
                    }
                }

                for (int i = 0; i < TierC_MAXBARS; i++)
                {
                    if (i < TierC_Current.Count)
                    {
                        TierC_Values_2[i] = TierC_Current[i].getKPI(current_Criteria2_KPI);
                    }
                    else
                    {
                        TierC_Values_2[i] = 0;
                    }
                }
            }
        }
        #endregion

        #region Sorting
        //sorting
        private void sortTier(CardTier Tier)
        {
            switch (Tier)
            {
                case CardTier.A:
                    sortTierA();
                    break;
                case CardTier.B:
                    sortTierB();
                    break;
                case CardTier.C:
                    sortTierC();
                    break;
            }
        }
        private void sortAllTiers(DowntimeMetrics sortableMetric)
        {
            if (!isSortLocked)
            {
                SortKPI = sortableMetric;
                sortTierA();
                sortTierB();
                sortTierC();
            }
        }
        private void sortTierA()
        {
            for (int i = 0; i < TierA_Master.Count; i++) { TierA_Master[i].setSortParam(SortKPI); }
            TierA_Master.Sort();
        }
        private void sortTierB()
        {
            for (int i = 0; i < TierB_Master.Count; i++) { TierB_Master[i].setSortParam(SortKPI); }
            TierB_Master.Sort();
        }
        private void sortTierC()
        {
            for (int i = 0; i < TierC_Master.Count; i++) { TierC_Master[i].setSortParam(SortKPI); }
            TierC_Master.Sort();
        }


        private void sortAllTiers_Current(DowntimeMetrics sortableMetric)
        {
            if (!isSortLocked)
            {
                SortKPI = sortableMetric;
                sortTierA_Current();
                sortTierB_Current();
                sortTierC_Current();

                sortAllTiers(sortableMetric);
            }
        }
        private void sortTierA_Current()
        {
            for (int i = 0; i < TierA_Current.Count; i++) { TierA_Current[i].setSortParam(SortKPI); }
            TierA_Current.Sort();
        }
        private void sortTierB_Current()
        {
            for (int i = 0; i < TierB_Current.Count; i++) { TierB_Current[i].setSortParam(SortKPI); }
            TierB_Current.Sort();
        }
        private void sortTierC_Current()
        {
            for (int i = 0; i < TierC_Current.Count; i++) { TierC_Current[i].setSortParam(SortKPI); }
            TierC_Current.Sort();
        }
        #endregion

        #region Raw Data Display
        public List<DTevent> getRawData(string LossName, string Card)
        {
            switch (Card)
            {
                case "A":
                    return getRawData(LossName, CardTier.A);
                case "B":
                    return getRawData(LossName, CardTier.B);
                default: //C
                    return getRawData(LossName, CardTier.C);
            }
        }
        public List<DTevent> getRawData(string LossName, CardTier Card) //SRO NEEDS UPDATE !!!
        {
            var tmpList = new List<DTevent>();
            var rowList = new List<int>();
            int tmpIndex;
            /* Step 1: Find the row list */
            switch (Card)
            {
                case CardTier.A:
                    tmpIndex = TierA_Current.IndexOf(new DTeventSummary(LossName));
                    rowList = TierA_Current[tmpIndex].RawRows;
                    break;
                case CardTier.B:
                    tmpIndex = TierB_Current.IndexOf(new DTeventSummary(LossName));
                    rowList = TierB_Current[tmpIndex].RawRows;
                    break;
                case CardTier.C:
                    tmpIndex = TierC_Current.IndexOf(new DTeventSummary(LossName));
                    rowList = TierC_Current[tmpIndex].RawRows;
                    break;
            }
            /* Step 2: Get the events from the data */
            if (TopLevelSelection == TopLevelSelected.Unplanned) { for (int i = 0; i < rowList.Count; i++) { tmpList.Add(AnalysisPeriodData.rawUnplannedData[i]); } }
            else { for (int i = 0; i < rowList.Count; i++) { tmpList.Add(AnalysisPeriodData.rawUnplannedData[i]); } }//this needs to be changed to PLANNED!!!
            return tmpList;
        }
        #endregion

        #region Helper Functions
        //updating intermediate from master
        private void LossCompass_AddMappingsToMasterList(CardTier Card, DowntimeField A, DowntimeField B)
        {
            int listSize = LossCompass_MasterMappingAList.Count;
            switch (Card)
            {
                case CardTier.A:
                    if (listSize == 0) { LossCompass_MasterMappingAList.Add(A); LossCompass_MasterMappingBList.Add(B); }
                    else { LossCompass_MasterMappingAList[TierA_Level] = A; LossCompass_MasterMappingBList[TierA_Level] = B; }
                    break;
                case CardTier.B:
                    if (TierB_Level >= listSize) { LossCompass_MasterMappingAList.Add(A); LossCompass_MasterMappingBList.Add(B); }
                    else { LossCompass_MasterMappingAList[TierB_Level] = A; LossCompass_MasterMappingBList[TierB_Level] = B; }
                    break;
                case CardTier.C:
                    if (TierC_Level >= listSize) { LossCompass_MasterMappingAList.Add(A); LossCompass_MasterMappingBList.Add(B); }
                    else { LossCompass_MasterMappingAList[TierC_Level] = A; LossCompass_MasterMappingBList[TierC_Level] = B; }
                    break;
            }
        }
        private void LossCompass_AddNameToMasterList(CardTier Card, string LossName)
        {
            int listSize = LossCompass_ActiveNames.Count;
            switch (Card)
            {
                case CardTier.A:
                    if (listSize == 0) { LossCompass_ActiveNames.Add(LossName); }
                    else { LossCompass_ActiveNames[TierA_Level] = LossName; }
                    break;
                case CardTier.B:
                    if (TierB_Level >= listSize) { LossCompass_ActiveNames.Add(LossName); }
                    else { LossCompass_ActiveNames[TierB_Level] = LossName; }
                    break;
                case CardTier.C:
                    if (TierC_Level >= listSize) { LossCompass_ActiveNames.Add(LossName); }
                    else { LossCompass_ActiveNames[TierC_Level] = LossName; }
                    break;
            }
        }
        private void LossCompass_AddEventListToMasterList(CardTier Card, List<DTeventSummary> LossList)
        {
            int listSize = LossCompass_MasterEventList.Count;
            switch (Card)
            {
                case CardTier.A:
                    if (listSize == 0) { LossCompass_MasterEventList.Add(LossList); }
                    else { LossCompass_MasterEventList[TierA_Level] = LossList; }
                    break;
                case CardTier.B:
                    if (TierB_Level >= listSize) { LossCompass_MasterEventList.Add(LossList); }
                    else { LossCompass_MasterEventList[TierB_Level] = LossList; }
                    break;
                case CardTier.C:
                    if (TierC_Level >= listSize) { LossCompass_MasterEventList.Add(LossList); }
                    else { LossCompass_MasterEventList[TierC_Level] = LossList; }
                    break;

            }
        }


        private void updateCurrentFromMaster(CardTier Tier, int scrollOffset = 0)
        {
            switch (Tier)
            {
                case CardTier.A:
                    TierA_Current.Clear();
                    clearTierAList();
                    //create the current list
                    for (int i = scrollOffset; i < Math.Min(TierA_MAXBARS + scrollOffset, TierA_Master.Count); i++) { TierA_Current.Add(TierA_Master[i]); }
                    updateIntermediateFromCurrent_TierA();//update the current values
                    break;
                case CardTier.B:
                    TierB_Current.Clear();
                    clearTierBList();
                    //create the current list
                    for (int i = scrollOffset; i < Math.Min(TierB_MAXBARS + scrollOffset, TierB_Master.Count); i++) { TierB_Current.Add(TierB_Master[i]); }
                    updateIntermediateFromCurrent_TierB();//update the current values
                    break;
                case CardTier.C:
                    TierC_Current.Clear();
                    clearTierCList();
                    //create the current list
                    for (int i = scrollOffset; i < Math.Min(TierC_MAXBARS + scrollOffset, TierC_Master.Count); i++) { TierC_Current.Add(TierC_Master[i]); }
                    updateIntermediateFromCurrent_TierC();//update the current values
                    break;
            }
        }
        private void updateIntermediateFromCurrent_TierA()
        {
            for (int i = 0; i < TierA_MAXBARS; i++)
            {
                if (i < TierA_Current.Count)
                {
                    TierA_Names[i] = TierA_Current[i].Name;
                    TierA_Values[i] = TierA_Current[i].getKPI(current_Criteria1_Display);
                    TierA_Values_2[i] = TierA_Current[i].getKPI(current_Criteria2_KPI);
                    if (LossCompass_isSimulationMode)
                    {
                        TierA_Values_Sim[i] = TierA_Current[i].getKPI_Sim(current_Criteria1_Display);
                        TierA_Values_2_Sim[i] = TierA_Current[i].getKPI_Sim(current_Criteria2_KPI);
                    }
                }
                else
                {
                    TierA_Names[i] = " ";
                    TierA_Values[i] = 0;
                    TierA_Values_2[i] = 0;
                    TierA_Values_Sim[i] = 0;
                    TierA_Values_2_Sim[i] = 0;
                }
            }
        }
        private void updateIntermediateFromCurrent_TierB()
        {
            for (int i = 0; i < TierB_MAXBARS; i++)
            {
                if (i < TierB_Current.Count)
                {
                    TierB_Names[i] = TierB_Current[i].Name;
                    TierB_Values[i] = TierB_Current[i].getKPI(current_Criteria1_Display);
                    TierB_Values_2[i] = TierB_Current[i].getKPI(current_Criteria2_KPI);
                    if (LossCompass_isSimulationMode)
                    {
                        TierB_Values_Sim[i] = TierB_Current[i].getKPI_Sim(current_Criteria1_Display);
                        TierB_Values_2_Sim[i] = TierB_Current[i].getKPI_Sim(current_Criteria2_KPI);
                    }
                }
                else
                {
                    TierB_Names[i] = " ";
                    TierB_Values[i] = 0;
                    TierB_Values_2[i] = 0;
                    TierB_Values_Sim[i] = 0;
                    TierB_Values_2_Sim[i] = 0;
                }
            }
        }
        private void updateIntermediateFromCurrent_TierC()
        {
            for (int i = 0; i < TierC_MAXBARS; i++)
            {
                if (i < TierC_Current.Count)
                {
                    TierC_Names[i] = TierC_Current[i].Name;
                    TierC_Values[i] = TierC_Current[i].getKPI(current_Criteria1_Display);
                    TierC_Values_2[i] = TierC_Current[i].getKPI(current_Criteria2_KPI);
                    if (LossCompass_isSimulationMode)
                    {
                        TierC_Values_Sim[i] = TierC_Current[i].getKPI_Sim(current_Criteria1_Display);
                        TierC_Values_2_Sim[i] = TierC_Current[i].getKPI_Sim(current_Criteria2_KPI);
                    }
                }
                else
                {
                    TierC_Names[i] = " ";
                    TierC_Values[i] = 0;
                    TierC_Values_2[i] = 0;
                    TierC_Values_Sim[i] = 0;
                    TierC_Values_2_Sim[i] = 0;
                }
            }
        }


        //resetting lists
        private void clearCurrentLists()
        { //clears intermediate sheet variables!
            clearTierAList();
            clearTierBList();
            clearTierCList();
        }
        private void clearTierAList()
        {
            for (int i = 0; i < TierA_MAXBARS; i++)
            {
                TierA_Values[i] = 0;
                TierA_Values_2[i] = 0;
                TierA_Values_Sim[i] = 0;
                TierA_Values_2_Sim[i] = 0;
                TierA_Names[i] = "";
            }
        }
        private void clearTierBList()
        {
            for (int i = 0; i < TierB_MAXBARS; i++)
            {
                TierB_Values[i] = 0;
                TierB_Values_2[i] = 0;
                TierB_Values_Sim[i] = 0;
                TierB_Values_2_Sim[i] = 0;
                TierB_Names[i] = "";
            }
        }
        private void clearTierCList()
        {
            for (int i = 0; i < TierC_MAXBARS; i++)
            {
                TierC_Values[i] = 0;
                TierC_Values_2[i] = 0;
                TierC_Values_Sim[i] = 0;
                TierC_Values_2_Sim[i] = 0;
                TierC_Names[i] = "";
            }
        }
        #endregion
        #endregion

    }
}
