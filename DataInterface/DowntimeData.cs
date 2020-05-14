using System;
using System.Collections.Generic;
using static ForkAnalyticsSettings.GlobalConstants;
using ProductionLines;
using Analytics;

namespace DataInterface
{
    public class downtimeInterface
    {
        #region Variables
        internal List<DTevent> rawConstraintData { get; set; } = new List<DTevent>();

        internal List<DTevent> rawRateLossData { get; set; } = new List<DTevent>();

        internal List<DTevent> UnplannedData { get; set; } = new List<DTevent>();
        internal List<DTevent> PlannedData { get; set; } = new List<DTevent>();

        internal List<DTevent> CILdata { get; set; } = new List<DTevent>();
        internal List<DTevent> COdata { get; set; } = new List<DTevent>();

        public LineConfig parentLine { get; set; }
        public DateTime StartDate
        {
            get { return rawConstraintData[0].startTime_UT; }
        }
        public DateTime EndDate
        {
            get { return rawConstraintData[rawConstraintData.Count - 1].endTime; }
        }
        public long Stops
        {
            get { return UnplannedData.Count; }
        }
        #endregion

        public void reMapData(DowntimeField MappingFieldA, DowntimeField MappingFieldB = DowntimeField.NA)
        {
            int i = 0;
            if (MappingFieldB == DowntimeField.NA)
            {
                for (i = 0; i <= rawConstraintData.Count - 1; i++)
                {
                    rawConstraintData[i].MappedField = rawConstraintData[i].getFieldFromInteger(MappingFieldA);
                }
                for (i = 0; i <= UnplannedData.Count - 1; i++)
                {
                    UnplannedData[i].MappedField = UnplannedData[i].getFieldFromInteger(MappingFieldA);
                }
                for (i = 0; i <= PlannedData.Count - 1; i++)
                {
                    PlannedData[i].MappedField = PlannedData[i].getFieldFromInteger(MappingFieldA);
                }
                //Dual Mapping!!!
            }
            else
            {
                for (i = 0; i <= rawConstraintData.Count - 1; i++)
                {
                    rawConstraintData[i].MappedField = rawConstraintData[i].getFieldFromInteger(MappingFieldA) + "-" + rawConstraintData[i].getFieldFromInteger(MappingFieldB);
                }
                for (i = 0; i <= UnplannedData.Count - 1; i++)
                {
                    UnplannedData[i].MappedField = UnplannedData[i].getFieldFromInteger(MappingFieldA) + "-" + UnplannedData[i].getFieldFromInteger(MappingFieldB);
                }
                for (i = 0; i <= PlannedData.Count - 1; i++)
                {
                    PlannedData[i].MappedField = PlannedData[i].getFieldFromInteger(MappingFieldA) + "-" + PlannedData[i].getFieldFromInteger(MappingFieldB);
                }
            }
        }

        #region Filtering Data Sets
        public void reFilterData_ClearAllFilters()
        {
            int i = 0;
            initializeDataForFiltering();
            for (i = 0; i <= rawConstraintData.Count - 1; i++)
            {
                rawConstraintData[i].isFiltered = false;
            }
            for (i = 0; i <= UnplannedData.Count - 1; i++)
            {
                UnplannedData[i].isFiltered = false;
            }
            for (i = 0; i <= PlannedData.Count - 1; i++)
            {
                PlannedData[i].isFiltered = false;
            }
            finalizeFiltering();
        }

