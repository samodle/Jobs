using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Analytics
{
    public class Occupation
    {
        public string Name { get; set; }
        public string SOCCode { get; set; }
        public string Description { get; set; }
        public List<string> AlternateNames { get; set; } 
        public List<Skill> Skills { get; set; }
        public List<Knowledge> Knowledge { get; set; }
        public List<Ability> Abilities { get; set; }

        public Occupation(string name, string socCode, string descriptions)
        {
            this.Name = name;
            this.SOCCode = SOCCode;
            this.Description = descriptions;
        }
    }
}
