using System;
using System.Collections.Generic;
using Windows_Desktop;
using Windows_Desktop.Properties;
using System.Windows.Forms;
using System.Diagnostics;
using System.Linq;
using static ForkAnalyticsSettings.GlobalConstants;

namespace DataInterface
{
    public class SystemSummaryReport
    {
        //Mapped Directory Access
        public double MappedDirector_Unplanned_DTpct(int Index)
        {
            return this.schedTime == 0 ? 0 : DT_Report.MappedDirectory[Index].DT / this.schedTime;
        }
        public double MappedDirector_Planned_DTpct(int Index)
        {
            return this.schedTime == 0 ? 0 : DT_Report.MappedDirectory_Planned[Index].DT / this.schedTime;
        }

        public double MappedDirector_Unplanned_MTBF(int Index)
        {
            return this.UT == 0 ? 0 : this.UT / DT_Report.MappedDirectory[Index].Stops;
        }
        public double MappedDirector_Planned_MTBF(int Index)
        {
            return this.UT == 0 ? 0 : this.UT / DT_Report.MappedDirectory_Planned[Index].Stops;
        }



        #region Variables
        public List<DTevent> rawUnplannedData { get { return DT_Report.rawDTdata.UnplannedData; } }
        public List<DTevent> rawData { get { return DT_Report.rawDTdata.rawConstraintData; } }

        internal SystemDowntimeReport DT_Report;

        internal SystemProductionReport PROD_Report;
        private productionLine ParentLine { get; set; }

        public DateTime startTime { get; set; }
        public DateTime endTime { get; set; }
        //these are the filtered criteria for this report

        public List<string> Brandcodes;
        public List<string> Teams;
        public List<string> Shifts;

        public List<string> Products;
        //all known fields
        //production based
        public List<string> BrandCodeReport = new List<string>();
        //dt based
        public List<string> ShiftReport = new List<string>();
        //dt based
        public List<string> TeamReport = new List<string>();
        //dt based
        public List<string> ProductReport = new List<string>();

        //properties
        public double OEE
        {
            get
            {
                // if (Settings.Default.AdvancedSettings_isAvailabilityMode)
                return (schedTime > 0) ? UT / schedTime : 0;
                //  return PROD_Report.PR;
            }
        }
        public double schedTime
        {
            get
            {
                if (Settings.Default.AdvancedSettings_isAvailabilityMode)
                    return DT_Report.schedTime;
                return PROD_Report.schedTime;
            }
        }
        public double schedTimeDT
        {
            get { return DT_Report.schedTime; }
        }
        public double UT
        {
            get
            {
                if (Settings.Default.AdvancedSettings_isAvailabilityMode)
                    return DT_Report.UT;
                return PROD_Report.UT;
            }
        }
        public double UT_DT
        {
            get { return DT_Report.UT; }
        }
        public double UPDT
        {
            get { return DT_Report.UPDT; }
        }
        public double PDT
        {
            get { return DT_Report.PDT; }
        }
        public double UPDTpct
        {
            get
            {
                if (schedTime == 0)
                    return 0;
                return DT_Report.UPDT / schedTime;
            }
        }
        public double PDTpct
        {
            get
            {
                if (schedTime == 0)
                    return 0;
                return DT_Report.PDT / schedTime;
            }
        }
        public double RateLoss
        {

            get { return 1.0 - OEE - UPDTpct - PDTpct; }
        }
        public double RateLossPct
        {
            get
            {
                if (schedTime == 0)
                {
                    return 0;
                }
                else
                {
                    return RateLoss;
                    /// schedTime ' LG Code
                }
            }
        }
        public double Stops
        { get { return DT_Report.Stops; } }
        public double ActualCases
        {
            get
            {
                if (Settings.Default.AdvancedSettings_isAvailabilityMode)
                    return 0;
                return PROD_Report.CasesActual;
            }
        }
        public double AdjustedCases
        {
            get
            {
                if (Settings.Default.AdvancedSettings_isAvailabilityMode)
                    return 0;
                return PROD_Report.CasesAdjusted;
            }
        }
        public double MTBF { get { return (Stops == 0) ? 0 : UT / Stops; } }
        public double MTTR { get { return (Stops == 0) ? 0 : UPDT / Stops; } }
        public double SPD { get { return (schedTime == 0) ? 0 : Stops / schedTime * 1440; } }

        #endregion

        #region Filter
        public void FilterDowntimeByField(DowntimeField Field, List<string> inclusionList)
        {
            switch (Field)
            {
                case DowntimeField.ProductGroup: reFilterDowntime_ProductGroup(inclusionList); break;
                //PROBLEM!!!     case DowntimeField.ProductCode: reFilterDowntime_ProductCode(inclusionList); break;
                case DowntimeField.Team: reFilterDowntime_Team(inclusionList); break;
                case DowntimeField.Format: reFilterDowntime_Format(inclusionList); break;
            }
        }
        public void reFilterDowntime_ProductGroup(List<string> inclusionList)
        {
            // If Not Settings.Default.AdvancedSettings_isAvailabilityMode = True Then PROD_Report = New SystemProductionReport(ParentLine, startTime, endTime, inclusionList, (int)DowntimeField.ProductGroup)
            DT_Report.reFilterData_ProductGroup(inclusionList);
        }
 
        public void reFilterDowntime_Team(List<string> inclusionList)
        {
            // if (!(Settings.Default.AdvancedSettings_isAvailabilityMode == true))
            //     PROD_Report = new SystemProductionReport(ParentLine, startTime, endTime, inclusionList, (int)DowntimeField.Team);
            DT_Report.reFilterData_Team(inclusionList);
        }
        public void reFilterDowntime_Format(List<string> inclusionList)
        {
            if (!(Settings.Default.AdvancedSettings_isAvailabilityMode == true))
                PROD_Report = new SystemProductionReport(ParentLine, startTime, endTime);
            DT_Report.reFilterData_Format(inclusionList);
        }

        #endregion

