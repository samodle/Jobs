using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Analytics
{
    public class Occupation //: IEquatable<Occupation>
    {
        public string Name { get; set; }
        public string SOCCode { get; set; }
        public string Description { get; set; }
        public List<string> AlternateNames { get; set; }
        public List<JobSkill> Skills { get; set; } = new List<JobSkill>();
        public List<Knowledge> Knowledge { get; set; } = new List<Knowledge>();
        public List<Ability> Abilities { get; set; } = new List<Ability>();

        public Occupation(string name, string socCode, string descriptions)
        {
            this.Name = name;
            this.SOCCode = socCode;
            this.Description = descriptions;
        }

        public override string ToString()
        {
            return "Occupation: " + Name + ", Associated Skills:" + Skills.Count;
        }

        /*   public bool Equals(Occupation other)
           {
               if (other.SOCCode.Equals(SOCCode))
               {
                   return true;
               }
               else
               {
                   return false;
               }
           }*/
    }
}