        public void reFilterData_Team(List<string> inclusionList)
        {
            initializeDataForFiltering();
            for (int i = 0; i <= rawConstraintData.Count - 1; i++)
            {
                if (inclusionList.IndexOf(rawConstraintData[i].Team) == -1)
                    rawConstraintData[i].isFiltered = true;
            }
            finalizeFiltering();
        }
        public void reFilterData_Format(List<string> inclusionList)
        {
            initializeDataForFiltering();
            for (int i = 0; i <= rawConstraintData.Count - 1; i++)
            {
                if (inclusionList.IndexOf(rawConstraintData[i].Format) == -1)
                    rawConstraintData[i].isFiltered = true;
            }
            finalizeFiltering();
        }
        public void reFilterData_ProductGroup(List<string> inclusionList)
        {
            initializeDataForFiltering();
            for (int i = 0; i <= rawConstraintData.Count - 1; i++)
            {
                if (inclusionList.IndexOf(rawConstraintData[i].ProductGroup) == -1)
                    rawConstraintData[i].isFiltered = true;
            }
            finalizeFiltering();
        }
        public void reFilterData_SKU(List<string> inclusionList)
        {
            initializeDataForFiltering();
            for (int i = 0; i <= rawConstraintData.Count - 1; i++)
            {
                if (inclusionList.IndexOf(rawConstraintData[i].Product) == -1)
                    rawConstraintData[i].isFiltered = true;
            }
            finalizeFiltering();
        }
        public void reFilterData_Shape(List<string> inclusionList)
        {
            initializeDataForFiltering();
            for (int i = 0; i <= rawConstraintData.Count - 1; i++)
            {
                if (inclusionList.IndexOf(rawConstraintData[i].Shape) == -1)
                    rawConstraintData[i].isFiltered = true;
            }
            finalizeFiltering();
        }
        private void initializeDataForFiltering()
        {
            UnplannedData.Clear();
            PlannedData.Clear();
            COdata.Clear();
            CILdata.Clear();
        }
        private void finalizeFiltering()
        {
            for (int eventIncrementer = 0; eventIncrementer <= rawConstraintData.Count - 1; eventIncrementer++)
            {
                if (!rawConstraintData[eventIncrementer].isExcluded)
                {
                    if (rawConstraintData[eventIncrementer].isUnplanned)
                    {
                        UnplannedData.Add(rawConstraintData[eventIncrementer]);
                    }
                    else if (rawConstraintData[eventIncrementer].isPlanned)
                    {
                        PlannedData.Add(rawConstraintData[eventIncrementer]);
                        if (rawConstraintData[eventIncrementer].isCIL)
                        {
                            CILdata.Add(rawConstraintData[eventIncrementer]);
                        }
                        else if (rawConstraintData[eventIncrementer].isChangeover)
                        {
                            COdata.Add(rawConstraintData[eventIncrementer]);
                        }
                    }
                }
            }
        }
        #endregion

        #region Constructors
        public downtimeInterface(LineConfig pLine)
        {
            parentLine = pLine;
        }

        public downtimeInterface(LineConfig pline, DTevent singleEvent) : this(pline)
        {
            rawConstraintData.Add(singleEvent);
            if (singleEvent.isPlanned)
            {
                PlannedData.Add(singleEvent);
                if (singleEvent.isCIL)
                {
                    CILdata.Add(singleEvent);
                }
                else if (singleEvent.isChangeover)
                {
                    COdata.Add(singleEvent);
                }
            }
            else if (singleEvent.isUnplanned)
            {
                UnplannedData.Add(singleEvent);
            }
        }

        public downtimeInterface(LineConfig pLine, List<DTevent> rawData) : this(pLine)
        {
            rawConstraintData = rawData;
            analyzeRawConstraintData();
        }
        #endregion

        public void analyzeRawConstraintData()
        {

            for (int eventIncrementer = 0; eventIncrementer < rawConstraintData.Count; eventIncrementer++)
            {
                if (rawConstraintData[eventIncrementer].isUnplanned)
                {
                    UnplannedData.Add(rawConstraintData[eventIncrementer]);
                }
                else if (rawConstraintData[eventIncrementer].isPlanned)
                {
                    PlannedData.Add(rawConstraintData[eventIncrementer]);
                    if (rawConstraintData[eventIncrementer].isCIL)
                    {
                        CILdata.Add(rawConstraintData[eventIncrementer]);
                    }
                    else if (rawConstraintData[eventIncrementer].isChangeover)
                    {
                        COdata.Add(rawConstraintData[eventIncrementer]);
                    }
                }

            }
        }