        #region Subset
        public List<SystemSummaryReport> getPeriodicSubsets(TimeSpan tPeriod)
        {
            List<SystemSummaryReport> reportList = new List<SystemSummaryReport>();
            DateTime tmpStartTime = startTime;
            DateTime tmpEndTime = this.startTime + tPeriod;
            if (tmpEndTime > this.endTime)
            {
                reportList.Add(this);
            }
            else
            {
                while (tmpEndTime < this.endTime)
                {
                    reportList.Add(this.getSubset(tmpStartTime, tmpEndTime));
                    tmpStartTime += tPeriod;
                    tmpEndTime += tPeriod;
                }
            }
            return reportList;
        }
        public SystemSummaryReport getSubset(DateTime startTime, DateTime endTime)
        {
            SystemDowntimeReport tmpReport = DT_Report.getSubset(new DateTime(Math.Max(this.startTime.Ticks, startTime.Ticks)), new DateTime(Math.Min(this.endTime.Ticks, endTime.Ticks)));
            return new SystemSummaryReport(tmpReport);
        }
        #endregion

        #region Mapping
        public void setMappedSchedTime()
        {
            for (int i = 0; i < DT_Report.MappedDirectory.Count; i++)
            {
                DT_Report.MappedDirectory[i].SchedTime = this.schedTime;
                DT_Report.MappedDirectory[i].UT = this.UT;
            }
            for (int i = 0; i < DT_Report.MappedDirectory_Planned.Count; i++)
            {
                DT_Report.MappedDirectory_Planned[i].SchedTime = this.schedTime;
                DT_Report.MappedDirectory_Planned[i].UT = this.UT;
            }
        }

        public void reMapDowntime(DowntimeField MappingA, DowntimeField MappingB)
        {
            DT_Report.reMapDataSet(MappingA, MappingB);
        }

        #endregion

        #region Construction
        public SystemSummaryReport(SystemDowntimeReport DTreport)
        {
            startTime = DTreport.StartTime;
            endTime = DTreport.EndTime;
            this.DT_Report = DTreport;
        }
        public SystemSummaryReport(productionLine InparentLine, ref DateTime reportStartTime, ref DateTime reportEndTime)
        {
            // Dim tmpHr As Double
            ParentLine = InparentLine;

            startTime = reportStartTime;
            endTime = reportEndTime;


            if (!Settings.Default.AdvancedSettings_isAvailabilityMode)
            {

            }

            DT_Report = new SystemDowntimeReport(ParentLine, startTime, endTime);

            reportStartTime = startTime;
            reportEndTime = endTime;
        }
        #endregion

        public List<Tuple<DateTime, DateTime>> getMonthStartEndDates()
        {
            var dateList = new List<Tuple<DateTime, DateTime>>();
            var testTime = this.startTime;
            while (testTime <= this.endTime)
            {
                Tuple<DateTime, DateTime> newMonth = GlobalFcns.getMonthStartEndTimes(testTime);
                dateList.Add(newMonth);
                testTime = newMonth.Item2.AddDays(2);
            }

            return dateList;
        }

        public double getKPIforMetric(DowntimeMetrics Metric)
        {
            switch (Metric)
            {
                case DowntimeMetrics.MTBF:
                    return MTBF;
                case DowntimeMetrics.MTTR:
                    return MTTR;
                case DowntimeMetrics.NumChangeovers:
                    return Math.Max(DT_Report.ActiveGCAS.Count - 1, 0);
                case DowntimeMetrics.OEE:
                    return OEE * 100;
                case DowntimeMetrics.PDTpct:
                    return PDTpct * 100;
                case DowntimeMetrics.SchedTime:
                    return schedTime;
                case DowntimeMetrics.SKUs:
                    return DT_Report.ActiveGCAS.Count;
                case DowntimeMetrics.SPD:
                    return SPD;
                case DowntimeMetrics.Stops:
                    return Stops;
                case DowntimeMetrics.UnitsProduced:
                    return ActualCases;
                case DowntimeMetrics.UPDTpct:
                    return UPDTpct * 100;
                default: return 0;
            }
        }

        public override string ToString()
        {
            return "S/E: " + startTime + "-" + endTime + " Sched: " + Math.Round(schedTime, 2) + " OEE/Stop: " + Math.Round(OEE * 100, 1) + "/" + Math.Round(Stops, 1);
        }
    }




    public class SystemProductionReport
    {
        private DateTime _startTime;

        private DateTime _endTime;
        private productionLine parentLine;
        private object[,] _rawProdData;

        //private forkProductionData _rawProductionData;
        // = 0
        private double _schedTime;
        // = 0
        private double _uptimeCalc;
        //DO NOT USE THIS!!!!!
        private double _PR = 0;
        // = 0
        private double _actCases;
        // = 0
        private double _adjCases;
        // = 0
        private double _statUnits;
        // = 0
        private double _adjUnits;

        //sorting parameters
        private List<string> _Brandcodes;
        private List<string> _Products;
        private bool isFilter = false;
        private bool isFilterProducts = false;

        private bool isFilterBrandcodes = false;

        public double schedTime
        {
            get { return _schedTime; }
        }
        public double UT
        {
            get { return _uptimeCalc; }
        }
        public double PR
        {
            get
            {
                if (_schedTime == 0)
                    return 0;
                return _uptimeCalc / _schedTime;
            }
        }
        public double CasesActual
        {
            get { return _actCases; }
        }
        public double CasesAdjusted
        {
            get { return _adjCases; }
        }
        public double UnitsAdjusted
        {
            get { return _adjUnits; }
        }
        public double UnitsStat
        {
            get { return _statUnits; }
        }


        #region Construction

        public SystemProductionReport(productionLine iParentLine, DateTime startTime, DateTime endTime)
        {
            _rawProdData = iParentLine.rawProficyProductionData;
            _startTime = startTime;
            _endTime = endTime;
            parentLine = iParentLine;

            getAllProductionMetrics();
            // Call executeProductionReport()
        }
        #endregion


        private void getAllProductionMetrics()
        {
            _uptimeCalc = 0;
            _schedTime = 0;
            _actCases = 0;
            _adjCases = 0;
            _adjUnits = 0;
            _statUnits = 0;
        }



    }
    public class SystemDowntimeReport
    {
        #region Variables
        public double UPDT { get; set; } = 0;
        public double PDT { get; set; } = 0;
        public double UT { get; set; } = 0;
        public double DT { get; set; } = 0;
        public double excludedTime { get; set; } = 0;
        public double schedTime { get; set; } = 0;
        public long rateLossEvents { get; set; } = 0;
        public long excludedStops { get; set; } = 0;

