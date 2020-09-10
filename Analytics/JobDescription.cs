using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Analytics
{
    public class JD
    {
        public JD(string JobTitle, string url, string company, string location, string salary, string search_term, string description)
        {
            this.JobTitle = JobTitle;
            this.url = url;
            this.company = company;
            this.location = location;
            this.salary = salary;
            this.search_term = search_term;
            this.description = description;
        }

        public string JobTitle { get; set; }

        public string url { get; set; }

        public string company { get; set; }

        public string location { get; set; }

        public string salary { get; set; }

        public string search_term { get; set; }

        public string description { get; set; }



        public int node_id { get; set; }
        public bool isTN { get; set; } 
        public bool isAR { get; set; }


        public override string ToString()
        {
            return JobTitle + " // " + location;
        }
    }

}