        public downtimeInterface getSubset(DateTime startDate, DateTime endDate, int MappingFieldA = -1, int MappingFieldB = -1)
        {
            int listIndexA = 0;
            int listIndexB = 0;
            DTevent tmpEvent;
            var tmpData = new downtimeInterface(parentLine);
            int eventIncrementer = 0;
            listIndexA = rawConstraintData.IndexOf(new DTevent(startDate));
            if (listIndexA == -1)
            {
                if (startDate < rawConstraintData[0].endTime)
                {
                    listIndexA = 0;
                }
                else if (startDate < rawConstraintData[rawConstraintData.Count - 1].endTime)
                { //here we'll just find the closest to get rid of errors
                    for (int i = 0; i < rawConstraintData.Count; i++)
                    {
                        if (startDate > rawConstraintData[i].startTime_UT)
                        {
                            listIndexA = i; //THIS IS A FUDGE FACTOR TO ACCOMODATE CRAPPY DATA!!!
                        }
                    }
                }
            }
            listIndexB = rawConstraintData.IndexOf(new DTevent(endDate));
            if (listIndexB == -1)
            {
                DTevent _with2 = rawConstraintData[rawConstraintData.Count - 1];
                if (endDate.Equals(_with2.endTime))
                {
                    listIndexB = rawConstraintData.Count - 1;
                }
                else if (endDate > _with2.endTime)
                {
                    rawConstraintData.Add(new DTevent((endDate - _with2.endTime).TotalSeconds / 60, endDate, endDate, false)); //DateDiff(DateInterval.Second, _with2.endTime, endDate) / 60, endDate, endDate, false));
                    listIndexB = rawConstraintData.Count - 1;

                    //maybe this event will help with our listIndexA...
                    if (listIndexA == -1)
                    {
                        if (startDate >= rawConstraintData[rawConstraintData.Count - 1].startTime_UT)
                            listIndexA = rawConstraintData.Count - 1;
                    }

                }
            }
            if (listIndexB == -1 && endDate > rawConstraintData[0].startTime_UT) //if were STILL here -> FUDGE IT! (ie assume crappy data)
            {
                for (int i = 0; i < rawConstraintData.Count; i++)
                {
                    if (endDate < rawConstraintData[i].endTime && i >= listIndexA)
                    {
                        listIndexB = i; //THIS IS A FUDGE FACTOR TO ACCOMODATE CRAPPY DATA!!!
                    }
                }
            }

            //one line only
            if (listIndexA == listIndexB)
            {
                tmpEvent = rawConstraintData[listIndexA].getCopy();
                tmpEvent.adjustMyStartTime(startDate);
                tmpEvent.adjustMyEndTime(endDate);
                return new downtimeInterface(parentLine, tmpEvent);
            }
            //multiple lines of data
            else if (listIndexA < listIndexB)
            {
                tmpData = new downtimeInterface(parentLine);
                tmpEvent = rawConstraintData[listIndexA].getCopy();
                tmpEvent.adjustMyStartTime(startDate);

                tmpData.rawConstraintData.Add(tmpEvent);


                for (eventIncrementer = listIndexA + 1; eventIncrementer <= listIndexB - 1; eventIncrementer++)
                {
                    tmpData.rawConstraintData.Add(rawConstraintData[eventIncrementer]);
                }

                if (rawConstraintData[listIndexB].DT > 0)
                {
                    tmpEvent = rawConstraintData[listIndexB].getCopy(); // sro - i changed this on january 26, 2016 //new DTevent();
                    tmpEvent.adjustMyEndTime(endDate);
                    tmpData.rawConstraintData.Add(tmpEvent);
                }
                else
                {
                    tmpData.rawConstraintData.Add(rawConstraintData[listIndexB]);
                }

                tmpData.analyzeRawConstraintData();
                return tmpData;
            }
            else if (listIndexA == -1 | listIndexB == -1)
            {
                throw new CustomExceptions.dateRangeException("Dates Not Found");
            }
            else
            {
                throw new CustomExceptions.dateRangeException("Date order incorrect");
            }
        }
    }

