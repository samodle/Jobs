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
        #region Gap Analysis
        #region Variables
        internal SystemSummaryReport Gap_AnalysisPeriodReport_L1;

        public List<string> Gap_Level1_LossNames_Unplanned = new List<string>();
        public List<string> Gap_Level1_LossNames_Planned = new List<string>();
        public List<List<string>> Gap_Level2_LossNames_Unplanned = new List<List<string>>();
        public List<List<string>> Gap_Level2_LossNames_Planned = new List<List<string>>();

        public List<double> Gap_Level1_LossKPIs_Unplanned = new List<double>();
        public List<double> Gap_Level1_LossKPIs_Planned = new List<double>();
        public List<List<double>> Gap_Level2_LossKPIs_Unplanned = new List<List<double>>();
        public List<List<double>> Gap_Level2_LossKPIs_Planned = new List<List<double>>();

        public List<Tuple<double, string>> Gap_Level1_Unplanned = new List<Tuple<double, string>>();
        public List<Tuple<double, string>> Gap_Level1_Planned = new List<Tuple<double, string>>();
        public List<List<Tuple<double, string>>> Gap_Level2_Unplanned = new List<List<Tuple<double, string>>>();
        public List<List<Tuple<double, string>>> Gap_Level2_Planned = new List<List<Tuple<double, string>>>();

        protected DowntimeMetrics Gap_ActiveKPI { get; set; } = DowntimeMetrics.DTpct;
        #endregion

        #region Targets
        public List<KPITarget> Gap_ActiveTargets = new List<KPITarget>();

        //returns -1 if no target, else returns target value
        public double Gap_FindTargetForLoss(string Name, DowntimeMetrics KPI, DowntimeField MappingA, DowntimeField MappingB)
        {
            int TargetIndex = Gap_ActiveTargets.IndexOf(new KPITarget(Name, KPI, MappingA, MappingB));
            return TargetIndex == -1 ? TargetIndex : Gap_ActiveTargets[TargetIndex].Target;
        }

        public void Gap_AddTarget(string Name, double Target, DowntimeMetrics KPI, DowntimeField MappingA, DowntimeField MappingB)
        {
            int TargetIndex = Gap_ActiveTargets.IndexOf(new KPITarget(Name, KPI, MappingA, MappingB));
            if (TargetIndex > -1)
                Gap_ActiveTargets.RemoveAt(TargetIndex);
            Gap_ActiveTargets.Add(new KPITarget(Name, Target, KPI, MappingA, MappingB));
        }

        //retrieve all targets from cloud or local repository
        private void Gap_DownloadTargets()
        {

        }

        //upload to cloud or save to local repository
        internal void Gap_SaveUploadTargets() { }
        #endregion

        public void Gap_SetNewKPI(DowntimeMetrics newKPI)
        {
            Gap_ActiveKPI = newKPI;
        }

        //needs to be called once "on load"
        internal void initializeGapAnalysis()
        {
            Gap_AnalysisPeriodReport_L1 = rawData.getSubset(this.startTime, this.endTime);
            Gap_AnalysisPeriodReport_L1.reMapDowntime(Gap_Level1_MappingA, Gap_Level1_MappingB);
            // Gap_AnalysisPeriodReport_L2 = rawData.getSubset(this.startTime, this.endTime);
            //Gap_AnalysisPeriodReport_L2.reMapDowntime(Gap_Level2_MappingA, Gap_Level2_MappingB);

            Gap_SetNameLists();
        }
        private void Gap_SetNameLists()
        {
            Gap_Level1_LossNames_Planned.Clear();
            Gap_Level1_LossNames_Unplanned.Clear();
            Gap_Level2_LossNames_Planned.Clear();
            Gap_Level2_LossNames_Unplanned.Clear();

            Gap_Level1_LossKPIs_Planned.Clear();
            Gap_Level1_LossKPIs_Unplanned.Clear();
            Gap_Level2_LossKPIs_Planned.Clear();
            Gap_Level2_LossKPIs_Unplanned.Clear();

            Gap_Level1_Unplanned.Clear();
            Gap_Level2_Unplanned.Clear();
            Gap_Level1_Planned.Clear();
            Gap_Level2_Planned.Clear();

            //  Gap_AnalysisPeriodReport_L1.DT_Report.MappedDirector

            for (int i = 0; i < Gap_AnalysisPeriodReport_L1.DT_Report.MappedDirectory.Count; i++)
            {
                Gap_Level1_LossNames_Unplanned.Add(Gap_AnalysisPeriodReport_L1.DT_Report.MappedDirectory[i].Name);
                Gap_Level1_LossKPIs_Unplanned.Add(Gap_AnalysisPeriodReport_L1.DT_Report.MappedDirectory[i].getKPI(Gap_ActiveKPI, Gap_AnalysisPeriodReport_L1.schedTime, Gap_AnalysisPeriodReport_L1.UT));

                var tmpList = Gap_AnalysisPeriodReport_L1.DT_Report.getMappedSubdirectoryForGivenHierarchy1(Gap_Level1_MappingA, Gap_Level1_MappingB, Gap_AnalysisPeriodReport_L1.DT_Report.MappedDirectory[i].Name, Gap_Level2_MappingA, Gap_Level2_MappingB);
                var tmpStringList = new List<string>();
                var tmpKPIList = new List<double>();

                for (int j = 0; j < tmpList.Count; j++)
                {
                    tmpStringList.Add(tmpList[j].Name);
                    tmpKPIList.Add(tmpList[j].getKPI(Gap_ActiveKPI, Gap_AnalysisPeriodReport_L1.schedTime, Gap_AnalysisPeriodReport_L1.UT));
                }

                Gap_Level2_LossNames_Unplanned.Add(tmpStringList);
                Gap_Level2_LossKPIs_Unplanned.Add(tmpKPIList);

            }

            for (int i = 0; i < Gap_AnalysisPeriodReport_L1.DT_Report.MappedDirectory_Planned.Count; i++)
            {
                Gap_Level1_LossNames_Planned.Add(Gap_AnalysisPeriodReport_L1.DT_Report.MappedDirectory_Planned[i].Name);
                Gap_Level1_LossKPIs_Planned.Add(Gap_AnalysisPeriodReport_L1.DT_Report.MappedDirectory_Planned[i].getKPI(Gap_ActiveKPI, Gap_AnalysisPeriodReport_L1.schedTime, Gap_AnalysisPeriodReport_L1.UT));

                var tmpList = Gap_AnalysisPeriodReport_L1.DT_Report.getMappedSubdirectoryForGivenHierarchy1_Planned(Gap_Level1_MappingA, Gap_Level1_MappingB, Gap_AnalysisPeriodReport_L1.DT_Report.MappedDirectory_Planned[i].Name, Gap_Level2_MappingA, Gap_Level2_MappingB);
                var tmpStringList = new List<string>();
                var tmpKPIList = new List<double>();

                for (int j = 0; j < tmpList.Count; j++)
                {
                    tmpStringList.Add(tmpList[j].Name);
                    tmpKPIList.Add(tmpList[j].getKPI(Gap_ActiveKPI, Gap_AnalysisPeriodReport_L1.schedTime, Gap_AnalysisPeriodReport_L1.UT));
                }

                Gap_Level2_LossNames_Planned.Add(tmpStringList);
                Gap_Level2_LossKPIs_Planned.Add(tmpKPIList);
            }


            //sort the stuff
            List<Tuple<double, string>> result0 = new List<Tuple<double, string>>();
            List<Tuple<double, string>> result1 = new List<Tuple<double, string>>();

            for (int i = 0; i < Gap_Level1_LossNames_Unplanned.Count; i++)
            {
                result0.Add(new Tuple<double, string>(Gap_Level1_LossKPIs_Unplanned[i], Gap_Level1_LossNames_Unplanned[i]));
            }
            for (int i = 0; i < Gap_Level1_LossNames_Planned.Count; i++)
            {
                result1.Add(new Tuple<double, string>(Gap_Level1_LossKPIs_Planned[i], Gap_Level1_LossNames_Planned[i]));
            }

            /*
            for (int i = 0; i < Gap_Level2_LossNames_Unplanned.Count; i++)
            {
                result2.Add(new Tuple<double, string>(Gap_Level2_LossKPIs_Unplanned[i], Gap_Level2_LossNames_Unplanned[i]));
            }
            for (int i = 0; i < Gap_Level2_LossNames_Planned.Count; i++)
            {
                result3.Add(new Tuple<double, string>(Gap_Level2_LossKPIs_Planned[i], Gap_Level2_LossNames_Planned[i]));
            }


            Gap_Level1_Unplanned =  result0.OrderBy(x => x.Item1).ToList();
            Gap_Level1_Planned = result1.OrderBy(x => x.Item1).ToList();
            Gap_Level2_Unplanned = result2.OrderBy(x => x.Item1).ToList();
            Gap_Level2_Planned = result3.OrderBy(x => x.Item1).ToList();*/
        }


        #region Mapping
        private DowntimeField Gap_Level1_MappingA = DowntimeField.Tier2;
        private DowntimeField Gap_Level1_MappingB = DowntimeField.NA;

        private DowntimeField Gap_Level2_MappingA = DowntimeField.Tier3;
        private DowntimeField Gap_Level2_MappingB = DowntimeField.NA;

        public void Gap_reMap_Level1(DowntimeField MappingA, DowntimeField MappingB)
        {
            Gap_AnalysisPeriodReport_L1.reMapDowntime(MappingA, MappingB);
            Gap_Level1_MappingA = MappingA;
            Gap_Level1_MappingB = MappingB;
            Gap_SetNameLists();
        }
        public void Gap_reMap_Level2(DowntimeField MappingA, DowntimeField MappingB)
        {
            Gap_Level2_MappingA = MappingA;
            Gap_Level2_MappingB = MappingB;
            Gap_SetNameLists();
        }

        #endregion

        #endregion

    }
}
