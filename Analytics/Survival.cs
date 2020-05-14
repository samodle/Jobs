using System;
using System.Collections.Generic;
using System.Linq;

namespace Analytics
{
    public class SurvivalAnalysis
    {
        private enum UTarrayCol { rawUT = 0, censoredUT = 1, Flag = 2 }

        private readonly List<Tuple<string, double>> EventData;
        private const double maxuptimecutoff = 400;
        private const double lowestUptimegroup = 0.0;
        private const double uptimeresolution = 0.5;

        public List<string> selectedfailuremodeList;
        public double[] netCDF;
        public double[,] survivaltable;
        public double[] uptimeGroupList;
        public double[] netUptimeCount;

        public SurvivalAnalysis(List<Tuple<string, double>> EventData)
        {
            this.EventData = EventData;
            CreateSurvivalTable();
        }

        private void CreateSurvivalTable()
        {
            double uptimegroup = 0.0;
            double maxuptime = 0;
            int uptimecount = 0;

            // determining max uptime in the raw downtime data
            long totalnoevents = 0;
            long totalnoevents_competing = 0;

            var failureModeListWithDupes = new List<string>();


            for (int i = 0; i < EventData.Count; i++)
            {
                failureModeListWithDupes.Add(EventData[i].Item1);

                if (EventData[i].Item2 < maxuptimecutoff) { maxuptime = Math.Max(maxuptime, EventData[i].Item2); }
                totalnoevents_competing += 1;
                totalnoevents += 1;
            }
            selectedfailuremodeList = failureModeListWithDupes.Distinct().ToList();
            var uptimearray = new double[3, totalnoevents];

            // definition of uptimearray
            // 0 --> raw uptime for all failure modes with PR in
            // 2 --> computed censored uptimes
            // 3 --> competing or cumulative flag

            //creating the uptimearray from raw proficy data
            for (int i = 0; i < EventData.Count; i++)
            {
                double tmpUT = EventData[i].Item2;
                uptimearray[(int)UTarrayCol.rawUT, i] = tmpUT; //0
                uptimearray[(int)UTarrayCol.censoredUT, i] = tmpUT; //2
                uptimearray[(int)UTarrayCol.Flag, i] = 1; //3
                // is competing
            }

            // calculating censored uptimes for each failure mode  ' need to add condition for competing cause
            string failuremode_analyzed;

            for (int m = uptimearray.GetLength(1) - 1; m > 0; m -= 1)
            {
                failuremode_analyzed = EventData[m].Item1;
                if (EventData[m - 1].Item1 != failuremode_analyzed)
                {
                    for (int n = m - 1; n >= 0; n -= 1)
                    {
                        if (EventData[n].Item1 == failuremode_analyzed)
                        {
                            break; // TODO: might not be correct. Was : Exit For
                        }
                        if (uptimearray[(int)UTarrayCol.Flag, n] == 1)
                        {
                            uptimearray[(int)UTarrayCol.censoredUT, m] += uptimearray[(int)UTarrayCol.rawUT, n];
                        }
                        // add uptimes only if a competing cause
                    }
                }
            }

            //creating the actual survival table for all failure modes and selected failure mode

            int numDataPoints = (int)(maxuptime / uptimeresolution) + 1;
            survivaltable = new double[selectedfailuremodeList.Count, numDataPoints];
            uptimeGroupList = new double[numDataPoints];
            netUptimeCount = new double[numDataPoints];
            netCDF = new double[numDataPoints];

            // uptimeGroupList --> uptime group list (0, 0.5, 1, 1.5, 2 ... max )
            // netUptimeCount --> uptime count for all failure modes
            // netCDF ->  CDF for all failure modes
            // survivaltable --> CDF for selected failure mode 1 ... n

            int j = 0;
            for (uptimegroup = lowestUptimegroup; uptimegroup <= maxuptime; uptimegroup += uptimeresolution)
            {
                uptimecount = 0;
                for (int i = 0; i < uptimearray.GetLength(1); i++)
                {
                    if (uptimearray[(int)UTarrayCol.rawUT, i] <= uptimegroup)
                    {
                        uptimecount += 1;
                    }

                    if (uptimearray[(int)UTarrayCol.censoredUT, i] <= uptimegroup)
                    {
                        int ListIndex = selectedfailuremodeList.FindIndex((string value) => { return value == EventData[i].Item1; });
                        survivaltable[ListIndex, j] += 1;
                    }
                }
                uptimeGroupList[j] = uptimegroup; //survivaltable[0, j] = uptimegroup;
                netUptimeCount[j] = uptimecount;// survivaltable[1, j] = uptimecount;
                netCDF[j] = Math.Round(1 - netUptimeCount[j] / totalnoevents, 4);//survivaltable[2, j] = Math.Round(1 - survivaltable[1, j] / totalnoevents, 4);

                j += 1;
            }

            // calculating CDF for selected failure modes with censoring
            for (int failureModeIncrementer = 0; failureModeIncrementer < selectedfailuremodeList.Count; failureModeIncrementer++)
            {
                for (int i = 0; i < uptimeGroupList.GetLength(0); i++)
                {
                    if (survivaltable[failureModeIncrementer, i] == survivaltable[failureModeIncrementer, j - 1])
                    {
                        survivaltable[failureModeIncrementer, i] = 0.0;
                    }
                    else
                    {
                        survivaltable[failureModeIncrementer, i] = Math.Round(1 - survivaltable[failureModeIncrementer, i] / survivaltable[failureModeIncrementer, j - 1], 4);
                    }
                }
            }


        }


    }
}
