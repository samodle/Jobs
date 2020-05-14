using System.Collections.Generic;
using static ForkAnalyticsSettings.GlobalConstants;
namespace Windows_Desktop
{
    public static class Window_Dashboard_Settings
    {
        #region Loss Compass
        public const int TierA_MAXBARS = 6;
        public const int TierB_MAXBARS = 6;
        public const int TierC_MAXBARS = 8;
        public enum CardTier { A, B, C, NA }

        public enum KPIs { One, Two, Three, Four, Five, NA }
        public enum TopLevelSelected { Unplanned, Planned, RateLoss, OEE }
        #endregion
        #region xSigma
        public const double ChronicSporadic_Threshold_High = 6.5;
        public const double ChronicSporadic_Threshold_Low = 3.5;
        #endregion
        #region Trends

        #endregion
        #region Pit Stop
        public const double PITSTOP_SU_ANALYSISPERIOD_MIN = 60;
        #endregion
        #region Loss Network
        public const double LOSSNETWORK_MINBUBBLESCORE = 0;
        #endregion

        #region LiveLine 
        public enum EventType { Running, Excluded, Planned, Unplanned }
        #endregion


        #region Downtime Field Hierarchy
        public static DowntimeField getNextDowntimeField(DowntimeField Field)
        {
            switch (Field)
            {
                case DowntimeField.Tier1:
                    return DowntimeField.Tier2;

                case DowntimeField.Tier2:
                    return DowntimeField.Tier3;

                case DowntimeField.Tier3:
                    return DowntimeField.Fault;

                case DowntimeField.Reason1:
                    return DowntimeField.Reason2;

                case DowntimeField.Reason2:
                    return DowntimeField.Reason3;

                case DowntimeField.Reason3:
                    return DowntimeField.Reason4;

                case DowntimeField.Reason4:
                    return DowntimeField.Fault;

                case DowntimeField.Location:
                    return DowntimeField.Reason3;

                case DowntimeField.DTGroup:
                    return DowntimeField.Tier2;

                case DowntimeField.Fault:
                    return DowntimeField.ProductCode;

                case DowntimeField.ProductCode:
                    return DowntimeField.Team;

                default:
                    return DowntimeField.NA;

            }
        }


        #endregion

        #region Enum -> String / String -> Enum
        public static DowntimeField getEnumForString(string FieldName)
        {
            DowntimeField tmpField = DowntimeField.NA;
            for (int i = 0; i <= (int)DowntimeField.NA; i++) { if (getStringForEnum((DowntimeField)i) == FieldName) { tmpField = (DowntimeField)i; } }
            return tmpField;
        }
        public static List<string> getStringListForEnumList(List<DowntimeField> enumList)
        {
            var tmpList = new List<string>();
            for (int i = 0; i < enumList.Count; i++)
            {
                tmpList.Add(getStringForEnum(enumList[i]));
            }
            return tmpList;
        }
        public static string getStringForEnum(DowntimeField Field)
        {
            switch (Field)
            {
                case DowntimeField.Tier1:
                    return "Tier 1";

                case DowntimeField.Tier2:
                    return "Tier 2";

                case DowntimeField.Tier3:
                    return "Tier 3";

                case DowntimeField.Reason1:
                    return "Reason 1";

                case DowntimeField.Reason2:
                    return "Reason 2";

                case DowntimeField.Reason3:
                    return "Reason 3";

                case DowntimeField.Reason4:
                    return "Reason 4";

                case DowntimeField.Location:
                    return "Location";

                case DowntimeField.DTGroup:
                    return "DT Group";

                case DowntimeField.Fault:
                    return "Fault";

                case DowntimeField.ProductCode:
                    return "SKU";

                case DowntimeField.ProductGroup:
                    return "Product Size";

                case DowntimeField.Team:
                    return "Team";

                default:
                    return "";

            }
        }

        public static string getStringForEnum_Metric(DowntimeMetrics Metric)
        {
            switch (Metric)
            {
                case DowntimeMetrics.DTpct: return "DTpct"; //mode
                case DowntimeMetrics.DT: return "DT";
                case DowntimeMetrics.SPD: return "SPD"; //line & mode
                case DowntimeMetrics.MTBF: return "MTBF";
                case DowntimeMetrics.Stops: return "Stops";
                case DowntimeMetrics.MTTR: return "MTTR";
                case DowntimeMetrics.OEE: return "Jobs"; //line
                case DowntimeMetrics.PDTpct: return "PDT %";
                case DowntimeMetrics.UPDTpct: return "UPDT %";
                case DowntimeMetrics.SKUs: return "SKUs";
                case DowntimeMetrics.UnitsProduced: return "Units";
                case DowntimeMetrics.NumChangeovers: return "# C/Os";
                case DowntimeMetrics.SchedTime: return "Sched Time";
                default: return "";
            }
        }

        #endregion



    }
}
