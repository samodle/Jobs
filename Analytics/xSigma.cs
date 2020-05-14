using DataInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using static ForkAnalyticsSettings.GlobalConstants;

namespace Analytics
{
    //for the listview in the UI
    public class xSigma_DisplayEvent
    {
        public string Name { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double DT { get; set; }
        public string DTpct { get; set; }
        public double Stops { get; set; } = 0;
        public double MTBF
        {
            get { return Stops == 0 ? 0 : Math.Round(UT / Stops, 1); }
        }

        public double UT { get; set; } = 0;
        public double xSigma_Score { get; set; } = 0;
        public string xSigma_String
        {
            get
            {
                if (xSigma_Score > 5)
                {
                    return "Sporadic";
                }
                else
                {
                    return "Chronic";
                }
            }
        }

        public xSigma_DisplayEvent(string Name, double Stops, double UT, double DTpct, double xSigma_Score)
        {
            this.Name = Name;
            this.Stops = Stops;
            this.UT = UT;
            this.DTpct = Math.Round(DTpct, 1) + "%";
            this.xSigma_Score = xSigma_Score;
        }


        public xSigma_DisplayEvent(string Name, DateTime StartTime, DateTime EndTime, double UT, double DT)
        {
            this.Name = Name;
            this.StartTime = StartTime;
            this.EndTime = EndTime;
            this.UT = UT;
            this.DT = DT;
        }
    }


    public class xSigma_Analysis
    {
        #region Variables
        public List<xSigma_Data> DataFields { get; set; } = new List<xSigma_Data>();
        private DateTime startTime { get; set; }
        public double AnalysisPeriod { get; set; }

        private List<double> DailyStops;
        #endregion
        #region Construction
        public xSigma_Analysis(List<double> rawData, DateTime startTime, List<double> rawDT, double AnalysisPeriod)
        {
            StabilityAnalysis tmpAnalysis;
            this.startTime = startTime;
            this.AnalysisPeriod = AnalysisPeriod;
            this.DailyStops = rawData;
            double DaysToAnalyze = rawData.Count - AnalysisPeriod;
            if (DaysToAnalyze >= 0)
            {
                for (int i = 0; i <= DaysToAnalyze; i++)
                {
                    DataFields.Add(analyzeRawData(rawData.GetRange(i, (int)AnalysisPeriod)));
                    DataFields[i].RawMetric = rawData[i + (int)AnalysisPeriod - 1];
                    DataFields[i].StartDate = startTime.AddDays(i + AnalysisPeriod);
                    DataFields[i].EndDate = DataFields[i].StartDate.AddDays(1);
                    DataFields[i].RawDT = rawDT[i + (int)AnalysisPeriod - 1];
                    /* find stability score */
                    tmpAnalysis = new StabilityAnalysis(DataFields[i].RawMean, DataFields[i].RawStdDev, ControlRulesets.Nelson);
                    DataFields[i].Stability_Score = tmpAnalysis.getStabilityScore(rawData.GetRange(i, (int)AnalysisPeriod), MaxScore: 3);
                    /* fin stability score */
                }
            }
            else {/*not sure what to do here but this is a problem*/ }
        }
        #endregion

        private xSigma_Data analyzeRawData(List<double> rawData)
        {
            var tmpData = new xSigma_Data();
            var Squares = new List<double>();
            var filteredList = new List<double>();
            double SquareAvg = 0;
            double tmpMeanDist = 0;
            //get the raw average
            tmpData.RawMean = rawData.Average();
            //use it to find dat std dev
            foreach (double value in rawData)
            {
                Squares.Add(Math.Pow(value - tmpData.RawMean, 2));
            }
            SquareAvg = Squares.Average();
            tmpData.RawStdDev = Math.Sqrt(SquareAvg);
            //LUKE! We're gonna have company!
            Squares.Clear();
            for (int i = 0; i < rawData.Count; i++)
            {
                tmpMeanDist = (rawData[i] - tmpData.RawMean) / tmpData.RawStdDev;
                if ((DailyStops[i] > 10 & tmpMeanDist > 2.5) | (DailyStops[i] < 11 & tmpMeanDist > 3))
                {
                    //Uh, everythings under control. Situation normal.
                }
                else
                {
                    filteredList.Add(rawData[i]);
                }
            }
            //are you not entertained? (we're gonna find it again)
            tmpData.AdjMean = filteredList.Average();
            foreach (double value in filteredList)
            {
                Squares.Add(Math.Pow(value - tmpData.AdjMean, 2));
            }
            SquareAvg = Squares.Average();
            tmpData.AdjStdDev = Math.Sqrt(SquareAvg);
            return tmpData;
        }
    }

