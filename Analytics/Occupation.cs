﻿using Microsoft.Office.Interop.Excel;
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
        public List<JobAttribute> Skills { get; set; } = new List<JobAttribute>();
        public List<JobAttribute> Knowledge { get; set; } = new List<JobAttribute>();
        public List<JobAttribute> Abilities { get; set; } = new List<JobAttribute>();
        public Constants.JobZone Zone { get; set; }

        public Occupation(string name, string socCode, string descriptions)
        {
            this.Name = name;
            this.SOCCode = socCode;
            this.Description = descriptions;
        }

        public override string ToString()
        {
            return Name + ", Skills:" + Skills.Count + ", Knowledge: " + Knowledge.Count + ", Abilities:" + Abilities.Count;
        }

        public List<JobAttribute> getAttributesByType(Constants.AttributeType type)
        {
            switch (type)
            {
                case Constants.AttributeType.Ability:
                    return Abilities;
                case Constants.AttributeType.Knowledge:
                    return Knowledge;
                case Constants.AttributeType.Skill:
                    return Skills;
                default:
                    return new List<JobAttribute>();
            }
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