    public class DTevent : IComparable<DTevent>, IEquatable<DTevent>
    {
        public override string ToString()
        {
            return ParentLineName + " " + startTime_UT.ToString("MM/dd HH:mm:ss") + " ~ " + endTime.ToString("MM/dd HH:mm:ss") + " DT/UT: " + Math.Round(DT, 1) + "/" + Math.Round(UT, 1) + " " + Tier1 + "/" + Tier2 + "/" + Tier3;
        }
        public string getFieldFromInteger(DowntimeField FieldA, DowntimeField FieldB)
        {
            if (FieldB == DowntimeField.NA) { return getFieldFromInteger(FieldA); }
            return getFieldFromInteger(FieldA) + "-" + getFieldFromInteger(FieldB);
        }

        public string getFieldFromInteger(DowntimeField TargetField)
        {
            switch (TargetField)
            {
                case DowntimeField.Location:
                    return Location;
                case DowntimeField.Fault:
                    return Fault;
                case DowntimeField.Reason1:
                    return Reason1;
                case DowntimeField.Reason2:
                    return Reason2;
                case DowntimeField.Reason3:
                    return Reason3;
                case DowntimeField.Reason4:
                    return Reason4;
                case DowntimeField.Stopclass:
                    return StopClass;
                case DowntimeField.DTGroup:
                    return DTGroup;
                case DowntimeField.ProductCode:
                    return ProductCode;
                case DowntimeField.Tier1:
                    return Tier1;
                case DowntimeField.Tier2:
                    return Tier2;
                case DowntimeField.Tier3:
                    return Tier3;
                case DowntimeField.Format:
                    return Format;
                case DowntimeField.Shape:
                    return Shape;
                case DowntimeField.ProductGroup:
                    return ProductGroup;
                case DowntimeField.Team:
                    return Team;
                case DowntimeField.ParentLine:
                    return ParentLineName;
                default:
                    return MappedField;
                    //throw new CustomExceptions.unknownMappingException();
            }
        }

        #region Sorting Parameters
        public bool _isStandardSort = true;
        public DowntimeField _sortField;
        public void setSortParam(DowntimeField dtField)
        {
            if (dtField == DowntimeField.startTime)
            {
                _isStandardSort = true;
            }
            else
            {
                _sortField = dtField;
                _isStandardSort = false;
            }
        }
        #endregion

        #region Variables
        internal bool _isExcluded;

        public bool isUnplanned { get; set; }
        public bool isExcluded
        {
            get { return (_isExcluded | isFiltered | isFiltered_ParentLine); }
        }
        public bool isExcluded_NoFilter
        {
            get { return _isExcluded; }
        }
        public bool isPlanned { get; set; }
        public bool isChangeover { get; set; } = false;
        public bool isCIL { get; set; } = false;
        public bool isFiltered_ParentLine = false;
        public bool isFiltered { get; set; } = false;
        public DateTime startTime_UT { get { return startTime.AddSeconds(-60 * UT); } }
        public DateTime startTime { get; set; }
        public DateTime endTime { get; set; }
        public string startTime_24hr
        {
            get { return startTime.ToString("MM/dd/yyyy HH:mm:ss"); }
        }
        public string endTime_24hr
        {
            get { return endTime.ToString("MM/dd/yyyy HH:mm:ss"); }
        }

        public double DT { get; set; }
        public double DT_display { get { return Math.Round(DT, 2); } }
        public double UT_display { get { return Math.Round(UT, 2); } }
        public double UT { get; set; }

