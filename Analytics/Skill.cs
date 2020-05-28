using System;
using System.Collections.Generic;
using System.Text;

namespace Analytics
{
    public class Skill //: IEquatable<Skill>
    {
        public string Name { get; set; }
        public string ElementID { get; set; }
        public List<string> OccupationIDs { get; set; } = new List<string>();

        public Skill(string name, string id, string occupationID)
        {
            this.Name = name;
            this.ElementID = id;
            OccupationIDs.Add(occupationID);
        }

        public override string ToString()
        {
            return "Skill: " + Name + ", Associated Occupations:" + OccupationIDs.Count;
        }
        //public List<WorkActivity>


        /*  public bool Equals(Skill other)
          {
              if (other.ElementID.Equals(ElementID))
              {
                  return true;
              }
              else
              {
                  return false;
              }
          }*/
    }

    public class JobSkill
    {
        public string Name { get; set; }
        public string ElementID { get; set; }
        public AttributeImportance Importance { get; set; }
        public AttributeLevel Level { get; set; }

        public JobSkill(string name, string id, AttributeImportance importance, AttributeLevel level)
        {
            this.Name = name;
            this.ElementID = id;
            this.Importance = importance;
            this.Level = level;
        }

        public override string ToString()
        {
            return "Skill: " + Name + ", Importance: " + Importance.Value + ", Level: " + Level.Value;
        }
    }
}
