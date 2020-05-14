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
        #region Multiline - Shared
        //see if we have data for the lines we're looking at
        //!!! -> also sets active lines!
        public List<string> Multi_getDataNeededLines(List<string> LineNames)
        {
            var returnList = new List<string>();
            int x;
            for (int i = 0; i < LineNames.Count; i++)
            {
                x = Multi_AllLinesWithData.IndexOf(LineNames[i]);
                if (i > -1) { returnList.Add(LineNames[i]); }
            }

            //now do the active line settings!!
            Multi_CurrentLineNames.Clear();
            for (int i = 0; i < LineNames.Count; i++)
            {
                Multi_CurrentLineNames.Add(LineNames[i]);
            }
            /////
            return returnList;
        }

        //add data for given lines
        //assumes only line we dont have data for yet
        //!!! ->sets active lines
        public void Multi_AddDataForNewLines(List<List<DTevent>> newDataList, List<string> LineNames, ProductionLines.LineConfig CurrentLineConfig)
        {
            //get all dtevents into one list
            var newMasterDTeventList = new List<DTevent>();
            if (newDataList.Count > 0)
            {
                for (int i = 0; i < LineNames.Count; i++)
                {
                    if (Multi_AllLinesWithData.IndexOf(LineNames[i]) == -1) { Multi_AllLinesWithData.Add(LineNames[i]); }
                    for (int j = 0; j < newDataList[i].Count; j++)
                    {
                        newMasterDTeventList.Add(newDataList[i][j].getCopy());
                    }
                }
            }
            //add in the existing stuff
            for (int i = 0; i < rawData.DT_Report.rawDTdata.rawConstraintData.Count; i++)
            {
                newMasterDTeventList.Add(rawData.DT_Report.rawDTdata.rawConstraintData[i].getCopy());
            }

            //do the active line check!!
            for (int i = 0; i < newMasterDTeventList.Count; i++)
            {
                string tmpString = newMasterDTeventList[i].ParentLineName;
                int x = Multi_CurrentLineNames.IndexOf(tmpString);
                if (x == -1)
                {
                    newMasterDTeventList[i].isFiltered_ParentLine = true;
                }
                else
                {
                    newMasterDTeventList[i].isFiltered_ParentLine = false;
                }
            }

            //create the master directories
            var tmpDTeventList = new List<List<DTevent>>();
            var tmpLineNames = new List<string>();
            int tmpIndex;
            Multi_CurrentSystemReports.Clear();
            for (int i = 0; i < newMasterDTeventList.Count; i++)
            {
                tmpIndex = tmpLineNames.IndexOf(newMasterDTeventList[i].ParentLineName);
                if (tmpIndex == -1)
                {
                    tmpLineNames.Add(newMasterDTeventList[i].ParentLineName);
                    var tmpList = new List<DTevent>();
                    tmpList.Add(newMasterDTeventList[i].getCopy());
                    tmpDTeventList.Add(tmpList);
                }
                else
                {
                    tmpDTeventList[tmpIndex].Add(newMasterDTeventList[i].getCopy());
                }
            }
            Multi_CurrentSystemReports_Names = tmpLineNames;

            for (int i = 0; i < tmpLineNames.Count; i++)
            {
                var tmpInterface = new downtimeInterface(CurrentLineConfig, tmpDTeventList[i]);
                var tmpDTreport = new SystemDowntimeReport(tmpInterface);
                Multi_CurrentSystemReports.Add(new SystemSummaryReport(tmpDTreport));
            }


            //add to the existing list
            var DTinterface = new downtimeInterface(CurrentLineConfig, newMasterDTeventList);
            var DTreport = new SystemDowntimeReport(DTinterface);
            this.rawData = new SystemSummaryReport(DTreport);

            this.AnalysisPeriodData = rawData.getSubset(this.startTime, this.endTime);
            initializeLossCompass();
            updateCurrentFromMaster(CardTier.A);

            //update the All Data Lists
            for (int i = 0; i < LineNames.Count; i++)
            {
                if (Multi_AllSystemReports_Names.IndexOf(LineNames[i]) == -1)
                {
                    //need to check this
                    Multi_AllSystemReports_Names.Add(LineNames[i]);
                    var tmpInterface = new downtimeInterface(CurrentLineConfig, tmpDTeventList[i]);
                    var tmpDTreport = new SystemDowntimeReport(tmpInterface);
                    var tmpSummaryReport = new SystemSummaryReport(tmpDTreport);
                    Multi_AllSystemReports.Add(tmpSummaryReport);
                    Multi_AllSystemReports_Daily.Add(tmpSummaryReport.getPeriodicSubsets(new TimeSpan(hours: 24, minutes: 0, seconds: 0)));
                    List<Tuple<DateTime, DateTime>> monthDates = tmpSummaryReport.getMonthStartEndDates();
                    Multi_AllSystemReports_Weekly.Add(tmpSummaryReport.getPeriodicSubsets(new TimeSpan(hours: 24 * 7, minutes: 0, seconds: 0)));
                    var tmpReportList = new List<SystemSummaryReport>();
                    for (int j = 0; j < monthDates.Count; j++)
                    {
                        tmpReportList.Add(tmpSummaryReport.getSubset(monthDates[j].Item1, monthDates[j].Item2));
                    }
                    Multi_AllSystemReports_Monthly.Add(tmpReportList);
                }
            }
        }


        //update loss chart based on tooltip
        public List<DTeventSummary> Multi_UpdateLossModeGraphs(CardTier selectedTier, string LossName)
        {
            List<DTeventSummary> tmpList = new List<DTeventSummary>();
            List<string> tmpNames = new List<string>();
            List<DowntimeField> listA = new List<DowntimeField>();
            List<DowntimeField> listB = new List<DowntimeField>();
            switch (selectedTier)
            {
                case CardTier.A:
                    if (TopLevelSelection == TopLevelSelected.Unplanned)
                    {
                        listA = LossCompass_MasterMappingAList.GetRange(0, TierA_Level);
                        listA.Add(TierA_Mapping_A);
                        listB = LossCompass_MasterMappingBList.GetRange(0, TierA_Level);
                        listB.Add(TierA_Mapping_B);
                        tmpNames = LossCompass_ActiveNames.GetRange(0, TierA_Level);
                        tmpNames.Add(LossName);
                        tmpList = AnalysisPeriodData.DT_Report.getMappedSubdirectoryForGivenHierarchy(listA, listB, tmpNames, DowntimeField.ParentLine, DowntimeField.NA);
                    }
                    else
                    {
                        tmpList = AnalysisPeriodData.DT_Report.getMappedSubdirectoryForGivenHierarchy_Planned(LossCompass_MasterMappingAList.GetRange(0, TierA_Level), LossCompass_MasterMappingBList.GetRange(0, TierA_Level), LossCompass_ActiveNames.GetRange(0, TierA_Level), DowntimeField.ParentLine, DowntimeField.NA);
                    }
                    break;
                case CardTier.B:
                    if (TopLevelSelection == TopLevelSelected.Unplanned)
                    {
                        tmpList = AnalysisPeriodData.DT_Report.getMappedSubdirectoryForGivenHierarchy(LossCompass_MasterMappingAList.GetRange(0, TierB_Level + 1), LossCompass_MasterMappingBList.GetRange(0, TierB_Level + 1), LossCompass_ActiveNames.GetRange(0, TierB_Level + 1), DowntimeField.ParentLine, DowntimeField.NA);
                    }
                    else
                    {
                        tmpList = AnalysisPeriodData.DT_Report.getMappedSubdirectoryForGivenHierarchy_Planned(LossCompass_MasterMappingAList.GetRange(0, TierB_Level + 1), LossCompass_MasterMappingBList.GetRange(0, TierB_Level + 1), LossCompass_ActiveNames.GetRange(0, TierB_Level + 1), DowntimeField.ParentLine, DowntimeField.NA);
                    }
                    break;
                case CardTier.C: //we're going to at least 4 tiers!
                    if (TopLevelSelection == TopLevelSelected.Unplanned)
                    {
                        tmpList = AnalysisPeriodData.DT_Report.getMappedSubdirectoryForGivenHierarchy(LossCompass_MasterMappingAList.GetRange(0, TierC_Level + 1), LossCompass_MasterMappingBList.GetRange(0, TierC_Level + 1), LossCompass_ActiveNames.GetRange(0, TierC_Level), DowntimeField.ParentLine, DowntimeField.NA);
                    }
                    else
                    {
                        tmpList = AnalysisPeriodData.DT_Report.getMappedSubdirectoryForGivenHierarchy_Planned(LossCompass_MasterMappingAList.GetRange(0, TierC_Level + 1), LossCompass_MasterMappingBList.GetRange(0, TierC_Level + 1), LossCompass_ActiveNames.GetRange(0, TierC_Level), DowntimeField.ParentLine, DowntimeField.NA);
                    }
                    break;
            }

            for (int i = 0; i < tmpList.Count; i++)
            {
                tmpList[i].SchedTime = AnalysisPeriodData.schedTime;
                tmpList[i].UT = AnalysisPeriodData.UT;
            }

            return tmpList;
        }



        //names showing which data we have
        public List<string> Multi_CurrentSystemReports_Names = new List<string>(); //names for the reports
        public List<SystemSummaryReport> Multi_CurrentSystemReports = new List<SystemSummaryReport>(); //current data we have

        public List<string> Multi_AllSystemReports_Names = new List<string>(); //names of lines in other reports
        public List<SystemSummaryReport> Multi_AllSystemReports = new List<SystemSummaryReport>();
        internal List<List<SystemSummaryReport>> Multi_AllSystemReports_Daily = new List<List<SystemSummaryReport>>();
        public List<List<SystemSummaryReport>> Multi_AllSystemReports_Weekly = new List<List<SystemSummaryReport>>();
        public List<List<SystemSummaryReport>> Multi_AllSystemReports_Monthly = new List<List<SystemSummaryReport>>();

        public void Multi_RemapSystemReports(DowntimeField MappingA, DowntimeField MappingB)
        {
            for (int i = 0; i < Multi_AllSystemReports.Count; i++)
            {
                Multi_AllSystemReports[i].reMapDowntime(MappingA, MappingB);
            }
            for (int i = 0; i < Multi_AllSystemReports_Daily.Count; i++)
            {
                for (int j = 0; j < Multi_AllSystemReports_Daily[i].Count; j++)
                {
                    Multi_AllSystemReports_Daily[i][j].reMapDowntime(MappingA, MappingB);
                }
            }
            for (int i = 0; i < Multi_AllSystemReports_Weekly.Count; i++)
            {
                for (int j = 0; j < Multi_AllSystemReports_Weekly[i].Count; j++)
                {
                    Multi_AllSystemReports_Weekly[i][j].reMapDowntime(MappingA, MappingB);
                }
            }
            for (int i = 0; i < Multi_AllSystemReports_Monthly.Count; i++)
            {
                for (int j = 0; j < Multi_AllSystemReports_Monthly[i].Count; j++)
                {
                    Multi_AllSystemReports_Monthly[i][j].reMapDowntime(MappingA, MappingB);
                }
            }
        }


        public List<string> Multi_CurrentLineNames { get; set; } = new List<string>(); //lines we're looking at right now
        public List<string> Multi_AllLinesWithData { get; set; } = new List<string>(); //all lines we have data for
        #endregion

        #region Shared & Top Line Variables
        #region Top Line 
        public string Name { get; set; }
        public DateTime startTime { get; set; }
        public DateTime endTime { get; set; }
        public double primaryKPI1_Value { get; set; }
        public double primaryKPI2_Value { get; set; }
        public double primaryKPI3_Value { get; set; }
        #endregion

        private SystemSummaryReport rawData;
        internal SystemSummaryReport AnalysisPeriodData;
        private List<SystemSummaryReport> DailyReports { get; set; } = new List<SystemSummaryReport>();

        #endregion

        #region Constructor
        public Dashboard_Intermediate_Single(SystemSummaryReport rawData, string Name, DateTime startDate, DateTime endDate)
        {
            this.Name = Name;
            Multi_CurrentLineNames.Add(Name);
            Multi_AllLinesWithData.Add(Name);
            this.rawData = rawData;
            AnalysisPeriodData = rawData.getSubset(startDate, endDate);

            Multi_AllSystemReports_Names.Add(Name);
            Multi_AllSystemReports.Add(rawData);
            Multi_AllSystemReports_Daily.Add(rawData.getPeriodicSubsets(new TimeSpan(hours: 24, minutes: 0, seconds: 0)));
            List<Tuple<DateTime, DateTime>> monthDates = rawData.getMonthStartEndDates();
            Multi_AllSystemReports_Weekly.Add(rawData.getPeriodicSubsets(new TimeSpan(hours: 24 * 7, minutes: 0, seconds: 0)));
            var tmpReportList = new List<SystemSummaryReport>();
            for (int j = 0; j < monthDates.Count; j++)
            {
                tmpReportList.Add(rawData.getSubset(monthDates[j].Item1, monthDates[j].Item2));
            }
            Multi_AllSystemReports_Monthly.Add(tmpReportList);

            //Top Line
            this.startTime = AnalysisPeriodData.startTime;
            this.endTime = AnalysisPeriodData.endTime;
        }

        #endregion

        #region Shared Functions
        /* System Summary Report -> double */
        public static List<List<double>> getDoubleForMetricsFromListOfSystemSummarys(List<SystemSummaryReport> rawList, List<DowntimeMetrics> Metrics, string Name, bool isUnplanned, DowntimeField Field = DowntimeField.NA)
        {
            var returnList = new List<List<double>>();

            //set up the metric lists
            for (int i = 0; i < Metrics.Count; i++)
            {
                var x = new List<double>();
                returnList.Add(x);
            }

            for (int i = 0; i < rawList.Count; i++) //get the approprate DTevents
            {
                int tmpIndex = isUnplanned ? rawList[i].DT_Report.MappedDirectory.IndexOf(new DTeventSummary(Name, Field)) : rawList[i].DT_Report.MappedDirectory_Planned.IndexOf(new DTeventSummary(Name, Field));
                //add the appropriate values
                if (tmpIndex > -1)
                {
                    double ST = rawList[i].schedTime;
                    double UT = rawList[i].UT;
                    for (int j = 0; j < returnList.Count; j++)
                    {
                        double tmpVal = isUnplanned ? rawList[i].DT_Report.MappedDirectory[tmpIndex].getKPI(Metrics[j], ST, UT) : rawList[i].DT_Report.MappedDirectory_Planned[tmpIndex].getKPI(Metrics[j], ST, UT);
                        returnList[j].Add(tmpVal);
                    }
                }
                else
                {
                    for (int j = 0; j < returnList.Count; j++)
                    {
                        returnList[j].Add(0);
                    }
                }
            }
            return returnList;
        }


        /* DTevent -> double */
        public static List<double> getDoubleForMetricFromListOfDTevents(ref List<List<DTeventSummary>> rawList, DowntimeMetrics Metric, DowntimeField Field, string Name)
        {
            var eventList = new List<DTeventSummary>();
            var metricList = new List<double>();
            int tmpIndex;
            for (int i = 0; i < rawList.Count; i++) //get the approprate DTevents
            {
                eventList = rawList[i];
                tmpIndex = eventList.IndexOf(new DTeventSummary(Name, Field));
                if (tmpIndex > -1)
                {
                    metricList.Add(eventList[tmpIndex].getKPI(Metric));
                }
                else { metricList.Add(0); }
            }
            return metricList;
        }
        #endregion
    }
}