        //top level leds fields
        public string Location { get; set; }
        public string Fault { get; set; }
        public string DTGroup { get; set; }
        //tree level fields
        public string Reason1 { get; set; }
        public string Reason2 { get; set; }
        public string Reason3 { get; set; }
        public string Reason4 { get; set; }
        public string StopClass { get; set; }
        public string Comment { get; set; }
        public string PlannedUnplanned { get; set; }
        public string Team { get; set; }
        public string OEE_inout { get; set; }
        public string ProductCode { get; set; }
        public string ProductGroup { get; set; }
        public string Product { get; set; }
        public string MasterProductionUnit { get; set; }
        public string Tier1 { get; set; }
        public string Tier2 { get; set; }
        public string Tier3 { get; set; }
        public string Format { get; set; }
        public string Shape { get; set; }
        public string MappedField { get; set; }

        public string ParentLineName { get; set; }
        #endregion

        public DTevent getCopy()
        {
            var tmpEvent = new DTevent();
            tmpEvent._isExcluded = this._isExcluded;

            tmpEvent._isStandardSort = true;
            tmpEvent._sortField = this._sortField;
            tmpEvent.isUnplanned = this.isUnplanned;
            tmpEvent.isFiltered_ParentLine = this.isFiltered_ParentLine;

            tmpEvent.isPlanned = this.isPlanned;
            tmpEvent.isChangeover = this.isChangeover;
            tmpEvent.isCIL = this.isCIL;
            tmpEvent.isFiltered = this.isFiltered;
            tmpEvent.startTime = this.startTime;
            tmpEvent.endTime = this.endTime;


            tmpEvent.DT = this.DT;
            tmpEvent.UT = this.UT;

            //top level leds fields
            tmpEvent.Location = this.Location;
            tmpEvent.Fault = this.Fault;
            tmpEvent.DTGroup = this.DTGroup;
            //tree level fields
            tmpEvent.Reason1 = this.Reason1;
            tmpEvent.Reason2 = this.Reason2;
            tmpEvent.Reason3 = this.Reason3;
            tmpEvent.Reason4 = this.Reason4;
            tmpEvent.StopClass = this.StopClass;
            tmpEvent.Comment = this.Comment;
            tmpEvent.PlannedUnplanned = this.PlannedUnplanned;
            tmpEvent.Team = this.Team;
            tmpEvent.OEE_inout = this.OEE_inout;
            tmpEvent.ProductCode = this.ProductCode;
            tmpEvent.ProductGroup = this.ProductGroup;
            tmpEvent.Product = this.Product;
            tmpEvent.MasterProductionUnit = this.MasterProductionUnit;
            tmpEvent.Tier1 = this.Tier1;
            tmpEvent.Tier2 = this.Tier2;
            tmpEvent.Tier3 = this.Tier3;
            tmpEvent.Format = this.Format;
            tmpEvent.Shape = this.Shape;
            tmpEvent.MappedField = this.MappedField;

            tmpEvent.ParentLineName = this.ParentLineName;
            return tmpEvent;
        }


        #region Construction
        public DTevent() { }
        public DTevent(DateTime startDate)
        {
            startTime = startDate;
            endTime = startDate;
        }
        //constructor for DEMO MODE
        public DTevent(DateTime startTime, DateTime endTime, double DT, double UT)
        {
            this.startTime = startTime;
            this.endTime = endTime;
            this.DT = DT;
            this.UT = UT;

            _isExcluded = false;
            isPlanned = false;
            isUnplanned = false;
            isChangeover = false;
            isCIL = false;
        }

        public DTevent(double uptimeOnly, DateTime startTime, DateTime endTime, bool isExcluded)
        {
            this.DT = 0;
            this.startTime = startTime;
            this.endTime = endTime;
            UT = uptimeOnly;

            _isExcluded = isExcluded;
            isPlanned = false;
            isUnplanned = true;
            isChangeover = false;
            isCIL = false;
        }
        #endregion

        #region Adjust Start/End Times


