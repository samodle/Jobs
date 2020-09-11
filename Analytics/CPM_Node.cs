using DataPersistancy;
using System;
using System.Collections.Generic;
using System.Text;
using static Analytics.Constants;

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

        public string Salary { get; set; }

        public void JuiceSalary(ActiveLocations l)
        {
            double pay = getSalary(l);

            double lowRange = pay - GetRandomNumber(0, 2);
            double highRange = pay + GetRandomNumber(0, 7);

            Salary = "$" + Math.Round(lowRange, 1) + " - " + Math.Round(highRange, 1) + "k";
        }

        private static double GetRandomNumber(double minimum, double maximum)
        {
            Random random = new Random();
            return random.NextDouble() * (maximum - minimum) + minimum;
        }

        public bool isRemote()
        {
            return Salary_R > 0;
        }
        public bool isRelocate()
        {
            return Salary_X > 0;
        }

        public double getSalary(ActiveLocations l) {

            switch (l){
                case ActiveLocations.AR:
                    if(Salary_AR < 0) 
                    { 
                        if(Salary_R > 0) { return Salary_R; } 
                        if(Salary_X > 0) { return Salary_X; }
                    }
                    return Salary_AR;
                case ActiveLocations.TN:
                    if (Salary_TN < 0)
                    {
                        if (Salary_R > 0) { return Salary_R; }
                        if (Salary_X > 0) { return Salary_X; }
                    }
                    return Salary_TN;
                default:
                    return -999;
            }
        }

        public CPM_Node NextNode(int offset, ActiveLocations loc)
        {
            if(NextSteps.Count == 1) { return DemoIO.getNode(NextSteps[0]); }
            else if(NextSteps.Count == 2) { return DemoIO.getNode(NextSteps[offset]); }
            else
            {
                bool toggle = offset==0? true : false;
                for(int i = 0; i < NextSteps.Count; i++)
                {
                    if (DemoIO.getNode(NextSteps[i]).getSalary(loc) > 0)
                    {
                        if (toggle)
                        {
                            return (DemoIO.getNode(NextSteps[i]));
                        }
                        else
                        {
                            toggle = true;
                        }
                    }
                }
            }

            //this shouldn't happen
            return DemoIO.getNode(NextSteps[0]);
        }

        public List<int> NextSteps { get; set; } = new List<int>();

        public System.Collections.ObjectModel.ObservableCollection<JD> getJobs(ActiveLocations l)
        {
            var tmpList = new System.Collections.ObjectModel.ObservableCollection<JD>();

            foreach(JD j in DemoIO.jobs)
            {
                if (j.search_term.Contains(Name.Replace("/", " ")))
                {
                    if(isRelocate() || isRemote()) // we want everything
                    {
                        tmpList.Add(j);
                    }
                    else if(j.isAR && l == ActiveLocations.AR && Salary_AR > 0)
                    {
                        tmpList.Add(j);
                    }
                    else if (j.isTN && l == ActiveLocations.TN && Salary_TN > 0)
                    {
                        tmpList.Add(j);
                    }
                }
                else if (ID == 0)
                {
                    if (j.search_term.Contains("Packer"))
                    {
                        if (j.isAR && l == ActiveLocations.AR && Salary_AR > 0)
                        {
                            tmpList.Add(j);
                        }
                        else if (j.isTN && l == ActiveLocations.TN && Salary_TN > 0)
                        {
                            tmpList.Add(j);
                        }
                    }
                }
                else if (ID == 1)
                {
                    if (j.search_term.Contains("Factory"))
                    {
                        if (j.isAR && l == ActiveLocations.AR && Salary_AR > 0)
                        {
                            tmpList.Add(j);
                        }
                        else if (j.isTN && l == ActiveLocations.TN && Salary_TN > 0)
                        {
                            tmpList.Add(j);
                        }
                    }
                }
            }


            return tmpList;
        }


        public override string ToString()
        {
            return this.Name + " " + this.ID + " " + this.Growth;
        }
    }
}
