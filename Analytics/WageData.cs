using System;
using System.Collections.Generic;
using System.Text;
using static Analytics.Constants;

namespace Analytics
{
    public class OccupationalWageSummary
    {
        public string Name { get; set; }
        public string SOCCode { get; set; }
        public List<RegionalWageSummary> Summary {get;set;} = new List<RegionalWageSummary>();

        public OccupationalWageSummary(string name, string soccode)
        {
            this.Name = name;
            this.SOCCode = soccode;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class RegionalWageSummary
    {
        public string Region { get; set; }
        public int TotalEmployed { get; set; } // Estimated total employment rounded to the nearest 10 (excludes self-employed).
        public double JobsPerOneK { get; set; } // Estimated total employment rounded to the nearest 10 (excludes self-employed).
        public double LocationQuotient { get; set; }  //The location quotient represents the ratio of an occupation’s share of employment in a given area to that occupation’s share of employment in the U.S. as a whole. For example, an occupation that makes up 10 percent of employment in a specific metropolitan area compared with 2 percent of U.S. employment would have a location quotient of 5 for the area in question. Only available for the state, metropolitan area, and nonmetropolitan area estimates; otherwise, this column is blank.

        public List<WageSnapshot> WageSnapshots { get; set; } = new List<WageSnapshot>();
    
    }

    public class WageSnapshot
    {
        public WageSnapshotType SnapshotType { get; set; }

        public double Mean { get; set; }
        public List<Tuple<int, double>> Percentiles { get; set; } = new List<Tuple<int, double>>();

    }

}