    //holds daily mean & dev info for CS analysis
    //ie for one time period, all net data points
    //instance of xSigma_Event contains 'List<xSigma_Data> BaselineData'
    public class xSigma_Data : IEquatable<xSigma_Data>
    {
        #region Variables
        public double RawMean { get; set; }
        public double RawStdDev { get; set; }
        public double AdjMean { get; set; }
        public double AdjStdDev { get; set; }
        public double RawMetric { get; set; }
        public double AdjDistFromMean { get; set; }
        public double xSigma_Score { get; set; }
        public double Stability_Score { get; set; }
        public double RawDT { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        #endregion
        #region Constructor
        public xSigma_Data() { }
        public xSigma_Data(DateTime StartDate) { this.StartDate = StartDate; this.EndDate = StartDate; }
        #endregion
        public override string ToString()
        {
            return "S " + Stability_Score + " CS: " + Math.Round(xSigma_Score, 1) + " DT: " + Math.Round(this.RawDT, 1) + " " + StartDate.ToString();
        }

        public bool Equals(xSigma_Data other)
        {
            if (other.StartDate >= this.StartDate & other.EndDate < this.EndDate)
            { return true; }
            else { return false; }
        }
    }

    //single failure mode, CS score for different days
    //cs periodic event summary has a list of this
    public class xSigma_Event : IEquatable<xSigma_Event>, IEquatable<DTeventSummary>
    {
        #region Variables
        public string Name { get; set; }
        public DowntimeField Field { get; set; }
        public List<xSigma_Data> BaselineData { get; set; }
        #endregion

        public xSigma_Event getSubset(DateTime targetTime)
        {
            List<xSigma_Data> tmpList = new List<xSigma_Data>();
            tmpList.Add(BaselineData[BaselineData.IndexOf(new xSigma_Data(targetTime))]);
            return new xSigma_Event(this.Name, this.Field, tmpList);
        }

        public override string ToString()
        {
            return this.Name;
        }

        #region Construction
        public xSigma_Event(string Name, DowntimeField Field)
        {
            this.Name = Name;
            this.Field = Field;
        }
        public xSigma_Event(string Name, DowntimeField Field, List<xSigma_Data> rawData) : this(Name, Field)
        {
            BaselineData = rawData;
        }
        #endregion

        #region Manipulate CS Data
        public void setAllCSscores()
        {
            for (int i = 0; i < BaselineData.Count; i++)
            {
                setCSscore(BaselineData[i]);
            }
        }
        private void setCSscore(xSigma_Data rawDatum)
        {

            rawDatum.AdjDistFromMean = Math.Abs((rawDatum.RawMetric - rawDatum.AdjMean) / rawDatum.AdjStdDev);

            if (rawDatum.AdjDistFromMean < 1)
            {
                if (rawDatum.AdjDistFromMean < 0.3)
                {
                    rawDatum.xSigma_Score = 1 - rawDatum.AdjDistFromMean;
                    //1
                    //between .3 and .6
                }
                else if (rawDatum.AdjDistFromMean < 0.6)
                {
                    rawDatum.xSigma_Score = 1.6 + rawDatum.AdjDistFromMean;//< 0.6;
                                                                           //2
                                                                           //If rawDatum.AdjDistFromMean < 0.75 Then
                }
                else
                {
                    rawDatum.xSigma_Score = 2.2 + rawDatum.AdjDistFromMean;
                    //3
                }
                ////                 break;
                ////               case  // ERROR: Case labels with binary operators are unsupported : LessThan
                ////  1.8:
            }
            else if (rawDatum.AdjDistFromMean < 1.8)
            {
                //1, 1.1, - 1.7
                // 1, 1.2, 1.3, 1.4
                if (rawDatum.AdjDistFromMean < 1.5)
                {
                    rawDatum.xSigma_Score = 2.8 + rawDatum.AdjDistFromMean;
                    //4
                    //1.5 to 1.7
                }
                else
                {
                    rawDatum.xSigma_Score = 3.4 + rawDatum.AdjDistFromMean;
                    //5 
                }
                ////            break;
                ////        case  // ERROR: Case labels with binary operators are unsupported : LessThan
                ////  3:
            }
            else if (rawDatum.AdjDistFromMean < 3)
            {
                //1.8 to 2.1
                if (rawDatum.AdjDistFromMean < 2.2)
                {
                    rawDatum.xSigma_Score = 3.9 + rawDatum.AdjDistFromMean;
                    //6
                    // 2.2 to 2.6
                }
                else if (rawDatum.AdjDistFromMean < 2.7)
                {
                    rawDatum.xSigma_Score = 4.6 + rawDatum.AdjDistFromMean;
                    //7
                }
                else
                {
                    rawDatum.xSigma_Score = 5.2 + rawDatum.AdjDistFromMean;
                    //8
                }
                ////                   break;
                ////               case  // ERROR: Case labels with binary operators are unsupported : LessThan
                //// 4:

            }
            else if (rawDatum.AdjDistFromMean < 4)
            {
                //3 to 3.9
                //9
                rawDatum.xSigma_Score = 5.5 + rawDatum.AdjDistFromMean;
                ////             default:
            }
            else
            {
                //10
                rawDatum.xSigma_Score = 10;
            }










        }

