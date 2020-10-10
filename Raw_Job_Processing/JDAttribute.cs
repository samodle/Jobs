using System;
using System.Collections.Generic;
//using System.Collections.IEnumerable;
using System.Text;

namespace Raw_Job_Processing
{

    public enum AttributeType
    {
        Certification = 0,
        Keyword = 1,
        ProgrammingLanguage = 2
    }

    public class JDAttribute
    {
        public string Name { get; set; }
        public AttributeType Type {get;set;}

        public List<string> SearchTerms { get; set; } = new List<string>();

        public JDAttribute(string name, AttributeType type, List<string> searchTerms)
        {
            Name = name;
            Type = type;
            SearchTerms = searchTerms;
        }

        public JDAttribute(string name, AttributeType type)
        {
            Name = name;
            Type = type;
            SearchTerms.Add(name);
        }
    }


}
