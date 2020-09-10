using System;
using System.Collections.Generic;
using System.Text;

namespace Analytics
{
    public class CPM_Node
    {
        public CPM_Node(int ID, string Name, string Summary, double Growth, List<string> Strengths, List<int> NextSteps)
        {
            this.ID = ID;
            this.Name = Name;
            this.Summary = Summary;
            this.Growth = Growth;
            this.Strengths = Strengths;
            this.NextSteps = NextSteps;
        }

        public int ID { get; set; }
        public string Name { get; set; }
        public string Summary { get; set; }
        public List<Tuple<string, string>> Actions { get; set; } = new List<Tuple<string, string>>();
        public double Growth { get; set; }
        public List<string> Strengths { get; set; } = new List<string>();

        public double Salary_AR { get; set; }
        public double Salary_TN { get; set; }
        public double Salary_R { get; set; }
        public double Salary_X { get; set; }

        public List<int> NextSteps { get; set; } = new List<int>();


        public override string ToString()
        {
            return this.Name + " " + this.ID + " " + this.Growth;
        }
    }
}
