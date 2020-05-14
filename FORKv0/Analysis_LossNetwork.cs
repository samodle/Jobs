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

        #region Loss Network
        public bool LossNetwork_isDemoMode = true;
        #region Variables & Properties
        public List<SystemSummaryReport> LossNetwork_analysisData = new List<SystemSummaryReport>();

        //  private List<int> LossNetwork_DependenciesPerLine = new List<int>();
        public double LossNetwork_MaxOEELoss(int targetIndex) { return LossNetwork_LossOEE[targetIndex].Max(); }
        public List<double> LossNetwork_MaxDependency { get; set; } = new List<double>();
        public List<List<string>> LossNetwork_LossNames { get; set; } = new List<List<string>>(); //analysis period data
        public List<List<double>> LossNetwork_LossOEE { get; set; } = new List<List<double>>();
        public List<List<double>> LossNetwork_LossStops { get; set; } = new List<List<double>>();
        //int-> index of pre dependency, double - > strength of dependency //if no dependencies <-1,0.0>
        //List Structure -> Line<PreFailure<PostFailures>>
        public List<List<List<Tuple<int, double>>>> LossNetwork_Dependencies { get; set; } = new List<List<List<Tuple<int, double>>>>();
        #endregion

        internal void LossNetwork_initialize()
        {
            List<List<DependencyEvent>> rawDepObject = new List<List<DependencyEvent>>();

            // reset data 
            LossNetwork_LossOEE.Clear();
            LossNetwork_LossNames.Clear();
            LossNetwork_analysisData.Clear();
            LossNetwork_Dependencies.Clear();
            LossNetwork_MaxDependency.Clear();
            LossNetwork_LossStops.Clear();
            //  LossNetwork_DependenciesPerLine.Clear();

            //set analysis data & dependency data from master lists
            for (int i = 0; i < Multi_CurrentLineNames.Count; i++) //get mapped lists to analysis reports
            {
                int lineIndex = Multi_AllSystemReports_Names.IndexOf(Multi_CurrentLineNames[i]);
                //remap data & set analysis lists
                Multi_AllSystemReports[lineIndex].reMapDowntime(LossNetwork_Mapping_A, LossNetwork_Mapping_B);
                LossNetwork_analysisData.Add(Multi_AllSystemReports[lineIndex].getSubset(this.startTime, this.endTime));

                //figure out the dependencies
                var tmpNameList = new List<string>();
                for (int j = 0; j < Multi_AllSystemReports[lineIndex].rawData.Count; j++)
                {
                    tmpNameList.Add(Multi_AllSystemReports[lineIndex].rawData[j].getFieldFromInteger(LossNetwork_Mapping_A, LossNetwork_Mapping_B));
                }
                rawDepObject.Add(DependencyAnalysis.executeDependencyAnalysis(tmpNameList));

                //find max dependencies
                double tmpMaxVal = 0;
                for (int j = 0; j < rawDepObject[i].Count; j++)
                {
                    if (rawDepObject[i][j].ActExp_Pct > tmpMaxVal)
                    {
                        tmpMaxVal = rawDepObject[i][j].ActExp_Pct;
                    }
                }
                LossNetwork_MaxDependency.Add(tmpMaxVal);

                //setup loss names & OEE losses from analysis period data
                var tmpLossNames = new List<string>();
                var tmpOEEs = new List<double>();
                var tmpStops = new List<double>();

                for (int j = 0; j < LossNetwork_analysisData[i].DT_Report.MappedDirectory.Count; j++)
                {
                    tmpLossNames.Add(LossNetwork_analysisData[i].DT_Report.MappedDirectory[j].Name);
                    tmpOEEs.Add(LossNetwork_analysisData[i].DT_Report.MappedDirectory[j].DT / LossNetwork_analysisData[i].schedTime);
                    tmpStops.Add(LossNetwork_analysisData[i].DT_Report.MappedDirectory[j].Stops);
                }
                for (int j = 0; j < LossNetwork_analysisData[i].DT_Report.MappedDirectory_Planned.Count; j++)
                {
                    tmpLossNames.Add(LossNetwork_analysisData[i].DT_Report.MappedDirectory_Planned[j].Name);
                    tmpOEEs.Add(LossNetwork_analysisData[i].DT_Report.MappedDirectory_Planned[j].DT / LossNetwork_analysisData[i].schedTime);
                    tmpStops.Add(LossNetwork_analysisData[i].DT_Report.MappedDirectory_Planned[j].Stops);
                }
                LossNetwork_LossNames.Add(tmpLossNames);
                LossNetwork_LossOEE.Add(tmpOEEs);
                LossNetwork_LossStops.Add(tmpStops);

                //now parse rawdepobject to get our actual usable values
                var tmpLineDepList = new List<List<Tuple<int, double>>>();
                for (int eventInc = 0; eventInc < tmpLossNames.Count; eventInc++)
                {
                    string testName = tmpLossNames[eventInc];
                    var tmpList = new List<Tuple<int, double>>();
                    for (int j = 0; j < rawDepObject[i].Count; j++)
                    {
                        if (testName == rawDepObject[i][j].PreStopFailureMode)
                        {
                            tmpList.Add(new Tuple<int, double>(tmpLossNames.IndexOf(rawDepObject[i][j].PostStopFailureMode), rawDepObject[i][j].ActExp_Pct));
                        }
                    }
                    if (tmpList.Count == 0) { tmpList.Add(new Tuple<int, double>(-1, 0.0)); }
                    tmpLineDepList.Add(tmpList);
                }
                LossNetwork_Dependencies.Add(tmpLineDepList);
            }
        }


        #region Mapping
        public DowntimeField LossNetwork_Mapping_A { get; set; } = DowntimeField.Fault;
        public DowntimeField LossNetwork_Mapping_B { get; set; } = DowntimeField.NA;
        public void LossNetwork_ReMap(string MappingA, string MappingB = "")
        {
            LossNetwork_ReMap(getEnumForString(MappingA), getEnumForString(MappingB));
        }
        public void LossNetwork_ReMap(DowntimeField MappingA, DowntimeField MappingB)
        {
            LossNetwork_Mapping_A = MappingA;
            LossNetwork_Mapping_B = MappingB;
            LossNetwork_initialize(); //remapping happens in initialization!
        }
        #endregion
        #endregion

    }
}