        private downtimeInterface _rawDTData;
        internal downtimeInterface rawDTdata
        {
            get { return _rawDTData; }
        }

        public DateTime StartTime
        {
            get { return _rawDTData.StartDate; }
        }
        public DateTime EndTime
        {
            get { return _rawDTData.EndDate; }
        }
        public long Stops
        {
            get { return _rawDTData.Stops - rateLossEvents - excludedStops; }
        }

        #endregion

        #region Lists
        internal List<DTeventSummary> FaultDirectory = new List<DTeventSummary>();

        internal List<DTeventSummary> Reason1Directory = new List<DTeventSummary>();
        internal List<DTeventSummary> LocationDirectory = new List<DTeventSummary>();
        internal List<DTeventSummary> Reason2Directory = new List<DTeventSummary>();
        internal List<DTeventSummary> Reason3Directory = new List<DTeventSummary>();
        internal List<DTeventSummary> Reason4Directory = new List<DTeventSummary>();

        internal List<DTeventSummary> Tier1Directory = new List<DTeventSummary>();

        internal List<DTeventSummary> PlannedTier1Directory = new List<DTeventSummary>();

        internal List<DTeventSummary> MappedDirectory = new List<DTeventSummary>();
        internal List<DTeventSummary> MappedDirectory_Planned = new List<DTeventSummary>();
        //Fields We Filter By
        internal List<string> ActiveGCAS = new List<string>();
        internal List<string> ActiveProducts = new List<string>();
        internal List<string> ActiveTeams = new List<string>();
        internal List<string> ActiveShapes = new List<string>();
        internal List<string> ActiveFormats = new List<string>();

        internal List<string> ActiveProductGroups = new List<string>();
        public List<string> getFilterList(DowntimeField FilterField)
        {
            var tmpList = new List<string>();
            int i = 0;
            switch (FilterField)
            {
                case DowntimeField.Product:
                    for (i = 0; i <= ActiveProducts.Count - 1; i++)
                    {
                        tmpList.Add(ActiveProducts[i]);
                    }

                    break;
                case DowntimeField.Shape:
                    for (i = 0; i <= ActiveShapes.Count - 1; i++)
                    {
                        tmpList.Add(ActiveShapes[i]);
                    }
                    break;
                case DowntimeField.Format:
                    for (i = 0; i <= ActiveFormats.Count - 1; i++)
                    {
                        tmpList.Add(ActiveFormats[i]);
                    }
                    break;
                case DowntimeField.Team:
                    for (i = 0; i <= ActiveTeams.Count - 1; i++)
                    {
                        tmpList.Add(ActiveTeams[i]);
                    }
                    break;
                case DowntimeField.ProductGroup:
                    for (i = 0; i <= ActiveProductGroups.Count - 1; i++)
                    {
                        tmpList.Add(ActiveProductGroups[i]);
                    }

                    break;
                default:
                    throw new CustomExceptions.unknownMappingException();
            }
            return tmpList;
        }



        #endregion

        #region Get Subdirectories For Mapped Reason Level
        private string getMappedName(DTevent sourceEvent, DowntimeField MappingA, DowntimeField MappingB)
        {
            if (MappingB != DowntimeField.NA)
            {
                return sourceEvent.getFieldFromInteger(MappingA) + "-" + sourceEvent.getFieldFromInteger(MappingB);
            }
            else
            {
                return sourceEvent.getFieldFromInteger(MappingA);
            }
        }
        private bool doesEventMatchHierarchy(DTevent sourceEvent, List<DowntimeField> ParentMapping_A, List<DowntimeField> ParentMapping_B, List<string> Parent_Fields)
        {
            for (int i = 0; i < Parent_Fields.Count; i++)
            {
                if (getMappedName(sourceEvent, ParentMapping_A[i], ParentMapping_B[i]) != Parent_Fields[i]) { return false; }
            }
            return true;
        }

        public List<DTeventSummary> getMappedSubdirectoryForGivenHierarchy1(DowntimeField ParentMapping_A, DowntimeField ParentMapping_B, string ParentField, DowntimeField OutputMapping_A, DowntimeField OutputMapping_B)
        {
            //var a = ;
            return getMappedSubdirectoryForGivenHierarchy(new List<DowntimeField>() { ParentMapping_A }, new List<DowntimeField> { ParentMapping_B }, new List<string> { ParentField }, OutputMapping_A, OutputMapping_B);
        }
        public List<DTeventSummary> getMappedSubdirectoryForGivenHierarchy1_Planned(DowntimeField ParentMapping_A, DowntimeField ParentMapping_B, string ParentField, DowntimeField OutputMapping_A, DowntimeField OutputMapping_B)
        {
            //var a = ;
            return getMappedSubdirectoryForGivenHierarchy_Planned(new List<DowntimeField>() { ParentMapping_A }, new List<DowntimeField> { ParentMapping_B }, new List<string> { ParentField }, OutputMapping_A, OutputMapping_B);
        }


        public List<DTeventSummary> getMappedSubdirectoryForGivenHierarchy(List<DowntimeField> ParentMapping_A, List<DowntimeField> ParentMapping_B, List<string> Parent_Fields, DowntimeField OutPutMapping_A, DowntimeField OutPutMapping_B)
        {
            int i = 0;
            int tmpIndex = 0;
            string targetName;
            var tmpList = new List<DTeventSummary>();

            for (i = 0; i <= _rawDTData.UnplannedData.Count - 1; i++)
            {
                if (doesEventMatchHierarchy(_rawDTData.UnplannedData[i], ParentMapping_A, ParentMapping_B, Parent_Fields))
                {
                    targetName = getMappedName(_rawDTData.UnplannedData[i], OutPutMapping_A, OutPutMapping_B);
                    tmpIndex = tmpList.IndexOf(new DTeventSummary(targetName));
                    if (tmpIndex == -1)
                    {
                        tmpList.Add(new DTeventSummary(i, targetName, _rawDTData.UnplannedData[i].DT));
                    }
                    else
                    {
                        tmpList[tmpIndex].addStopWithRow(i, _rawDTData.UnplannedData[i].DT);
                    }
                }
            }
            return tmpList;
        }

