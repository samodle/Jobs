namespace ForkAnalyticsSettings
{
    public static class GlobalConstants
    {
        public static class Mappings
        {
            public enum Cleaning
            {
                Standard
            }
            public enum Format
            {
                NoMappingAvailables
            }

            public enum Shape
            {
                NoMappingAvailable
            }

            public enum fork
            {
                NoMappingAvailable,
                GENERIC
            }

            public enum DTsched
            {
                NoMappingAvailable,
                GENERIC
            }

            public enum PRODsched
            {
                NoMappingAvailable
            }
        }

        public enum ControlRulesets
        {
            WesternElectric, Nelson, Montgomery, Westgard, AIAG, IHI, NA
        }

        public enum DowntimeMetrics
        {
            DTpct, //mode
            DT,
            SPD, //line & mode
            MTBF,
            Stops,
            MTTR,
            OEE, //line
            PDTpct,
            UPDTpct,
            SKUs,
            UnitsProduced,
            NumChangeovers,
            SchedTime,
            Survivability,
            Chronicity,
            NA
        }


        public enum DowntimeField
        {
            //Event Duration
            startTime,
            endTime,
            DT,
            UT,
            //Event Description
            MasterProdUnit,
            Location,
            Fault,
            Reason1,
            Reason2,
            Reason3,
            Reason4,
            PR_inout,
            Team,
            PlannedUnplanned,
            DTGroup,
            Product,
            ProductCode,
            Comment,
            Tier1,
            Tier2,
            Tier3,
            Format,
            Shape,
            Classification,
            Stopclass,
            ProductGroup,

            ParentLine,

            NA
        }

    }
}
