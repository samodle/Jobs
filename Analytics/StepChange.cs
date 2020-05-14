using System;
using System.Collections.Generic;

namespace Analytics
{
    internal static class StepChangeAnalysis
    {
        internal static List<List<List<List<double>>>> getStepChangeForSeries(List<List<List<List<double>>>> rawValues)
        {
            var returnList = new List<List<List<List<double>>>>();
            for (int i = 0; i < rawValues.Count; i++)
            {
                returnList.Add(getStepChangeForSeries(rawValues[i]));
            }
            return returnList;
        }

        internal static List<List<List<double>>> getStepChangeForSeries(List<List<List<double>>> rawValues)
        {
            var returnList = new List<List<List<double>>>();
            for (int i = 0; i < rawValues.Count; i++)
            {
                returnList.Add(getStepChangeForSeries(rawValues[i]));
            }
            return returnList;
        }

        internal static List<List<double>> getStepChangeForSeries(List<List<double>> rawValues)
        {
            var returnList = new List<List<double>>();
            for (int i = 0; i < rawValues.Count; i++)
            {
                returnList.Add(getStepChangeForSeries(rawValues[i]));
            }
            return returnList;
        }

        internal static List<double> getStepChangeForSeries(List<double> rawValues)
        {
            var tmpData = new List<double>();
            foreach (double value in rawValues)
            {
                tmpData.Add(value);
            }
            return getStepChangeForSeries_NoRounding(tmpData);
        }

        private static List<double> getStepChangeForSeries_NoRounding(List<double> rawValues)
        {
            double instaMSE; double instaSum = 0;
            int lastFlagIndex;
            int ContiPos = 0; int ContiNeg = 0;
            int max = rawValues.Count - 1;
            int ComPeriod = 7; //depends on system 'noise'
            int ComPeriodIncrement = 1; int k;
            int NumberOfIterations = 30;
            double SuccessFactor = 0.7;
            int preferredIteration = -1;

            var MovingAverage = new double[rawValues.Count];

            var MSE_Values = new double[NumberOfIterations];
            var WorkSheet = new double[NumberOfIterations, rawValues.Count];
            var tmpList = new List<double>();

            for (int rawDataInc = 0; rawDataInc < rawValues.Count; rawDataInc++)
            {
                instaSum += rawValues[rawDataInc];
                MovingAverage[rawDataInc] = instaSum / (rawDataInc + 1);
            }

            for (int IterCount = 0; IterCount < NumberOfIterations; IterCount++)
            {
                var Inflection = new bool[rawValues.Count];
                lastFlagIndex = 0;
                for (int i = 0; i < rawValues.Count; i++)
                {
                    /* ----- Identify Inflection Point(80 % out of next y) ----- */
                    if (i - lastFlagIndex > ComPeriod)
                    {
                        if (i + ComPeriod > (rawValues.Count - 1))
                        {
                            ComPeriod = rawValues.Count - 1 - i;
                        }
                        /* ----- Positive ----- */
                        if (MovingAverage[i] > rawValues[i])
                        {
                            for (int i1 = i; i1 < Math.Min(i + ComPeriod, rawValues.Count - 1); i1++)
                            {
                                if (MovingAverage[i1] > rawValues[i1]) { ContiPos += 1; }
                            }
                            int i1_x = Math.Min(i + ComPeriod, rawValues.Count - 1);
                            if (ContiPos > SuccessFactor * ComPeriod)
                            {
                                Inflection[i1_x] = true;
                                lastFlagIndex = i1_x;
                            }
                        }

                        /* ----- Negative ----- */
                        if (MovingAverage[i] < rawValues[i])
                        {
                            for (int i1 = i; i1 < Math.Min(i + ComPeriod, rawValues.Count - 1); i1++)
                            {
                                if (MovingAverage[i1] < rawValues[i1]) { ContiNeg += 1; }
                            }
                            int i1_x = Math.Min(i + ComPeriod, rawValues.Count - 1);
                            if (ContiNeg > SuccessFactor * ComPeriod)
                            {
                                Inflection[i1_x] = true;
                                lastFlagIndex = i1_x;
                            }
                        }
                    }
                }

                k = 0;
                instaSum = 0;
                instaMSE = 0;
                for (int j = 0; j < rawValues.Count; j++)
                {
                    instaSum = instaSum + rawValues[j];
                    if (Inflection[j] || j == max + 1)
                    {
                        for (int l = k; l <= j; l++)
                        {
                            WorkSheet[IterCount, l] = instaSum / (j + 1 - k);
                            instaMSE = instaMSE + Math.Pow(((instaSum / (j + 1 - k)) - rawValues[l]), 2);
                        }
                        k = j + 1;
                        instaSum = 0;
                    }
                }
                MSE_Values[IterCount] = instaMSE / max;
                ComPeriod += ComPeriodIncrement;
            }

            //finding the preferred iteration.. first check for an inflection point, if no inflection point, take the first iteration MSE
            for (int p = 0; p < NumberOfIterations; p++)
            {
                preferredIteration = p;
                if (p != 0 && MSE_Values[p] > MSE_Values[p - 1])
                {
                    preferredIteration = p;
                    break;// p = NumberOfIterations + 1;
                }
                if (p == NumberOfIterations - 1 && MSE_Values[0] > MSE_Values[p])
                {
                    preferredIteration = 0;
                }
            }

            //copying the step change list for the preferred iteration to a temp list that is fedback to the UI.
            for (int i = 0; i < rawValues.Count; i++)
            {
                tmpList.Add(WorkSheet[preferredIteration, i]);
                // tmpList.Add(WorkSheet[NumberOfIterations - 1, i]);// preferredIteration, i]);
            }

            return tmpList;
        }
    }
}