        //modify event based on a start and end time
        public void adjustMyStartTime(DateTime startTime)
        {
            double timeDifference = 0;
            timeDifference = (this.startTime - startTime).TotalSeconds; //DateDiff(DateInterval.Second, startTime, _startTime);

            /*
            switch (timeDifference) {
                case  // ERROR: Case labels with binary operators are unsupported : LessThan
    0:
                    //_startTime is earlier: we need to cut the all the ut & and dt
                    UT = 0;
                    _DT = ((TimeSpan)((DateTime)_endTime - (DateTime)startTime)).TotalMinutes; //DateDiff(DateInterval.Second, startTime, _endTime) / 60;
                    _startTime = startTime;
                    break;
                case 0:
                    break;
                // do nothing
                case  // ERROR: Case labels with binary operators are unsupported : GreaterThan
    0:
                    // _startTime is later: just cut the ut
                    UT = timeDifference / 60;
                    break;
                default:
                    throw new CustomExceptions.dateRangeException();
            }
            */
            if (timeDifference < 0)
            {
                UT = 0;
                DT = (this.endTime - startTime).TotalMinutes; //DateDiff(DateInterval.Second, startTime, _endTime) / 60;
                this.startTime = startTime;

            }
            else if (timeDifference == 0)
            {

            }
            else
            {
                UT = timeDifference / 60;

            }


        }

        public void adjustMyEndTime(DateTime endTimeX)
        {
            double timeDifference = 0;
            timeDifference = (this.startTime - endTimeX).TotalSeconds; //DateDiff(DateInterval.Second, endTimeX, _startTime);

            if (timeDifference < 0)
            {
                this.DT = (-1 * timeDifference) / 60;
                this.endTime = endTimeX;
            }
            else if (timeDifference == 0)
            {
                this.endTime = endTimeX;
                this.DT = 0;
                isUnplanned = false;
            }
            else
            {
                this.endTime = endTimeX;
                this.DT = 0;
                isUnplanned = false;
                UT = (endTimeX - startTime_UT).TotalMinutes;

            }



        }
        #endregion

        #region Sortable/Equitable
        public int CompareTo(DTevent Other)
        {
            /*  if (Obj == null) return 1;
              DTevent OtherEvent = Obj as DTevent;
              if (OtherEvent != null)
                  return this.startTime.CompareTo(OtherEvent.startTime);
              else
                  throw new ArgumentException("Object is not a Downtime Event!");
              */
            // return this._startTime.CompareTo(OtherEvent.startTime);

            if (_isStandardSort)
            {
                return this.startTime.CompareTo(Other.startTime);
            }
            else
            {
                switch (_sortField)
                {
                    case DowntimeField.DT:
                        return this.DT.CompareTo(Other.DT);
                    case DowntimeField.UT:
                        return this.UT.CompareTo(Other.UT);
                    case DowntimeField.Reason1:
                        return this.Reason1.CompareTo(Other.Reason1);
                    case DowntimeField.Reason2:
                        return this.Reason2.CompareTo(Other.Reason2);
                    case DowntimeField.Reason3:
                        return this.Reason3.CompareTo(Other.Reason3);
                    case DowntimeField.Reason4:
                        return this.Reason4.CompareTo(Other.Reason4);
                    case DowntimeField.endTime:
                        return this.endTime.CompareTo(Other.endTime);
                    case DowntimeField.Fault:
                        return this.Fault.CompareTo(Other.Fault);
                    case DowntimeField.ProductCode:
                        return this.ProductCode.CompareTo(Other.ProductCode);
                    case DowntimeField.ProductGroup:
                        return this.ProductGroup.CompareTo(Other.ProductGroup);
                    default:
                        return 0;  //<- this should maybe be an error?
                }
            }
        }
        /*
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            DTevent objAsPart = obj as DTevent;
            if (objAsPart == null)
            {
                return false;
            }
            else
            {
                return Equals(objAsPart);
            }
        }*/
        public bool Equals(DTevent other)
        {
            if (other.startTime >= this.startTime_UT & other.startTime < this.endTime)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        #endregion
    }