        public List<DTeventSummary> getMappedSubdirectoryForGivenHierarchy_Planned(List<DowntimeField> ParentMapping_A, List<DowntimeField> ParentMapping_B, List<string> Parent_Fields, DowntimeField OutPutMapping_A, DowntimeField OutPutMapping_B)
        {
            int i = 0;
            int tmpIndex = 0;
            string targetName;
            var tmpList = new List<DTeventSummary>();

            for (i = 0; i <= _rawDTData.PlannedData.Count - 1; i++)
            {
                if (doesEventMatchHierarchy(_rawDTData.PlannedData[i], ParentMapping_A, ParentMapping_B, Parent_Fields))
                {
                    targetName = getMappedName(_rawDTData.PlannedData[i], OutPutMapping_A, OutPutMapping_B);
                    tmpIndex = tmpList.IndexOf(new DTeventSummary(targetName));
                    if (tmpIndex == -1)
                    {
                        tmpList.Add(new DTeventSummary(i, targetName, _rawDTData.PlannedData[i].DT));
                    }
                    else
                    {
                        tmpList[tmpIndex].addStopWithRow(i, _rawDTData.PlannedData[i].DT);
                    }
                }
            }
            return tmpList;
        }




        public List<DTeventSummary> getSubdirectoryForGivenField(DowntimeField sourceField, string sourceName, DowntimeField targetField, DowntimeField sourceField2 = DowntimeField.NA, string sourceName2 = "")
        {
            int i = 0;
            int tmpIndex = 0;
            string tmpName; string tmpName2; string targetName;
            var tmpList = new List<DTeventSummary>();
            /* Top Level OR 1 Level Lower */
            if (sourceField2 == DowntimeField.NA)
            {
                for (i = 0; i <= _rawDTData.UnplannedData.Count - 1; i++)
                {
                    tmpName = _rawDTData.UnplannedData[i].getFieldFromInteger(sourceField);
                    if ((tmpName == sourceName) || (sourceField == DowntimeField.NA))
                    {
                        targetName = _rawDTData.UnplannedData[i].getFieldFromInteger(targetField);
                        tmpIndex = tmpList.IndexOf(new DTeventSummary(targetName));
                        if (tmpIndex == -1)
                        {
                            tmpList.Add(new DTeventSummary(i, targetName, _rawDTData.UnplannedData[i].DT));
                        }
                        else
                        {
                            tmpList[tmpIndex].addStopWithRow(i, _rawDTData.UnplannedData[i].DT);
                        }
                    }
                }
            }
            /* > 1 Level Down */
            else
            {
                for (i = 0; i <= _rawDTData.UnplannedData.Count - 1; i++)
                {
                    tmpName = _rawDTData.UnplannedData[i].getFieldFromInteger(sourceField);
                    tmpName2 = _rawDTData.UnplannedData[i].getFieldFromInteger(sourceField2);
                    if ((tmpName == sourceName) && (tmpName2 == sourceName2))
                    {
                        targetName = _rawDTData.UnplannedData[i].getFieldFromInteger(targetField);
                        tmpIndex = tmpList.IndexOf(new DTeventSummary(targetName));
                        if (tmpIndex == -1)
                        {
                            tmpList.Add(new DTeventSummary(i, targetName, _rawDTData.UnplannedData[i].DT));
                        }
                        else
                        {
                            tmpList[tmpIndex].addStopWithRow(i, _rawDTData.UnplannedData[i].DT);
                        }
                    }
                }
            }
            /* c'est fin */
            return tmpList;
        }


        public List<DTeventSummary> getMappedSubdirectory(string MappedName, DowntimeField targetField)
        {
            switch (targetField)
            {
                case DowntimeField.Reason1:
                    return getMappedReason1Directory(MappedName);
                case DowntimeField.Reason2:
                    return getMappedReason2Directory(MappedName);
                case DowntimeField.Reason3:
                    return getMappedReason3Directory(MappedName);
                case DowntimeField.Reason4:
                    return getMappedReason4Directory(MappedName);
                case DowntimeField.Fault:
                    return getMappedFaultDirectory(MappedName);
                default:
                    throw new CustomExceptions.unknownMappingException();
            }
        }
        private List<DTeventSummary> getMappedFaultDirectory(string MappedName)
        {
            int i = 0;
            int tmpIndex = 0;
            List<DTeventSummary> tmpList = new List<DTeventSummary>();
            for (i = 0; i <= _rawDTData.UnplannedData.Count - 1; i++)
            {
                var _with2 = _rawDTData.UnplannedData[i];
                if (_with2.MappedField == MappedName)
                {
                    tmpIndex = tmpList.IndexOf(new DTeventSummary(_with2.Fault));
                    if (tmpIndex == -1)
                    {
                        tmpList.Add(new DTeventSummary(i, _with2.Fault, _with2.DT));
                    }
                    else
                    {
                        tmpList[tmpIndex].addStopWithRow(i, _with2.DT);
                    }
                }
            }
            return tmpList;
        }
        private List<DTeventSummary> getMappedReason1Directory(string MappedName)
        {
            int i = 0;
            int tmpIndex = 0;
            List<DTeventSummary> tmpList = new List<DTeventSummary>();
            for (i = 0; i <= _rawDTData.UnplannedData.Count - 1; i++)
            {
                var _with3 = _rawDTData.UnplannedData[i];
                if (_with3.MappedField == MappedName)
                {
                    tmpIndex = tmpList.IndexOf(new DTeventSummary(_with3.Reason1));
                    if (tmpIndex == -1)
                    {
                        tmpList.Add(new DTeventSummary(i, _with3.Reason1, _with3.DT));
                    }
                    else
                    {
                        tmpList[tmpIndex].addStopWithRow(i, _with3.DT);
                    }
                }
            }
            return tmpList;
        }
        private List<DTeventSummary> getMappedReason4Directory(string MappedName)
        {
            int i = 0;
            int tmpIndex = 0;
            List<DTeventSummary> tmpList = new List<DTeventSummary>();
            for (i = 0; i <= _rawDTData.UnplannedData.Count - 1; i++)
            {
                var _with4 = _rawDTData.UnplannedData[i];
                if (_with4.MappedField == MappedName)
                {
                    tmpIndex = tmpList.IndexOf(new DTeventSummary(_with4.Reason4));
                    if (tmpIndex == -1)
                    {
                        tmpList.Add(new DTeventSummary(i, _with4.Reason4, _with4.DT));
                    }
                    else
                    {
                        tmpList[tmpIndex].addStopWithRow(i, _with4.DT);
                    }
                }
            }
            return tmpList;
        }
        private List<DTeventSummary> getMappedReason3Directory(string MappedName)
        {
            int i = 0;
            int tmpIndex = 0;
            List<DTeventSummary> tmpList = new List<DTeventSummary>();
            for (i = 0; i <= _rawDTData.UnplannedData.Count - 1; i++)
            {
                var _with5 = _rawDTData.UnplannedData[i];
                if (_with5.MappedField == MappedName)
                {
                    tmpIndex = tmpList.IndexOf(new DTeventSummary(_with5.Reason3));
                    if (tmpIndex == -1)
                    {
                        tmpList.Add(new DTeventSummary(i, _with5.Reason3, _with5.DT));
                    }
                    else
                    {
                        tmpList[tmpIndex].addStopWithRow(i, _with5.DT);
                    }
                }
            }
            return tmpList;
        }
        private List<DTeventSummary> getMappedReason2Directory(string MappedName)
        {
            int i = 0;
            int tmpIndex = 0;
            List<DTeventSummary> tmpList = new List<DTeventSummary>();
            for (i = 0; i <= _rawDTData.UnplannedData.Count - 1; i++)
            {
                var _with6 = _rawDTData.UnplannedData[i];
                if (_with6.MappedField == MappedName)
                {
                    tmpIndex = tmpList.IndexOf(new DTeventSummary(_with6.Reason2));
                    if (tmpIndex == -1)
                    {
                        tmpList.Add(new DTeventSummary(i, _with6.Reason2, _with6.DT));
                    }
                    else
                    {
                        tmpList[tmpIndex].addStopWithRow(i, _with6.DT);
                    }
                }
            }
            return tmpList;
        }
        #endregion

