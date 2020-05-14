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
        #region Rate-o-meter
        public RateTrainerAnalysis RateTrainer_RawAnalysis = null;

        //returns whats needed for visual initialization
        public Tuple<double, double> initializeRateTrainer()
        {
            double systemRate = 240;
            RateTrainer_RawAnalysis = new RateTrainerAnalysis(AnalysisPeriodData.OEE, AnalysisPeriodData.PDTpct, systemRate);
            return new Tuple<double, double>(AnalysisPeriodData.PDTpct * 100, systemRate);
        }

        #endregion



    }
}
