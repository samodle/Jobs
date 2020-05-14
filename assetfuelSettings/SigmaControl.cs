using System;
using System.Collections.Generic;
using System.Linq;
using DataInterface;
using static assetfuelSettings.GlobalConstants;

namespace Analytics
{    
    public class CS_Analysis
    {
        #region Variables
        public List<CS_Data> DataFields { get; set; } = new List<CS_Data>();
        private DateTime startTime { get; set; }
        public double AnalysisPeriod { get; set; }

        private List<double> DailyStops;
        #endregion
        #region Construction
        public CS_Analysis(List<double> rawData, DateTime startTime, List<double> rawDT, double AnalysisPeriod)
        {
            StabilityAnalysis tmpAnalysis;
            this.startTime = startTime;
            this.AnalysisPeriod = AnalysisPeriod;
            this.DailyStops = rawData;
            double DaysToAnalyze = rawData.Count - AnalysisPeriod;
            if(DaysToAnalyze >= 0)
            {
                for(int i = 0; i <= DaysToAnalyze; i++)
                {
                    DataFields.Add(analyzeRawData(rawData.GetRange(i, (int)AnalysisPeriod)));
                    DataFields[i].RawMetric = rawData[i + (int)AnalysisPeriod-1];
                    DataFields[i].StartDate = startTime.AddDays(i + AnalysisPeriod);
                    DataFields[i].EndDate = DataFields[i].StartDate.AddDays(1);
                    DataFields[i].RawDT = rawDT[i + (int)AnalysisPeriod-1];
                    /* find stability score */
                    tmpAnalysis = new StabilityAnalysis(DataFields[i].RawMean, DataFields[i].RawStdDev, ControlRulesets.Nelson);
                    DataFields[i].Stability_Score = tmpAnalysis.getStabilityScore(rawData.GetRange(i, (int)AnalysisPeriod), MaxScore: 3);
                /* fin stability score */
                }
            }
            else {/*not sure what to do here but this is a problem*/ }
        }
        #endregion

        private CS_Data analyzeRawData(List<double> rawData)
        {
            var tmpData = new CS_Data();
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
    public class CS_Data: IEquatable<CS_Data>
    {
        #region Variables
        public double RawMean { get; set; }
        public double RawStdDev { get; set; }
        public double AdjMean { get; set; }
        public double AdjStdDev { get; set; }
        public double RawMetric { get; set; }
        public double AdjDistFromMean { get; set; }
        public double CS_Score { get; set; }
        public double Stability_Score { get; set; } //= 0; //THIS SHOULDNT BE DEFAULT 0!!!
        public double RawDT { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        #endregion
        #region Constructor
        public CS_Data() { }
        public CS_Data(DateTime StartDate) { this.StartDate = StartDate; this.EndDate = StartDate; }
        #endregion
        public override string ToString()
        {
            return "S " + Stability_Score + " CS: " + Math.Round(CS_Score,1) + " DT: " + Math.Round(this.RawDT,1) + " " + StartDate.ToString();
        }

        public bool Equals(CS_Data other)
        {
            if (other.StartDate >= this.StartDate & other.EndDate < this.EndDate)
                { return true; }
            else { return false; }
        }
    }


    //single failure mode, CS score for different days
    public class CS_Event:  IEquatable<CS_Event>, IEquatable<DTeventSummary>
    {
        #region Variables
        public string Name { get; set; }
        public DowntimeField Field { get; set; }
        public List<CS_Data> BaselineData { get; set; }
        #endregion

        public CS_Event getSubset(DateTime targetTime)
        {
            List<CS_Data> tmpList = new List<CS_Data>();
            tmpList.Add(BaselineData[BaselineData.IndexOf(new CS_Data(targetTime))]);
            return new CS_Event(this.Name, this.Field, tmpList);
        }

        public override string ToString()
        {
            return this.Name;
        }

        #region Construction
        public CS_Event(string Name, DowntimeField Field)
        {
            this.Name = Name;
            this.Field = Field;
        }
        public CS_Event(string Name, DowntimeField Field, List<CS_Data> rawData):this(Name, Field)
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
        private void setCSscore(CS_Data rawDatum)
        {

            rawDatum.AdjDistFromMean = Math.Abs((rawDatum.RawMetric - rawDatum.AdjMean) / rawDatum.AdjStdDev);

            if (rawDatum.AdjDistFromMean < 1)
            {
                if (rawDatum.AdjDistFromMean < 0.3)
                {
                    rawDatum.CS_Score = 1 - rawDatum.AdjDistFromMean;
                    //1
                    //between .3 and .6
                }
                else if (rawDatum.AdjDistFromMean < 0.6)
                {
                    rawDatum.CS_Score = 1.6 + rawDatum.AdjDistFromMean;//< 0.6;
                                                     //2
                                                     //If rawDatum.AdjDistFromMean < 0.75 Then
                }
                else
                {
                    rawDatum.CS_Score = 2.2 + rawDatum.AdjDistFromMean;
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
                    rawDatum.CS_Score = 2.8 + rawDatum.AdjDistFromMean;
                    //4
                    //1.5 to 1.7
                }
                else
                {
                    rawDatum.CS_Score = 3.4 + rawDatum.AdjDistFromMean;
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
                    rawDatum.CS_Score = 3.9 + rawDatum.AdjDistFromMean;
                    //6
                    // 2.2 to 2.6
                }
                else if (rawDatum.AdjDistFromMean < 2.7)
                {
                    rawDatum.CS_Score = 4.6 + rawDatum.AdjDistFromMean;
                    //7
                }
                else
                {
                    rawDatum.CS_Score = 5.2 + rawDatum.AdjDistFromMean;
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
                rawDatum.CS_Score = 5.5 + rawDatum.AdjDistFromMean;
                ////             default:
            }
            else
            {
                //10
                rawDatum.CS_Score = 10;
            }










        }

        #endregion

        #region Equitable / Implicit Conversion
        public static implicit operator DTeventSummary(CS_Event E)
        {
            return new DTeventSummary(E.Name, E.Field);
        }
        public bool Equals(CS_Event other)
        {
            if (other.Name == this.Name & other.Field == this.Field)
            {
                return true;
            } else { return false;  }            
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


    public class CS_PeriodicSystemReport
    {
        #region Variables
        public DateTime StartTime { get; set; }
        // public DateTime EndTime { get; set; }
        public List<CS_Event> DataList { get; set; } = new List<CS_Event>();
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
        public double DTPct_NotUS { get {return (DTpct - DTpct_Stable - DTpct_Unstable); } }
        public double DT_Stable { get; set; } = 0;
        public double DT_Unstable { get; set; } = 0;
        public double DT_NotUS { get { return (DT - DT_Stable - DT_Unstable); } }
        #endregion

        #region Constructor
        public CS_PeriodicSystemReport() { }
        public CS_PeriodicSystemReport(DateTime startTime) { this.StartTime = startTime; }
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
                if (DataList[i].BaselineData[0].CS_Score >= 5)
                {
                    DT_Sporadic += tmpDT;
                }
                else if (DataList[i].BaselineData[0].CS_Score <= 3)
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