        #region Get Tier 1-3 Directories
        public List<DTeventSummary> getUnplannedEventDirectory(int targetDtField, bool isByStops = true)
        {
            var tmpList = new List<DTeventSummary>();
            int i = 0;
            switch (targetDtField)
            {
                case (int)DowntimeField.Reason1:
                    if (isByStops)
                    {
                        GlobalFcns.sortEventList_ByStops(ref Reason1Directory);
                    }
                    else
                    {
                        GlobalFcns.sortEventList_ByDT(ref Reason1Directory);
                    }
                    for (i = 0; i <= Reason1Directory.Count - 1; i++)
                    {
                        tmpList.Add(Reason1Directory[i]);
                    }

                    break;
                case (int)DowntimeField.Reason2:
                    if (isByStops)
                    {
                        GlobalFcns.sortEventList_ByStops(ref Reason2Directory);
                    }
                    else
                    {
                        GlobalFcns.sortEventList_ByDT(ref Reason2Directory);
                    }
                    for (i = 0; i <= Reason2Directory.Count - 1; i++)
                    {
                        tmpList.Add(Reason2Directory[i]);
                    }

                    break;
                case (int)DowntimeField.Reason3:
                    if (isByStops)
                    {
                        GlobalFcns.sortEventList_ByStops(ref Reason3Directory);
                    }
                    else
                    {
                        GlobalFcns.sortEventList_ByDT(ref Reason3Directory);
                    }
                    for (i = 0; i <= Reason3Directory.Count - 1; i++)
                    {
                        tmpList.Add(Reason3Directory[i]);
                    }

                    break;
                case (int)DowntimeField.Reason4:
                    if (isByStops)
                    {
                        GlobalFcns.sortEventList_ByStops(ref Reason4Directory);
                    }
                    else
                    {
                        GlobalFcns.sortEventList_ByDT(ref Reason4Directory);
                    }
                    for (i = 0; i <= Reason4Directory.Count - 1; i++)
                    {
                        tmpList.Add(Reason4Directory[i]);
                    }

                    break;
                case (int)DowntimeField.Fault:
                    if (isByStops)
                    {
                        GlobalFcns.sortEventList_ByStops(ref FaultDirectory);
                    }
                    else
                    {
                        GlobalFcns.sortEventList_ByDT(ref FaultDirectory);
                    }
                    tmpList = FaultDirectory;
                    for (i = 0; i <= FaultDirectory.Count - 1; i++)
                    {
                        tmpList.Add(FaultDirectory[i]);
                    }

                    break;
                case (int)DowntimeField.Location:
                    if (isByStops)
                    {
                        GlobalFcns.sortEventList_ByStops(ref LocationDirectory);
                    }
                    else
                    {
                        GlobalFcns.sortEventList_ByDT(ref LocationDirectory);
                    }
                    tmpList = LocationDirectory;
                    for (i = 0; i <= LocationDirectory.Count - 1; i++)
                    {
                        tmpList.Add(LocationDirectory[i]);
                    }

                    break;
                case (int)DowntimeField.Tier1:
                    if (isByStops)
                    {
                        GlobalFcns.sortEventList_ByStops(ref Tier1Directory);
                    }
                    else
                    {
                        GlobalFcns.sortEventList_ByDT(ref Tier1Directory);
                    }
                    for (i = 0; i <= Tier1Directory.Count - 1; i++)
                    {
                        tmpList.Add(Tier1Directory[i]);
                    }

                    break;
                default:
                    throw new CustomExceptions.unknownMappingException();
            }
            return tmpList;
        }

