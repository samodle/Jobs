using DataInterface;
using System;
using System.Collections.Generic;
using System.Text;

namespace Analytics
{
    public class PitStopStartupAnalysis
    {
        private const double MaxScore = 100;
        public double KPI_A_raw { get; set; } //OEE
        public double KPI_B_raw { get; set; } //Stops
        public double KPI_C_raw { get; set; } //MTBF
        public double KPI_D_raw { get; set; } //first stop

        public double KPI_A { get; set; } //OEE
        public double KPI_B { get; set; } //Stops
        public double KPI_C { get; set; } //MTBF
        public double KPI_D { get; set; } //first stop

        public const double KPI_A_Weight = 0.4;
        public const double KPI_B_Weight = 0.4;
        public const double KPI_C_Weight = 0.1;
        public const double KPI_D_Weight = 0.1;

        public double getScoreForPeriod(SystemSummaryReport testReport, double testPeriodMins)
        {
            KPI_A_raw = testReport.OEE;
            KPI_B_raw = testReport.Stops;
            KPI_C_raw = double.IsNaN(testReport.MTBF) || double.IsInfinity(testReport.MTBF) ? 0 : testReport.MTBF;// Math.Min(testReport.MTBF, testPeriodHRs * 60);
            if (testReport.DT_Report.rawDTdata.rawConstraintData.Count < 2) { KPI_D_raw = 0; } else { KPI_D_raw = testReport.DT_Report.rawDTdata.rawConstraintData[1].UT; }
            weightRawScores(testPeriodMins);
            if (double.IsInfinity(getWeightedScore()) || double.IsNaN(getWeightedScore())) { System.Diagnostics.Debugger.Break(); }
            return getWeightedScore();
        }

        private void weightRawScores(double testPeriodMin)
        {
            KPI_A = KPI_A_raw * MaxScore; //OEE
            KPI_B = (Math.Min((KPI_B_raw / testPeriodMin), 1)) * MaxScore; //stops
            KPI_C = (KPI_C_raw / testPeriodMin) * MaxScore; //MTBF
            KPI_D = (KPI_D_raw / testPeriodMin) * MaxScore; //first stop
        }

        private double getWeightedScore()
        {
            return (KPI_A * KPI_A_Weight) + (KPI_B * KPI_B_Weight) + (KPI_C * KPI_C_Weight) + (KPI_D * KPI_D_Weight);
        }

    }
}