    public class DTeventSummary : IComparable<DTeventSummary>, IEquatable<DTeventSummary>, IEquatable<xSigma_Event>
    {
        #region Variables
        public string Name { get; set; }
        public double DT { get; set; }
        public double UT { get; set; }
        public double DT_display { get { return Math.Round(DT, 1); } }
        public double UT_display { get { return Math.Round(UT, 1); } }
        public DowntimeField Field { get; set; } = DowntimeField.NA;
        public double Stops { get; set; } = 0;
        public double SPD { get { return (this.SchedTime == 0) ? 0 : 1440 * this.Stops / this.SchedTime; } }
        public double SchedTime { get; set; } = 0;
        public double DTpct { get { return (this.SchedTime == 0) ? 0 : this.DT / this.SchedTime; } }
        public double MTTR { get { return (this.Stops == 0) ? 0 : this.DT / this.Stops; } }
        public double MTBF { get { return (this.Stops == 0) ? 0 : this.UT / this.Stops; } }
        public double Availability { get { return UT / (DT + UT); } } // MTBF/(MTBF+MTTR)
        public double SortParam { get; set; }
        public double SortParamSecondary { get; set; }
        public List<int> RawRows { get; set; } = new List<int>();

        #region Crystal Ball
        public double SchedTimeSim { get; set; }
        public double DTsim { get; set; }
        public double StopsSim { get; set; }
        public double ScaleFactor_Stops { get; set; }
        public double ScaleFactor_DT { get; set; }
        public double DTpctSim { get { return DTsim / SchedTimeSim; } }
        public double MTTRsim { get { return DTsim / Stops; } }//removed from stops sim to separate this
        public double MTBFsim { get { return UT / StopsSim; } }
        public double AvailabilitySim { get { return UT / (UT + DTsim); } }
        public double AvailabilitySim_System { get { return (1 - AvailabilitySim) / AvailabilitySim; } }
        public double SPDsim { get { return StopsSim * 1440 / SchedTime; } }
        public double CrystalBall_simNewScaleFactors(double ScaleStops, double ScaleDT) //returns delta
        {
            double DTsim_original = DTsim;

            ScaleFactor_Stops = ScaleFactor_Stops * ScaleStops;
            ScaleFactor_DT = ScaleFactor_DT * ScaleDT;

            DTsim = DTsim * ScaleDT;
            StopsSim = StopsSim * ScaleStops;

            return (DTsim - DTsim_original);
        }
        public void CrystalBall_initialize() //also reset
        {
            DTsim = DT;
            StopsSim = Stops;
            ScaleFactor_DT = 1;
            ScaleFactor_Stops = 1;
            SchedTimeSim = SchedTime;
            // DTpctSim = DTpct;
        }
        #endregion
        #endregion

        public override string ToString()
        {
            return Name + " %/S: " + Math.Round(100 * DTpct, 1) + "/" + Stops;
        }

        public double getKPI(DowntimeMetrics Metric, double scheduledTime, double Uptime)
        {
            switch (Metric)
            {
                case DowntimeMetrics.DT:
                    return this.DT;
                case DowntimeMetrics.DTpct:
                    return (this.DT / scheduledTime) * 100;
                case DowntimeMetrics.MTTR:
                    return this.MTTR;
                case DowntimeMetrics.MTBF:
                    return (Uptime / this.Stops);
                case DowntimeMetrics.SPD:
                    return (this.Stops * 1440) / scheduledTime;
                case DowntimeMetrics.Stops:
                    return this.Stops;
                default:
                    return this.DT;
            }
        }

