using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;

namespace Analytics
{
    public class RateTrainerAnalysis
    {
        #region Variables
        public double Baseline_OEE { get; set; }
        public double OEE_Insensitive { get; set; }
        public double SystemSensitivity { get; set; }
        public double Baseline_Rate { get; set; }
        //   public double OEE_dt_factor { get; set; }
        //   public double OEE_Insensitive_factor { get; set; }
        //   public double unit_cost { get; set; }
        internal double[] Values_X_Axis = new double[61];

        internal double[] Values_2_OEE = new double[61];
        internal double[] Values_4_OEE = new double[61];
        internal double[] Values_8_OEE = new double[61];
        internal double[] Values_12_OEE = new double[61];
        internal double[] Values_16_OEE = new double[61];

        internal double[] Values_2_Tput = new double[61];
        internal double[] Values_4_Tput = new double[61];
        internal double[] Values_8_Tput = new double[61];
        internal double[] Values_12_Tput = new double[61];
        internal double[] Values_16_Tput = new double[61];

        internal double[] Values_2_Cost = new double[61];
        internal double[] Values_4_Cost = new double[61];
        internal double[] Values_8_Cost = new double[61];
        internal double[] Values_12_Cost = new double[61];
        internal double[] Values_16_Cost = new double[61];

        #endregion

        #region Max/Mins Of Output
        internal double getMaxTput()
        {
            var evalList = new List<double>();
            evalList.Add(Values_2_Tput.Max());
            evalList.Add(Values_4_Tput.Max());
            evalList.Add(Values_8_Tput.Max());
            evalList.Add(Values_12_Tput.Max());
            evalList.Add(Values_16_Tput.Max());
            return evalList.Max();
        }
        internal double getMinTput()
        {
            var evalList = new List<double>();
            evalList.Add(Values_2_Tput.Min());
            evalList.Add(Values_4_Tput.Min());
            evalList.Add(Values_8_Tput.Min());
            evalList.Add(Values_12_Tput.Min());
            evalList.Add(Values_16_Tput.Min());
            return evalList.Min();
        }

        internal double getMaxOEE()
        {
            var evalList = new List<double>();
            evalList.Add(Values_2_OEE.Max());
            evalList.Add(Values_4_OEE.Max());
            evalList.Add(Values_8_OEE.Max());
            evalList.Add(Values_12_OEE.Max());
            evalList.Add(Values_16_OEE.Max());
            return evalList.Max();
        }
        internal double getMinOEE()
        {
            var evalList = new List<double>();
            evalList.Add(Values_2_OEE.Min());
            evalList.Add(Values_4_OEE.Min());
            evalList.Add(Values_8_OEE.Min());
            evalList.Add(Values_12_OEE.Min());
            evalList.Add(Values_16_OEE.Min());
            return evalList.Min();
        }
        #endregion

        public Tuple<double, double, double> UpdateSensitivity(double selectedSensitivity, double selectedRate, double scaleMin = 0, double scaleMax = 10)
        {
            //rescale input sensitivity to be between 2 and 16
            double selectedPCT = selectedSensitivity / (scaleMax - scaleMin);
            double n = (16 - 2) * selectedPCT;

            SystemSensitivity = 2 + n;

            return getOutputForSelectedValues(selectedRate);
        }

        public Tuple<double, double, double> UpdateInsensitiveLoss(double selectedPDT, double selectedRate)
        {
            OEE_Insensitive = selectedPDT / 100;
            GenerateRateAnalysis();

            return getOutputForSelectedValues(selectedRate);

        }