        #endregion

        #region Equitable / Implicit Conversion
        public static implicit operator DTeventSummary(xSigma_Event E)
        {
            return new DTeventSummary(E.Name, E.Field);
        }
        public bool Equals(xSigma_Event other)
        {
            if (other.Name == this.Name & other.Field == this.Field)
            {
                return true;
            }
            else { return false; }
        }
        public bool Equals(DTeventSummary other)
        {
            if (other.Name == this.Name & other.Field == this.Field)
            {
                return true;
            }
            else { return false; }
        }
        #endregion
    }

    //for a single period, all the xSigma Information
    public class xSigma_PeriodicSystemReport
    {
        #region Variables
        public DateTime StartTime { get; set; }
        // public DateTime EndTime { get; set; }
        public List<xSigma_Event> DataList { get; set; } = new List<xSigma_Event>();
        public double schedTime { get; set; }
        public double DTpct { get { return DT / schedTime; } }
        private double DT { get; set; } = 0;
        /* Chronic vs. Sporadic */
        public double DTPct_Chronic { get { return DT_Chronic / schedTime; } }
        public double DTPct_Sporadic { get { return DT_Sporadic / schedTime; } }
        public double DTPct_NotCS { get { return (DTpct - DTPct_Chronic - DTPct_Sporadic); } }
        public double DT_Chronic { get; set; } = 0;
        public double DT_Sporadic { get; set; } = 0;
        public double DT_NotCS { get { return (DT - DT_Chronic - DT_Sporadic); } }
        /* Stable vs. Unstable */
        public double DTpct_Stable { get { return DT_Stable / schedTime; } }
        public double DTpct_Unstable { get { return DT_Unstable / schedTime; } }
        public double DTPct_NotUS { get { return (DTpct - DTpct_Stable - DTpct_Unstable); } }
        public double DT_Stable { get; set; } = 0;
        public double DT_Unstable { get; set; } = 0;
        public double DT_NotUS { get { return (DT - DT_Stable - DT_Unstable); } }
        #endregion

        #region Constructor
        public xSigma_PeriodicSystemReport() { }
        public xSigma_PeriodicSystemReport(DateTime startTime) { this.StartTime = startTime; }
        #endregion

        public void calculateAllParameters()
        {
            calculateCSparameters();
            calculateStabilityParameters();
        }

        private void calculateCSparameters()
        {
            double tmpDT;
            for (int i = 0; i < DataList.Count; i++)
            {
                tmpDT = DataList[i].BaselineData[0].RawDT;
                DT += tmpDT;
                if (DataList[i].BaselineData[0].xSigma_Score >= 5)
                {
                    DT_Sporadic += tmpDT;
                }
                else if (DataList[i].BaselineData[0].xSigma_Score <= 3)
                {
                    DT_Chronic += tmpDT;
                }
            }
        }

        private void calculateStabilityParameters()
        {
            double tmpDT;
            for (int i = 0; i < DataList.Count; i++)
            {
                tmpDT = DataList[i].BaselineData[0].RawDT;
                DT += tmpDT;
                if (DataList[i].BaselineData[0].Stability_Score >= 5)
                {
                    DT_Sporadic += tmpDT;
                }
                else if (DataList[i].BaselineData[0].Stability_Score <= 3)
                {
                    DT_Chronic += tmpDT;
                }
            }
        }

    }
}