        public List<DTeventSummary> getTier2Directory(string Tier1Name = "")
        {
            int i = 0;
            int tmpIndex = 0;
            List<DTeventSummary> tmpList = new List<DTeventSummary>();
            if (Convert.IsDBNull(Tier1Name))
            {
                for (i = 0; i <= _rawDTData.UnplannedData.Count - 1; i++)
                {
                    var _with7 = _rawDTData.UnplannedData[i];
                    tmpIndex = tmpList.IndexOf(new DTeventSummary(_with7.Tier2));
                    if (tmpIndex == -1)
                    {
                        tmpList.Add(new DTeventSummary(i, _with7.Tier2, _with7.DT));
                    }
                    else
                    {
                        tmpList[tmpIndex].addStopWithRow(i, _with7.DT);
                    }
                }
            }
            else
            {
                for (i = 0; i <= _rawDTData.UnplannedData.Count - 1; i++)
                {
                    var _with8 = _rawDTData.UnplannedData[i];
                    if (_with8.Tier1 == Tier1Name & _with8.Tier2.Length > 1)
                    {
                        tmpIndex = tmpList.IndexOf(new DTeventSummary(_with8.Tier2));
                        if (tmpIndex == -1)
                        {
                            tmpList.Add(new DTeventSummary(i, _with8.Tier2, _with8.DT));
                        }
                        else
                        {
                            tmpList[tmpIndex].addStopWithRow(i, _with8.DT);
                        }
                    }
                }
            }
            return tmpList;
        }
        public List<DTeventSummary> getTier3Directory(string Tier1Name = "", string Tier2Name = "")
        {
            int i = 0;
            int tmpIndex = 0;
            List<DTeventSummary> tmpList = new List<DTeventSummary>();
            if (Convert.IsDBNull(Tier1Name))
            {
                for (i = 0; i <= _rawDTData.UnplannedData.Count - 1; i++)
                {
                    var _with9 = _rawDTData.UnplannedData[i];
                    tmpIndex = tmpList.IndexOf(new DTeventSummary(_with9.Tier3));
                    if (tmpIndex == -1)
                    {
                        tmpList.Add(new DTeventSummary(i, _with9.Tier3, _with9.DT));
                    }
                    else
                    {
                        tmpList[tmpIndex].addStopWithRow(i, _with9.DT);
                    }
                }
            }
            else
            {
                for (i = 0; i <= _rawDTData.UnplannedData.Count - 1; i++)
                {
                    var _with10 = _rawDTData.UnplannedData[i];
                    if (_with10.Tier1 == Tier1Name & _with10.Tier2 == Tier2Name & _with10.Tier3.Length > 1)
                    {
                        tmpIndex = tmpList.IndexOf(new DTeventSummary(_with10.Tier3));
                        if (tmpIndex == -1)
                        {
                            tmpList.Add(new DTeventSummary(i, _with10.Tier3, _with10.DT));
                        }
                        else
                        {
                            tmpList[tmpIndex].addStopWithRow(i, _with10.DT);
                        }
                    }
                }
            }
            return tmpList;
        }

        public List<DTeventSummary> getPlannedEventDirectory(int targetDtfield, bool isByStops = true)
        {
            var tmpList = new List<DTeventSummary>();
            int i = 0;
            switch (targetDtfield)
            {
                case (int)DowntimeField.Tier1:
                    if (isByStops)
                    {
                        GlobalFcns.sortEventList_ByStops(ref PlannedTier1Directory);
                    }
                    else
                    {
                        GlobalFcns.sortEventList_ByDT(ref Tier1Directory);
                    }
                    for (i = 0; i <= PlannedTier1Directory.Count - 1; i++)
                    {
                        tmpList.Add(PlannedTier1Directory[i]);
                    }

                    break;
                default:
                    throw new CustomExceptions.unknownMappingException();
            }
            return tmpList;
        }

        public List<DTeventSummary> getPlannedTier2Directory(string PlannedTier1Name = "")
        {
            int i = 0;
            int tmpIndex = 0;
            List<DTeventSummary> tmpList = new List<DTeventSummary>();
            if (Convert.IsDBNull(PlannedTier1Name))
            {
                //   For i = 0 To _rawDTData.PlannedData.Count - 1
                // With _rawDTData.PlannedData[i]
                // tmpIndex = tmpList.IndexOf(New DTeventSummary(.Tier2, 0))
                // If tmpIndex = -1 Then
                // tmpList.Add(New DTeventSummary(.Tier2, .DT, i))
                //  E() 'lse
                //   tmpList[tmpIndex].addStopWithRow(.DT, i)
                //End If
                //    End With
                //   Next
            }
            else
            {
                for (i = 0; i <= _rawDTData.PlannedData.Count - 1; i++)
                {
                    var _with11 = _rawDTData.PlannedData[i];
                    if (_with11.Tier1 == PlannedTier1Name)
                    {
                        tmpIndex = tmpList.IndexOf(new DTeventSummary(_with11.Tier2));
                        if (tmpIndex == -1)
                        {
                            tmpList.Add(new DTeventSummary(i, _with11.Tier2, _with11.DT));
                        }
                        else
                        {
                            tmpList[tmpIndex].addStopWithRow(i, _with11.DT);
                        }
                    }
                }
            }
            return tmpList;
        }
        public List<DTeventSummary> getPlannedTier3Directory(string PlannedTier1Name = "", string PlannedTier2Name = "")
        {
            int i = 0;
            int tmpIndex = 0;
            List<DTeventSummary> tmpList = new List<DTeventSummary>();
            if (Convert.IsDBNull(PlannedTier1Name))
            {
                for (i = 0; i <= _rawDTData.PlannedData.Count - 1; i++)
                {
                    var _with12 = _rawDTData.PlannedData[i];
                    tmpIndex = tmpList.IndexOf(new DTeventSummary(_with12.Tier3));
                    if (tmpIndex == -1)
                    {
                        tmpList.Add(new DTeventSummary(i, _with12.Tier3, _with12.DT));
                    }
                    else
                    {
                        tmpList[tmpIndex].addStopWithRow(i, _with12.DT);
                    }
                }
            }
            else
            {
                for (i = 0; i <= _rawDTData.PlannedData.Count - 1; i++)
                {
                    var _with13 = _rawDTData.PlannedData[i];
                    if (_with13.Tier1 == PlannedTier1Name & _with13.Tier2 == PlannedTier2Name)
                    {
                        tmpIndex = tmpList.IndexOf(new DTeventSummary(_with13.Tier3));
                        if (tmpIndex == -1)
                        {
                            tmpList.Add(new DTeventSummary(i, _with13.Tier3, _with13.DT));
                        }
                        else
                        {
                            tmpList[tmpIndex].addStopWithRow(i, _with13.DT);
                        }
                    }
                }
            }
            return tmpList;
        }