        public Tuple<double, double, double> getOutputForSelectedValues(double selectedRate)
        {
            double tgtOEE = -1;
            double tgtThroughput = -1;
            double tgtMTBF = 50;
            double highVal = 0; double lowVal = 0; double pctDifference = 0;
            double highVal2 = 0; double lowVal2 = 0;

            int rateIndex = 0;
            //firt find the correct index
            double searchValue = selectedRate / Baseline_Rate;
            while (Values_X_Axis[rateIndex] < searchValue)
            {
                rateIndex++;
            }

            //find values
            if (SystemSensitivity < 4)
            {
                highVal = Values_4_OEE[rateIndex];
                lowVal = Values_2_OEE[rateIndex];
                highVal2 = Values_4_Tput[rateIndex];
                lowVal2 = Values_2_Tput[rateIndex];

                pctDifference = (SystemSensitivity - 2) / 2;
            }
            else if (SystemSensitivity < 8)
            {
                highVal = Values_12_OEE[rateIndex];
                lowVal = Values_8_OEE[rateIndex];
                highVal2 = Values_12_Tput[rateIndex];
                lowVal2 = Values_8_Tput[rateIndex];

                pctDifference = (SystemSensitivity - 4) / 4;
            }
            else if (SystemSensitivity < 12)
            {
                highVal = Values_12_OEE[rateIndex];
                lowVal = Values_8_OEE[rateIndex];
                highVal2 = Values_12_Tput[rateIndex];
                lowVal2 = Values_8_Tput[rateIndex];

                pctDifference = (SystemSensitivity - 8) / 4;
            }
            else if (SystemSensitivity < 16)
            {
                highVal = Values_16_OEE[rateIndex];
                lowVal = Values_12_OEE[rateIndex];
                highVal2 = Values_16_Tput[rateIndex];
                lowVal2 = Values_12_Tput[rateIndex];

                pctDifference = (SystemSensitivity - 12) / 4;
            }
            else
            {
                tgtOEE = Values_16_OEE[rateIndex];
                tgtThroughput = Values_16_Tput[rateIndex];
            }

            //if it wasnt 16..
            if (tgtOEE == -1)
            {
                //OEE
                if (highVal > lowVal)
                {
                    tgtOEE = lowVal + ((highVal - lowVal) * pctDifference);
                }
                else if (lowVal > highVal)
                {
                    tgtOEE = lowVal - ((lowVal - highVal) * pctDifference);
                }
                else
                {
                    tgtOEE = highVal;
                }
                //TPUT
                if (highVal2 > lowVal2)
                {
                    tgtThroughput = lowVal2 + ((highVal2 - lowVal2) * pctDifference);
                }
                else if (lowVal2 > highVal2)
                {
                    tgtThroughput = lowVal2 - ((lowVal2 - highVal2) * pctDifference);
                }
                else
                {
                    tgtThroughput = highVal2;
                }
            }

            return new Tuple<double, double, double>(tgtOEE * 100, tgtThroughput * 100, tgtMTBF); ;
        }

        #region Constructor
        public RateTrainerAnalysis(double OEE, double PDT, double Rate)
        {
            Baseline_OEE = OEE;
            SystemSensitivity = 6;
            OEE_Insensitive = PDT;
            Baseline_Rate = Rate;

            //   OEE_dt_factor = 0; //1-OEE factor
            //   OEE_Insensitive_factor = 0; //1/OEE factor
            //   unit_cost = 2; //unit cost ($)

            GenerateRateAnalysis();
        }
        #endregion

        #region Analytics
        internal void GenerateRateAnalysis()
        {
            double n = OEE_Insensitive / (1 - Baseline_OEE - OEE_Insensitive);

            //OEE TRENDS
            for (int rateIncrementer = 0; rateIncrementer <= 60; rateIncrementer++)
            {
                Values_X_Axis[rateIncrementer] = 0.6 + 0.01 * (rateIncrementer - 1);

                //OEE Trends
                Values_2_OEE[rateIncrementer] = getDistribution(2, Values_X_Axis[rateIncrementer], Baseline_OEE, n);
                Values_4_OEE[rateIncrementer] = getDistribution(4, Values_X_Axis[rateIncrementer], Baseline_OEE, n);
                Values_8_OEE[rateIncrementer] = getDistribution(8, Values_X_Axis[rateIncrementer], Baseline_OEE, n);
                Values_12_OEE[rateIncrementer] = getDistribution(12, Values_X_Axis[rateIncrementer], Baseline_OEE, n);
                Values_16_OEE[rateIncrementer] = getDistribution(16, Values_X_Axis[rateIncrementer], Baseline_OEE, n);

                //Tput Trends
                Values_2_Tput[rateIncrementer] = Values_2_OEE[rateIncrementer] * Values_X_Axis[rateIncrementer] / Baseline_OEE;
                Values_4_Tput[rateIncrementer] = Values_4_OEE[rateIncrementer] * Values_X_Axis[rateIncrementer] / Baseline_OEE;
                Values_8_Tput[rateIncrementer] = Values_8_OEE[rateIncrementer] * Values_X_Axis[rateIncrementer] / Baseline_OEE;
                Values_12_Tput[rateIncrementer] = Values_12_OEE[rateIncrementer] * Values_X_Axis[rateIncrementer] / Baseline_OEE;
                Values_16_Tput[rateIncrementer] = Values_16_OEE[rateIncrementer] * Values_X_Axis[rateIncrementer] / Baseline_OEE;
            }

        }

        internal static double getDistribution(double Beta, double Xpoint, double OEE, double N)
        {
            return OEE / (OEE + (1 - OEE) * (N + Math.Exp(Beta * (Xpoint - 1))) / (N + 1));
        }
        #endregion
    }

}