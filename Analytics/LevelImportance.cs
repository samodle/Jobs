using System;
using System.Collections.Generic;
using System.Text;
using static Analytics.Constants;

namespace Analytics
{
    public class AttributeLevel
    {
        public double Value { get; set; }
        public double N { get; set; }
        public double StandardError { get; set; }
        public double LowerCIBound { get; set; }
        public double UpperCIBound { get; set; }
        public bool RecommendSuppress { get; set; }
        public DateTime Date { get; set; }
        public string Source { get; set; }

        public AttributeLevel(double value, double n, double stdError, double lowerCI, double upperCI, string suppress, DateTime date, string source)
        {
            this.Value = value;
            this.N = n;
            this.StandardError = stdError;
            this.LowerCIBound = lowerCI;
            this.UpperCIBound = upperCI;
            this.RecommendSuppress = suppress == "Y" ? true : false;
            this.Date = date;
            this.Source = source;
        }

        public double calculateSimilarity(AttributeLevel other)
        {
            if(RecommendSuppress || other.RecommendSuppress) { return INVALID_DISTANCE; }
            else
            {
                double levelDifference = Math.Abs(other.Value - Value);
                if (levelDifference < LEVEL_MATCH_THRESHOLD)
                { return 0; }
                else
                {
                    return levelDifference;
                }
            }
        }

       public override string ToString()
        {
            return "Value: " + Value + ", Standard Error:" + StandardError + ", Lower CI Bound:" + LowerCIBound + ", Upper CI Bound:" + UpperCIBound;
        } 
    }

    public class AttributeImportance : AttributeLevel
    {
        public bool NotRelevant { get; set; }

        public AttributeImportance(double value, double n, double stdError, double lowerCI, double upperCI, string suppress, DateTime date, string source, string notRelevant) : base(value, n, stdError, lowerCI, upperCI, suppress, date, source)
        {
            this.NotRelevant = notRelevant == "N" ? false : true;
        }

        public double calculateSimilarity(AttributeImportance other)
        {
            if (NotRelevant || RecommendSuppress || other.RecommendSuppress || other.RecommendSuppress) { return INVALID_DISTANCE; }
            else
            {
                double importanceDifference = Math.Abs(other.Value - Value);
                if (importanceDifference < IMPORTANCE_MATCH_THRESHOLD)
                { return 0; }
                else
                {
                    return importanceDifference;
                }
            }
        }
    }
}