        public List<DTeventSummary> getReason2SubDirectory(string Reason1Name)
        {
            int i = 0;
            int tmpIndex = 0;
            List<DTeventSummary> tmpList = new List<DTeventSummary>();
            for (i = 0; i <= _rawDTData.UnplannedData.Count - 1; i++)
            {
                var _with14 = _rawDTData.UnplannedData[i];
                if (_with14.Reason1 == Reason1Name)
                {
                    tmpIndex = tmpList.IndexOf(new DTeventSummary(_with14.Reason2));
                    if (tmpIndex == -1)
                    {
                        tmpList.Add(new DTeventSummary(i, _with14.Reason2, _with14.DT));
                    }
                    else
                    {
                        tmpList[tmpIndex].addStopWithRow(i, _with14.DT);
                    }
                }
            }
            return tmpList;
        }
        #endregion

        #region Construction
        public SystemDowntimeReport(downtimeInterface rawDataInterface)
        {
            _rawDTData = rawDataInterface;

            initializeFilterAnalysis();
            initializeTacticalAnalysis();
            initializeTacticalAnalysis_Planned();

            DT = PDT + UPDT;
            schedTime = UT + DT;
        }
        public SystemDowntimeReport(productionLine newParentline, DateTime startTime, DateTime endTime)
        {
            _rawDTData = newParentline.rawDowntimeData.getSubset(startTime, endTime);

            initializeFilterAnalysis();
            initializeTacticalAnalysis();
            initializeTacticalAnalysis_Planned();

            DT = PDT + UPDT;
            schedTime = UT + DT;

            if (newParentline._isDualConstraint & Settings.Default.AdvancedSettings_MultiConstraintAnalysisMode == (int)Globals.MultiConstraintAnalysis.NoRateLossStops)
            {
                for (int listIncrementer = 0; listIncrementer <= _rawDTData.UnplannedData.Count - 1; listIncrementer++)
                {
                    if (_rawDTData.UnplannedData[listIncrementer].MasterProductionUnit.Contains("Rate"))
                        rateLossEvents += 1;
                }
            }
        }

        #endregion

        public SystemDowntimeReport getSubset(DateTime startTime, DateTime endTime)
        {
            if (startTime < this.StartTime || endTime > this.EndTime)
            {
                if (startTime < this.StartTime && endTime > this.EndTime)
                {
                    return this;
                }
                else if (startTime < this.StartTime)
                {
                    return new SystemDowntimeReport(_rawDTData.getSubset(this.StartTime, endTime));
                }
                else
                {
                    return new SystemDowntimeReport(_rawDTData.getSubset(startTime, this.EndTime));
                }
            }
            else
            {
                return new SystemDowntimeReport(_rawDTData.getSubset(startTime, endTime));
            }
        }

        public void reMapDataSet(DowntimeField MappingA, DowntimeField MappingB)
        {
            int tmpIndex = 0;
            //change the mapped field
            _rawDTData.reMapData(MappingA, MappingB);
            //clear existing directory
            MappedDirectory.Clear();
            MappedDirectory_Planned.Clear();
            //recreate the directory
            for (int i = 0; i <= _rawDTData.UnplannedData.Count - 1; i++)
            {
                DTevent _with15 = _rawDTData.UnplannedData[i];
                //mapped stuff
                tmpIndex = MappedDirectory.IndexOf(new DTeventSummary(_with15.MappedField));
                if (tmpIndex == -1)
                {
                    MappedDirectory.Add(new DTeventSummary(_with15.MappedField, _with15.DT, 1));
                }
                else
                {
                    MappedDirectory[tmpIndex].addStop(_with15.DT);
                }
            }
            for (int i = 0; i <= _rawDTData.PlannedData.Count - 1; i++)
            {
                DTevent _ith15 = _rawDTData.PlannedData[i];
                //mapped stuff
                tmpIndex = MappedDirectory_Planned.IndexOf(new DTeventSummary(_ith15.MappedField));
                if (tmpIndex == -1)
                {
                    MappedDirectory_Planned.Add(new DTeventSummary(_ith15.MappedField, _ith15.DT, 1));
                }
                else
                {
                    MappedDirectory_Planned[tmpIndex].addStop(_ith15.DT);
                }
            }
        }
        #region Filtering

        public void reFilterData_SKU(List<string> inclusionList)
        {
            reFilter_Initialize();
            rawDTdata.reFilterData_SKU(inclusionList);
            reFilter_Finalize();
        }
        public void reFilterData_Team(List<string> inclusionList)
        {
            reFilter_Initialize();
            rawDTdata.reFilterData_Team(inclusionList);
            reFilter_Finalize();
        }
        public void reFilterData_Shape(List<string> inclusionList)
        {
            reFilter_Initialize();
            rawDTdata.reFilterData_Shape(inclusionList);
            reFilter_Finalize();
        }
        public void reFilterData_Format(List<string> inclusionList)
        {
            reFilter_Initialize();
            rawDTdata.reFilterData_Format(inclusionList);
            reFilter_Finalize();
        }
        public void reFilterData_ProductGroup(List<string> inclusionList)
        {
            reFilter_Initialize();
            rawDTdata.reFilterData_ProductGroup(inclusionList);
            reFilter_Finalize();
        }
        public void reFilterData_ClearAllFilters()
        {
            reFilter_Initialize();
            rawDTdata.reFilterData_ClearAllFilters();
            reFilter_Finalize();
        }
        private void reFilter_Initialize()
        {
            UPDT = 0;
            PDT = 0;
            UT = 0;
            DT = 0;
            excludedTime = 0;
            schedTime = 0;

            rateLossEvents = 0;

            //UNPLANNED - raw data 
            FaultDirectory.Clear();
            Reason1Directory.Clear();

            LocationDirectory.Clear();

            Reason2Directory.Clear();
            Reason3Directory.Clear();
            Reason4Directory.Clear();
            Tier1Directory.Clear();
            PlannedTier1Directory.Clear();
            MappedDirectory.Clear();
            MappedDirectory_Planned.Clear();
        }
        private void reFilter_Finalize()
        {
            initializeTacticalAnalysis();
            initializeTacticalAnalysis_Planned();

            DT = PDT + UPDT;
            schedTime = UT + DT;
        }

