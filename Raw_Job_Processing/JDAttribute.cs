using System;
using System.Collections.Generic;
//using System.Collections.IEnumerable;
using System.Text;

namespace Raw_Job_Processing
{

    public enum JDAttributeType
    {
        Certification = 0,
        Keyword = 1,
        ProgrammingLanguage = 2
    }

    public class JDAttribute
    {
        public string Name { get; set; }
        public JDAttributeType Type {get;set;}

        public List<string> SearchTerms { get; set; } = new List<string>();

        public JDAttribute(string name, JDAttributeType type, List<string> searchTerms)
        {
            Name = name;
            Type = type;
            SearchTerms = searchTerms;
        }

        public JDAttribute(string name, JDAttributeType type)
        {
            Name = name;
            Type = type;
            SearchTerms.Add(name);
        }
    }


}
