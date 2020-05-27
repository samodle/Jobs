using System;
using System.Collections.Generic;
using System.Text;

namespace Analytics
{
    public class AttributeLevel
    {
        public string Name { get; set; }
        public double N { get; set; }
        public double LowerCIBound { get; set; }
        public double UpperCIBound { get; set; }
        public bool RecommentSuppress { get; set; }
        public DateTime Date { get; set; }
        public string Source { get; set; }

        public AttributeLevel(string name, double n, double lowerCI, double upperCI, bool suppress, DateTime date, string source)
        {
            this.Name = name;
            this.N = n;
            this.LowerCIBound = lowerCI;
            this.UpperCIBound = upperCI;
            this.RecommentSuppress = suppress;
            this.Date = date;
            this.Source = source;
        }
    }

    public class AttributeImportance : AttributeLevel
    {
        public bool NotRelevant { get; set; }

        public AttributeImportance(string name, double n, double lowerCI, double upperCI, bool suppress, DateTime date, string source, bool notRelevant) : base(name, n, lowerCI, upperCI, suppress, date, source)
        {
            this.NotRelevant = notRelevant;
        }
    }
}