        #endregion

        private void initializeFilterAnalysis()
        {
            //set up our sorting fields
            var tmpGCAS = new List<string>();
            var tmpProducts = new List<string>();
            var tmpTeams = new List<string>();
            var tmpShapes = new List<string>();
            var tmpFormats = new List<string>();
            var tmpProductGroups = new List<string>();
            //look at all the unplanned data
            for (int i = 0; i <= _rawDTData.rawConstraintData.Count - 1; i++)
            {
                var _with16 = _rawDTData.rawConstraintData[i];
                if (!_with16.isExcluded)
                {
                    //sorting fields
                    tmpFormats.Add(_with16.Format);
                    tmpProducts.Add(_with16.Product);
                    tmpGCAS.Add(_with16.ProductCode);
                    tmpShapes.Add(_with16.Shape);
                    tmpTeams.Add(_with16.Team);
                    tmpProductGroups.Add(_with16.ProductGroup);
                }
            }
            //finalize our sorting fields
            ActiveGCAS = tmpGCAS.Distinct().ToList();
            ActiveProducts = tmpProducts.Distinct().ToList();
            ActiveTeams = tmpTeams.Distinct().ToList();
            ActiveShapes = tmpShapes.Distinct().ToList();
            ActiveFormats = tmpFormats.Distinct().ToList();
            ActiveProductGroups = tmpProductGroups.Distinct().ToList();
        }
        private void initializeTacticalAnalysis()
        {
            int tmpIndex = 0;
            //look at all the unplanned data
            for (int i = 0; i < _rawDTData.UnplannedData.Count; i++)
            {
                var _with17 = _rawDTData.UnplannedData[i];
                UPDT += _with17.DT;
                UT += _with17.UT;

                //? - not sure why? sro 1/26/2016  if (UT == 0) { excludedStops += 1; }


                //R1
                tmpIndex = Reason1Directory.IndexOf(new DTeventSummary(_with17.Reason1));
                if (tmpIndex == -1)
                {
                    Reason1Directory.Add(new DTeventSummary(i, _with17.Reason1, _with17.DT));
                }
                else
                {
                    Reason1Directory[tmpIndex].addStopWithRow(i, _with17.DT);
                }
                //R2
                tmpIndex = Reason2Directory.IndexOf(new DTeventSummary(_with17.Reason2));
                if (tmpIndex == -1)
                {
                    Reason2Directory.Add(new DTeventSummary(i, _with17.Reason2, _with17.DT));
                }
                else
                {
                    Reason2Directory[tmpIndex].addStopWithRow(i, _with17.DT);
                }
                //R3
                tmpIndex = Reason3Directory.IndexOf(new DTeventSummary(_with17.Reason3));
                if (tmpIndex == -1)
                {
                    Reason3Directory.Add(new DTeventSummary(i, _with17.Reason3, _with17.DT));
                }
                else
                {
                    Reason3Directory[tmpIndex].addStopWithRow(i, _with17.DT);
                }
                //R4
                tmpIndex = Reason4Directory.IndexOf(new DTeventSummary(_with17.Reason4));
                if (tmpIndex == -1)
                {
                    Reason4Directory.Add(new DTeventSummary(i, _with17.Reason4, _with17.DT));
                }
                else
                {
                    Reason4Directory[tmpIndex].addStopWithRow(i, _with17.DT);
                }
                //fault
                tmpIndex = FaultDirectory.IndexOf(new DTeventSummary(_with17.Fault));
                if (tmpIndex == -1)
                {
                    FaultDirectory.Add(new DTeventSummary(i, _with17.Fault, _with17.DT));
                }
                else
                {
                    FaultDirectory[tmpIndex].addStopWithRow(i, _with17.DT);
                }
                //location
                tmpIndex = LocationDirectory.IndexOf(new DTeventSummary(_with17.Location));
                if (tmpIndex == -1)
                {
                    LocationDirectory.Add(new DTeventSummary(i, _with17.Location, _with17.DT));
                }
                else
                {
                    LocationDirectory[tmpIndex].addStopWithRow(i, _with17.DT);
                }
                //tier1
                tmpIndex = Tier1Directory.IndexOf(new DTeventSummary(_with17.Tier1));
                if (tmpIndex == -1)
                {
                    Tier1Directory.Add(new DTeventSummary(i, _with17.Tier1, _with17.DT));
                }
                else
                {
                    Tier1Directory[tmpIndex].addStopWithRow(i, _with17.DT);
                }
                tmpIndex = MappedDirectory.IndexOf(new DTeventSummary(_with17.MappedField));
                if (tmpIndex == -1)
                {
                    MappedDirectory.Add(new DTeventSummary(i, _with17.MappedField, _with17.DT));
                }
                else
                {
                    MappedDirectory[tmpIndex].addStopWithRow(i, _with17.DT);
                }
                //*/

            }

        }
        private void initializeTacticalAnalysis_Planned()
        {
            int tmpIndex = 0;
            //look at all the unplanned data
            for (int i = 0; i < _rawDTData.PlannedData.Count; i++)
            {
                var _with18 = _rawDTData.PlannedData[i];
                PDT += _with18.DT;
                UT += _with18.UT;
                //data fields


                //tier1
                tmpIndex = PlannedTier1Directory.IndexOf(new DTeventSummary(_with18.Tier1));
                if (tmpIndex == -1)
                {
                    PlannedTier1Directory.Add(new DTeventSummary(i, _with18.Tier1, _with18.DT));
                }
                else
                {
                    PlannedTier1Directory[tmpIndex].addStopWithRow(i, _with18.DT);
                }
                tmpIndex = MappedDirectory_Planned.IndexOf(new DTeventSummary(_with18.MappedField));
                if (tmpIndex == -1)
                {
                    MappedDirectory_Planned.Add(new DTeventSummary(i, _with18.MappedField, _with18.DT));
                }
                else
                {
                    MappedDirectory_Planned[tmpIndex].addStopWithRow(i, _with18.DT);
                }


            }

        }
    }
}

