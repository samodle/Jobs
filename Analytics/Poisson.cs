using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows_Desktop;

namespace Analytics
{

    public class DependencyEvent
    {
        public DependencyEvent(string Type, string PreStopFailureMode, string PostStopFailureMode, double ActExp_Num, double ActExp_Pct, double Act_Num, double Act_Pct, double Exp_Num, double Exp_Pct)
        {
            this.Type = Type;
            this.PreStopFailureMode = PreStopFailureMode;
            this.PostStopFailureMode = PostStopFailureMode;
            this.ActExp_Num = ActExp_Num;
            this.ActExp_Pct = ActExp_Pct;
            this.Act_Num = Act_Num;
            this.Act_Pct = Act_Pct;
            this.Exp_Num = Exp_Num;
            this.Exp_Pct = Exp_Pct;
        }

        public override string ToString()
        {
            return Type + " " + PreStopFailureMode + "-" + PostStopFailureMode + " " + Math.Round(ActExp_Num, 2) + "/" + Math.Round(ActExp_Pct, 2) + "%";
        }

        public string Type { get; set; }
        public string PreStopFailureMode { get; set; }
        public string PostStopFailureMode { get; set; }
        public double ActExp_Num { get; set; }
        public double ActExp_Pct { get; set; }
        public double Act_Num { get; set; }
        public double Act_Pct { get; set; }
        public double Exp_Num { get; set; }
        public double Exp_Pct { get; set; }
    }
    public static class DependencyAnalysis
    {
        public static List<DependencyEvent> executeDependencyAnalysis(List<string> mappedData)
        {
            #region Variables
            object[,] excelSheet_DEP;
            var returnArray = new List<DependencyEvent>();

            double x_expect = 0;
            double pois = 0;
            double lambda = 0;
            double pois1 = 0;
            double y = 0;
            double LogY = 0;
            double rank = 0;
            double x_total_r = 0;
            double x_rows_r = 0;
            double Check = 0;
            double min_take = 0;
            double x_total_i_r = 0;
            double x_total_j_r = 0;
            const double alpha = 0.0013499856;//NORMSDIST(-Z);
            double eLog = 0;
            long x_rows = 0;
            int i = 0;
            long x_max = 0;
            double n_chance = 0;
            int j = 0;
            long m = 0;
            long k = 0;
            long i_list = 0;
            long yExtend = 0;
            long neg_i_list = 0;
            long pos_i_list = 0;
            long x_list_old = 0;
            long x_list = 0;
            double x_actual = 0;
            long x_total = 0;
            double x_total_i = 0;
            long x_total_j = 0;
            long r = 0;
            double sum_actual = 0;
            double sum_expected = 0;
            double neg_sum_actual = 0;
            double neg_sum_expected = 0;
            double pos_sum_actual = 0;
            double pos_sum_expected = 0;

            var RawData = new List<Tuple<string, int>>();
            List<string> Texts = mappedData.Distinct().ToList();
            long[,] Counts = null;
            double[,] Totals = null;

            const int RESULT_COL = 5;
            const int RESULT_ROW = 66;
            const int DEP_TABLE_ROW = 33;
            const int DEP_TABLE_COL = 4;

            const int LAG = 1;
            const int MINIM = 0;

            excelSheet_DEP = new object[4100, 40];

            sum_actual = 0;
            sum_expected = 0;
            neg_sum_actual = 0;
            neg_sum_expected = 0;
            neg_i_list = 0;
            pos_sum_actual = 0;
            pos_sum_expected = 0;
            pos_i_list = 0;
            #endregion

            for (i = 0; i < mappedData.Count; i++)
            {    // First Create List Of Raw Data w/ Unique 'Code' 
                RawData.Add(new Tuple<string, int>(mappedData[i], Texts.IndexOf(mappedData[i])));
            }

            i_list = 1;
            x_rows = mappedData.Count; //xrows is the number of raw data items
            x_max = Texts.Count; //xmax is the number of unique events

            Counts = new long[x_max + 1, x_max];
            Totals = new double[x_max, 3];

            // Build the table of data of actual counts
            for (i = 0; i < x_max; i++) { for (j = 0; j < x_max; j++) { Counts[i, j] = 0; } } //zero it out
            for (i = LAG; i < x_rows; i++)
            {
                //  excelSheet_Raw[x_list_old, TABLE_COL + x_list - 1] = x_actual + 1; //???
                x_list_old = RawData[i - LAG].Item2; //item 2 is unique 'text code' 
                x_list = RawData[i].Item2;
                x_actual = Counts[x_list_old, x_list];
                Counts[x_list_old, x_list] = (long)x_actual + 1;
            }

            for (j = 0; j < x_max; j++)
            { //now sum the totals for each inidvidual
                x_total = 0;
                for (i = 0; i < x_max; i++)
                {
                    x_total += Counts[i, j];
                }
                Counts[x_max, j] = x_total;
            }

            eLog = Math.Log(10);

            //First Pass, just look for Same-Same Dependencies
            for (i = 0; i < x_max; i++)
            {
                j = i;

                x_total_i = Counts[x_max, i];
                x_total_j = Counts[x_max, j];
                x_expect = (x_rows - LAG) * x_total_i * x_total_j / x_rows / x_rows;
                x_actual = Counts[i, j];

                if (((x_expect >= 5) | (x_actual >= 5)) & (x_total_i >= MINIM) & (x_total_j >= MINIM))
                {
                    pois = 0;
                    lambda = x_expect;
                    r = (long)x_actual;
                    if (lambda < 1)
                    {
                        pois1 = 0;
                        for (m = 0; m <= r; m++)
                        {
                            y = 1;
                            yExtend = 0;
                            if (m > 0)
                            {
                                for (k = 1; k <= m; k++)
                                {
                                    LogY = Math.Log10(y);
                                    if ((Math.Abs(LogY) > 300))
                                    {
                                        yExtend = yExtend + Convert.ToInt64(LogY);
                                        y = Math.Pow((10), (LogY - Convert.ToInt64(LogY)));
                                    }
                                    y = y * lambda / k;
                                }
                            }
                            if ((yExtend == 0))
                            {
                                pois1 += y * Math.Exp(-lambda);
                            }
                            else
                            {
                                LogY = Math.Log(y);
                                pois1 += Math.Exp(LogY - lambda + yExtend * eLog);
                            }
                        }
                        if (pois1 > 1)
                        {
                            pois1 = 1;
                        }
                        n_chance += (1 - pois1);
                        lambda = 1;
                    }
                    else
                    {
                        n_chance += alpha;
                    }
                    for (m = 0; m <= r; m++)
                    {
                        y = 1;
                        yExtend = 0;
                        if (m > 0)
                        {
                            for (k = 1; k <= m; k++)
                            {
                                LogY = Math.Log10(y);
                                if ((Math.Abs(LogY) > 300))
                                {
                                    yExtend = yExtend + Convert.ToInt64(LogY);
                                    y = Math.Pow((10), (LogY - Convert.ToInt64(LogY)));
                                }
                                y = y * lambda / k;
                            }
                        }
                        if ((yExtend == 0))
                        {
                            pois = pois + y * Math.Exp(-lambda);
                        }
                        else
                        {
                            LogY = Math.Log(y);
                            pois = pois + Math.Exp(LogY - lambda + yExtend * eLog);
                        }
                    }
                    if (pois > 1)
                    {
                        pois = 1;
                    }
                    rank = Math.Abs(pois - 0.5) + 0.5;
                    if (rank >= 1 - alpha)
                    {
                        //  excelSheet_Raw[x_list_old, TABLE_COL 7 + x_list - 1] = x_actual + 1; //???
                        //  if (excelSheet_Raw[i + 1, MAP_COL + 1] == excelSheet_Raw[j + 1, MAP_COL + 1])
                        if (Texts[i] == Texts[j]) //sro eliminated 1/22/16 tor emove _Raw dependency
                        {
                            excelSheet_DEP[RESULT_ROW - 1 + i_list, RESULT_COL] = "Same-Same";
                        }
                        else
                        {
                            excelSheet_DEP[RESULT_ROW - 1 + i_list, RESULT_COL] = "Not-Same";
                        }

                        excelSheet_DEP[RESULT_ROW - 1 + i_list, RESULT_COL + 1] = Texts[i];

                        excelSheet_DEP[RESULT_ROW - 1 + i_list, RESULT_COL + 2] = Texts[j];
                        excelSheet_DEP[RESULT_ROW - 1 + i_list, RESULT_COL + 3] = x_actual - x_expect;
                        excelSheet_DEP[RESULT_ROW - 1 + i_list, RESULT_COL + 4] = 100.0 * (x_actual - x_expect) / x_rows;
                        excelSheet_DEP[RESULT_ROW - 1 + i_list, RESULT_COL + 5] = x_actual;
                        excelSheet_DEP[RESULT_ROW - 1 + i_list, RESULT_COL + 6] = 100.0 * x_actual / x_rows;
                        excelSheet_DEP[RESULT_ROW - 1 + i_list, RESULT_COL + 7] = x_expect;

                        sum_actual += x_actual;
                        sum_expected += x_expect;
                        i_list += 1;
                        if (x_actual < x_expect)
                        {
                            neg_sum_actual += x_actual;
                            neg_sum_expected += x_expect;
                            neg_i_list += 1;
                        }
                        else
                        {
                            pos_sum_actual += x_actual;
                            pos_sum_expected += x_expect;
                            pos_i_list += 1;
                        }
                    }
                }
            }

            excelSheet_DEP[DEP_TABLE_ROW, DEP_TABLE_COL] = pos_i_list;
            excelSheet_DEP[DEP_TABLE_ROW, DEP_TABLE_COL + 2] = x_max;
            excelSheet_DEP[DEP_TABLE_ROW, DEP_TABLE_COL + 4] = pos_sum_actual;
            excelSheet_DEP[DEP_TABLE_ROW, DEP_TABLE_COL + 5] = pos_sum_expected;
            excelSheet_DEP[DEP_TABLE_ROW, DEP_TABLE_COL + 6] = pos_sum_actual - pos_sum_expected;
            excelSheet_DEP[DEP_TABLE_ROW, DEP_TABLE_COL + 8] = 100 * Convert.ToInt64(excelSheet_DEP[DEP_TABLE_ROW, DEP_TABLE_COL + 6]) / x_rows;

            excelSheet_DEP[DEP_TABLE_ROW + 6, DEP_TABLE_COL] = neg_i_list;
            excelSheet_DEP[DEP_TABLE_ROW + 6, DEP_TABLE_COL + 2] = x_max;
            excelSheet_DEP[DEP_TABLE_ROW + 6, DEP_TABLE_COL + 4] = neg_sum_actual;
            excelSheet_DEP[DEP_TABLE_ROW + 6, DEP_TABLE_COL + 5] = neg_sum_expected;
            excelSheet_DEP[DEP_TABLE_ROW + 6, DEP_TABLE_COL + 6] = neg_sum_actual - neg_sum_expected;
            excelSheet_DEP[DEP_TABLE_ROW + 6, DEP_TABLE_COL + 8] = 100 * Math.Abs(Convert.ToInt64(excelSheet_DEP[DEP_TABLE_ROW + 6, DEP_TABLE_COL + 6]) / x_rows);

            //Replace all Same-Same pairs in the table with counts needed to make the Actual=Expected.
            //Use a convergence algorithm to do this
            for (i = 0; i < x_max; i++)
            {
                Totals[i, 2] = 0;
            }

            x_rows_r = 0;
            for (i = 0; i < x_max; i++)
            {
                x_total_r = 0;
                for (j = 0; j < x_max; j++)
                {
                    if (i != j)
                    {
                        x_total_r += Counts[i, j];
                    }
                    else
                    {
                        x_total_r += Totals[i, 2];
                    }
                }

                Totals[i, 1] = x_total_r;
                x_rows_r = x_rows_r + x_total_r;
            }

            x_rows_r = x_rows_r + LAG;
            Check = x_rows_r;
            for (k = 1; k <= 100; k++)
            {
                for (i = 0; i < x_max; i++)
                {
                    x_total_i = Totals[i, 1];
                    x_actual = (x_rows_r - LAG) * x_total_i * x_total_i / x_rows_r / x_rows_r;

                    Totals[i, 2] = x_actual;
                }
                x_rows_r = 0;
                for (i = 0; i < x_max; i++)
                {
                    x_total_r = 0;
                    for (j = 0; j < x_max; j++)
                    {
                        if (i != j)
                        {
                            x_total_r += Counts[i, j];
                        }
                        else
                        {
                            x_total_r += Totals[i, 2];
                        }
                    }
                    Totals[i, 1] = x_total_r;
                    x_rows_r += x_total_r;
                }
                x_rows_r = x_rows_r + LAG;
                if ((Math.Abs(Check - x_rows_r) <= 0.0001))
                {
                    k = 100;
                }
                else
                {
                    Check = x_rows_r;
                }
            }

            // Now look for Not Same Dependencies
            min_take = x_rows_r;
            for (i = 0; i < x_max; i++)
            {
                for (j = 0; j < x_max; j++)
                {

                    if (i != j)
                    {
                        x_total_i_r = Totals[i, 1];

                        x_total_j_r = Totals[j, 1];
                        x_expect = (x_rows_r - LAG) * x_total_i_r * x_total_j_r / x_rows_r / x_rows_r;

                        x_actual = Counts[i, j];
                        if (((x_expect >= 5) | (x_actual >= 5)) & (x_total_i_r >= MINIM) & (x_total_j_r >= MINIM))
                        {
                            pois = 0;
                            lambda = x_expect;
                            r = (long)x_actual;
                            if (lambda < 1)
                            {
                                pois1 = 0;
                                for (m = 0; m <= r; m++)
                                {
                                    y = 1;
                                    yExtend = 0;
                                    if (m > 0)
                                    {
                                        for (k = 1; k <= m; k++)
                                        {
                                            LogY = Math.Log10(y);
                                            if ((Math.Abs(LogY) > 300))
                                            {
                                                yExtend = yExtend + Convert.ToInt64(LogY);
                                                y = Math.Pow((10), (LogY - Convert.ToInt64(LogY)));
                                            }
                                            y = y * lambda / k;
                                        }
                                    }
                                    if ((yExtend == 0))
                                    {
                                        pois1 = pois1 + y * Math.Exp(-lambda);
                                    }
                                    else
                                    {
                                        LogY = Math.Log(y);
                                        pois1 = pois1 + Math.Exp(LogY - lambda + yExtend * eLog);
                                    }
                                }
                                if (pois1 > 1)
                                {
                                    pois1 = 1;
                                }
                                n_chance += (1 - pois1);
                                lambda = 1;
                            }
                            else
                            {
                                n_chance += alpha;
                            }
                            for (m = 0; m <= r; m++)
                            {
                                y = 1;
                                yExtend = 0;
                                if (m > 0)
                                {
                                    for (k = 1; k <= m; k++)
                                    {
                                        LogY = Math.Log10(y);
                                        if ((Math.Abs(LogY) > 300))
                                        {
                                            yExtend += Convert.ToInt64(LogY);
                                            y = Math.Pow((10), (LogY - Convert.ToInt64(LogY)));
                                        }
                                        y = y * lambda / k;
                                    }
                                }
                                if ((yExtend == 0))
                                {
                                    pois += y * Math.Exp(-lambda);
                                }
                                else
                                {
                                    LogY = Math.Log(y);
                                    pois += Math.Exp(LogY - lambda + yExtend * eLog);
                                }
                            }
                            if (pois > 1)
                            {
                                pois = 1;
                            }
                            rank = Math.Abs(pois - 0.5) + 0.5;
                            if (rank >= 1 - alpha)
                            {
                                // If Sheets("Raw_Data").Cells(i + 1, MAP_COL + 1).Value = Sheets("Raw_Data").Cells(j + 1, MAP_COL + 1).Value Then
                                if (Texts[i] == Texts[j])
                                {
                                    excelSheet_DEP[RESULT_ROW - 1 + i_list, RESULT_COL] = "Same-Same";
                                }
                                else
                                {
                                    excelSheet_DEP[RESULT_ROW - 1 + i_list, RESULT_COL] = "Not-Same";
                                }

                                excelSheet_DEP[RESULT_ROW - 1 + i_list, RESULT_COL + 1] = Texts[i];

                                excelSheet_DEP[RESULT_ROW - 1 + i_list, RESULT_COL + 2] = Texts[j];
                                excelSheet_DEP[RESULT_ROW - 1 + i_list, RESULT_COL + 3] = x_actual - x_expect;
                                excelSheet_DEP[RESULT_ROW - 1 + i_list, RESULT_COL + 4] = 100.0 * (x_actual - x_expect) / x_rows;
                                excelSheet_DEP[RESULT_ROW - 1 + i_list, RESULT_COL + 5] = x_actual;
                                excelSheet_DEP[RESULT_ROW - 1 + i_list, RESULT_COL + 6] = 100.0 * x_actual / x_rows;
                                excelSheet_DEP[RESULT_ROW - 1 + i_list, RESULT_COL + 7] = x_expect;

                                sum_actual += x_actual;
                                sum_expected += x_expect;
                                i_list += 1;
                                if (x_actual < x_expect)
                                {
                                    neg_sum_actual += x_actual;
                                    neg_sum_expected += x_expect;
                                    neg_i_list += 1;
                                }
                                else
                                {
                                    pos_sum_actual += +x_actual;
                                    pos_sum_expected += x_expect;
                                    pos_i_list += 1;
                                }
                                if (x_total_i_r < min_take)
                                {
                                    min_take = x_total_i_r;
                                }
                                if (x_total_j_r < min_take)
                                {
                                    min_take = x_total_j_r;
                                }
                            }
                        }
                    }
                }
            }

            excelSheet_DEP[27, 7] = n_chance;

            excelSheet_DEP[DEP_TABLE_ROW + 2, DEP_TABLE_COL] = pos_i_list;
            excelSheet_DEP[DEP_TABLE_ROW + 2, DEP_TABLE_COL + 2] = x_max * x_max;
            excelSheet_DEP[DEP_TABLE_ROW + 2, DEP_TABLE_COL + 4] = pos_sum_actual;
            excelSheet_DEP[DEP_TABLE_ROW + 2, DEP_TABLE_COL + 5] = pos_sum_expected;
            excelSheet_DEP[DEP_TABLE_ROW + 2, DEP_TABLE_COL + 6] = pos_sum_actual - pos_sum_expected;
            excelSheet_DEP[DEP_TABLE_ROW + 2, DEP_TABLE_COL + 8] = 100 * Convert.ToInt64(excelSheet_DEP[DEP_TABLE_ROW + 2, DEP_TABLE_COL + 6]) / x_rows;
            excelSheet_DEP[DEP_TABLE_ROW + 1, DEP_TABLE_COL] = pos_i_list - Convert.ToInt64(excelSheet_DEP[DEP_TABLE_ROW, DEP_TABLE_COL]);
            excelSheet_DEP[DEP_TABLE_ROW + 1, DEP_TABLE_COL + 2] = x_max * x_max - x_max;
            excelSheet_DEP[DEP_TABLE_ROW + 1, DEP_TABLE_COL + 4] = pos_sum_actual - Convert.ToInt64(excelSheet_DEP[DEP_TABLE_ROW, DEP_TABLE_COL + 4]);
            excelSheet_DEP[DEP_TABLE_ROW + 1, DEP_TABLE_COL + 5] = pos_sum_expected - Convert.ToInt64(excelSheet_DEP[DEP_TABLE_ROW, DEP_TABLE_COL + 5]);
            excelSheet_DEP[DEP_TABLE_ROW + 1, DEP_TABLE_COL + 6] = pos_sum_actual - pos_sum_expected - Convert.ToInt64(excelSheet_DEP[DEP_TABLE_ROW, DEP_TABLE_COL + 6]);
            excelSheet_DEP[DEP_TABLE_ROW + 1, DEP_TABLE_COL + 8] = 100 * Convert.ToInt64(excelSheet_DEP[DEP_TABLE_ROW + 1, DEP_TABLE_COL + 6]) / x_rows;

            excelSheet_DEP[DEP_TABLE_ROW + 8, DEP_TABLE_COL] = neg_i_list;
            excelSheet_DEP[DEP_TABLE_ROW + 8, DEP_TABLE_COL + 2] = x_max * x_max;
            excelSheet_DEP[DEP_TABLE_ROW + 8, DEP_TABLE_COL + 4] = neg_sum_actual;
            excelSheet_DEP[DEP_TABLE_ROW + 8, DEP_TABLE_COL + 5] = neg_sum_expected;
            excelSheet_DEP[DEP_TABLE_ROW + 8, DEP_TABLE_COL + 6] = neg_sum_actual - neg_sum_expected;
            excelSheet_DEP[DEP_TABLE_ROW + 8, DEP_TABLE_COL + 8] = 100 * Math.Abs(Convert.ToInt64(excelSheet_DEP[DEP_TABLE_ROW + 8, DEP_TABLE_COL + 6])) / x_rows;
            excelSheet_DEP[DEP_TABLE_ROW + 7, DEP_TABLE_COL] = neg_i_list - Convert.ToInt64(excelSheet_DEP[DEP_TABLE_ROW + 6, DEP_TABLE_COL]);
            excelSheet_DEP[DEP_TABLE_ROW + 7, DEP_TABLE_COL + 2] = x_max * x_max - x_max;
            excelSheet_DEP[DEP_TABLE_ROW + 7, DEP_TABLE_COL + 4] = neg_sum_actual - Convert.ToInt64(excelSheet_DEP[DEP_TABLE_ROW + 6, DEP_TABLE_COL + 4]);
            excelSheet_DEP[DEP_TABLE_ROW + 7, DEP_TABLE_COL + 5] = neg_sum_expected - Convert.ToInt64(excelSheet_DEP[DEP_TABLE_ROW + 6, DEP_TABLE_COL + 5]);
            excelSheet_DEP[DEP_TABLE_ROW + 7, DEP_TABLE_COL + 6] = neg_sum_actual - neg_sum_expected - Convert.ToInt64(excelSheet_DEP[DEP_TABLE_ROW + 6, DEP_TABLE_COL + 6]);
            excelSheet_DEP[DEP_TABLE_ROW + 7, DEP_TABLE_COL + 8] = 100 * Math.Abs(Convert.ToInt64(excelSheet_DEP[DEP_TABLE_ROW + 7, DEP_TABLE_COL + 6])) / x_rows;

            //LETS TRY TO GET SOME OUTPUT
            i = 66;

            string testString = (string)excelSheet_DEP[i, 5];

            while (testString == "Same-Same" || testString == "Not-Same")
            {
                returnArray.Add(new DependencyEvent((string)excelSheet_DEP[i, 5], (string)excelSheet_DEP[i, 6], (string)excelSheet_DEP[i, 7], (double)excelSheet_DEP[i, 8], (double)excelSheet_DEP[i, 9], (double)excelSheet_DEP[i, 10], (double)excelSheet_DEP[i, 11], (double)excelSheet_DEP[i, 12], (Convert.ToInt64(excelSheet_DEP[i, 11]) - Convert.ToInt64(excelSheet_DEP[i, 9]))));
                i += 1;
                testString = (string)excelSheet_DEP[i, 5];
            }
            return returnArray;
        }

        private static float NORMSDIST(float x)
        {
            float result = 0;
            float y = 1 / (1 + ((float)0.2316419 * Math.Abs(x)));
            float z = (float)0.3989423 * ((float)Math.Exp((-Math.Pow(x, 2)) / (float)2));

            result = 1 - z * (((float)1.33027 * ((float)Math.Pow(y, 5))) - ((float)1.821256 * ((float)Math.Pow(y, 4))) + ((float)1.781478 * ((float)Math.Pow(y, 3))) - ((float)0.356538 * ((float)Math.Pow(y, 2))) + ((float)0.3193815 * y));

            if (x > 0)
            {
                return result;
            }
            else
            {
                return 1 - result;
            }
        }


    }





}