        public double getKPI(DowntimeMetrics Metric)
        {
            switch (Metric)
            {
                case DowntimeMetrics.DT:
                    return this.DT;
                case DowntimeMetrics.DTpct:
                    return this.DTpct * 100;
                case DowntimeMetrics.MTTR:
                    return this.MTTR;
                case DowntimeMetrics.MTBF:
                    return this.MTBF;
                case DowntimeMetrics.SPD:
                    return this.SPD;
                case DowntimeMetrics.Stops:
                    return this.Stops;
                default:
                    return this.DT;
            }
        }
        public double getKPI_Sim(DowntimeMetrics Metric)
        {
            switch (Metric)
            {
                case DowntimeMetrics.DT:
                    return this.DTsim;
                case DowntimeMetrics.DTpct:
                    return this.DTpctSim * 100;
                case DowntimeMetrics.MTTR:
                    return this.MTTRsim;
                case DowntimeMetrics.MTBF:
                    return this.MTBFsim;
                case DowntimeMetrics.SPD:
                    return this.SPDsim;
                case DowntimeMetrics.Stops:
                    return this.StopsSim;
                default:
                    return this.DTsim;
            }
        }

        public void addStop(double DT) { this.Stops += 1; this.DT += DT; }
        public void addStopWithRow(int Row, double DT) { addStop(DT); RawRows.Add(Row); }

        #region Constructor
        public DTeventSummary(string Name) { this.Name = Name; }
        public DTeventSummary(string Name, DowntimeField Field)
        {
            this.Name = Name;
            this.Field = Field;
        }
        public DTeventSummary(string Name, double DT, double Stops)
        {
            this.Name = Name;
            this.DT = DT;
            this.Stops = Stops;

            this.SortParam = DT;
            this.SortParamSecondary = Stops;
        }

        public DTeventSummary(int Row, string Name, double DT)
        {
            this.Name = Name;
            this.DT = DT;
            this.Stops = 1;
            RawRows.Add(Row);
        }
        #endregion

        #region Sorting/Equality/Implicit Conversion
        public static implicit operator xSigma_Event(DTeventSummary E)
        {
            return new xSigma_Event(E.Name, E.Field);
        }
        public void setSortParam(DowntimeMetrics targetKPI)
        {
            switch (targetKPI)
            {
                case DowntimeMetrics.Stops:
                    SortParam = this.Stops;
                    SortParamSecondary = this.DT;
                    break;
                case DowntimeMetrics.MTBF:
                    SortParam = this.MTBF;
                    SortParamSecondary = this.DT;
                    break;
                case DowntimeMetrics.DT:
                    SortParam = this.DT;
                    SortParamSecondary = this.Stops;
                    break;
                case DowntimeMetrics.DTpct:
                    SortParam = DTpct;
                    SortParamSecondary = Stops;
                    break;
                case DowntimeMetrics.MTTR:
                    SortParam = MTTR;
                    SortParamSecondary = DT;
                    break;
                case DowntimeMetrics.SPD:
                    SortParam = SPD;
                    SortParamSecondary = DT;
                    break;
                default:
                    System.Diagnostics.Debugger.Break();
                    break;
            }
        }
        public bool Equals(DTeventSummary other)
        {
            if (other == null)
            {
                return false;
            } //if they have the same name and they're the same field, they're the same!
            return (this.Name.Equals(other.Name) && this.Field == other.Field);
        }
        public bool Equals(xSigma_Event other)
        {
            if (other.Name == this.Name & other.Field == this.Field)
            {
                return true;
            }
            else { return false; }
        }
        public virtual int CompareTo(DTeventSummary Other)
        {
            if (Other.SortParam == SortParam)
            {
                if (Other.SortParamSecondary == SortParamSecondary)
                {
                    return Name.CompareTo(Other.Name);
                }
                else if (Other.SortParamSecondary > SortParamSecondary)
                {
                    return 1;
                }
                else
                {
                    return -1;
                }
            }
            else if (Other.SortParam > SortParam)
            {
                return 1;
            }
            else
            {
                return -1;
            }
        }

        #endregion


    }
}
