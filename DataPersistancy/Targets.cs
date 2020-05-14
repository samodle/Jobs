using System;
using System.Collections.Generic;
using System.Text;
using static ForkAnalyticsSettings.GlobalConstants;

namespace DataPersistancy
{
    public class KPITarget : IComparable<KPITarget>, IEquatable<KPITarget>
    {

        #region Variables
        public string Name { get; set; }
        public double Target { get; set; }
        public DowntimeMetrics Metric { get; set; }
        public DowntimeField MappingA { get; set; }
        public DowntimeField MappingB { get; set; }
        #endregion

        #region Constructor
        public KPITarget(string Name, double Target, DowntimeMetrics Metric, DowntimeField MappingA, DowntimeField MappingB)
        {
            this.Name = Name;
            this.Target = Target;
            this.Metric = Metric;
            this.MappingA = MappingA;
            this.MappingB = MappingB;
        }
        public KPITarget(string Name, DowntimeMetrics Metric, DowntimeField MappingA, DowntimeField MappingB)
        {
            this.Name = Name;
            this.Metric = Metric;
            this.MappingA = MappingA;
            this.MappingB = MappingB;
            this.Target = -1;
        }

        #endregion

        #region Sortable / Equitable
        public int CompareTo(KPITarget Other)
        {
            if (this.Metric == Other.Metric)
            {
                return this.Metric.CompareTo(Other.Metric);
            }
            else
            {
                return this.Name.CompareTo(Other.Name);
            }


        }

        public bool Equals(KPITarget other)
        {
            if (this.Name == other.Name && this.Metric == other.Metric && this.MappingB == other.MappingB && this.MappingA == other.MappingA)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion


    }
}
