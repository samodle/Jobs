using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using static Analytics.Constants;

namespace Analytics
{
    public class Attribute
    {
        public string Name { get; set; }
        public string ElementID { get; set; }
        public List<string> OccupationIDs { get; set; } = new List<string>();
        public AttributeType Type { get; set; }
        //public bool isHot { get; set; }

        public Attribute(string name, string id, string occupationID, AttributeType type)
        {
            this.Name = name;
            this.ElementID = id;
            this.OccupationIDs.Add(occupationID);
            this.Type = type;
        }

        public override string ToString()
        {
            return getStringForAttributeType(this.Type) + ": " + Name + ", Occupations:" + OccupationIDs.Count;
        }
    }

    public class JobAttribute: IEquatable<JobAttribute>
    {
        public string Name { get; set; }
        public string ElementID { get; set; }
        public AttributeLevel Importance { get; set; }
        public AttributeImportance Level { get; set; }
        public AttributeType Type { get; set; }

        public JobAttribute(string name, string id, AttributeLevel importance, AttributeImportance level, AttributeType type) 
        {
            this.Name = name;
            this.ElementID = id;
            this.Importance = importance;
            this.Level = level;
            this.Type = type;

        }

        public double getDistance()
        {
            if(Importance.NotRelevant || Level.RecommendSuppress)
            {
                return INVALID_DISTANCE;
            }
            else
            {
                return Level.Value * LEVEL_OVER_IMPORTANCE_FACTOR + Importance.Value;
            }
        }
        public double calculateSimilarity(JobAttribute other)
        {
            if (!this.Equals(other)) { return INVALID_DISTANCE; }
            else 
            {
                double iDist = Importance.calculateSimilarity(other.Importance);
                double lDist = Level.calculateSimilarity(other.Level);

                if (iDist == INVALID_DISTANCE || lDist == INVALID_DISTANCE)
                {
                    return INVALID_DISTANCE;
                }
                else
                {
                    return LEVEL_OVER_IMPORTANCE_FACTOR * lDist + iDist;
                }
            }
        }

        public override string ToString()
        {
            return getStringForAttributeType(this.Type) + ": " + Name + ", Importance: " + Importance.Value + ", Level: " + Level.Value;
        }

        public bool Equals(JobAttribute other)
          {
            return Name.Equals(other.Name) && Type == other.Type;
          }

    }
}
