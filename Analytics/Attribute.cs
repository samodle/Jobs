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

    public class JobAttribute
    {
        public string Name { get; set; }
        public string ElementID { get; set; }
        public AttributeImportance Importance { get; set; }
        public AttributeLevel Level { get; set; }
        public AttributeType Type { get; set; }

        public JobAttribute(string name, string id, AttributeImportance importance, AttributeLevel level, AttributeType type)
        {
            this.Name = name;
            this.ElementID = id;
            this.Importance = importance;
            this.Level = level;
            this.Type = type;

        }

        public override string ToString()
        {
            return getStringForAttributeType(this.Type) + ": " + Name + ", Importance: " + Importance.Value + ", Level: " + Level.Value;
        }
    }
}
