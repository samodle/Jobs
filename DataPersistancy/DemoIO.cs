using Analytics;
using LumenWorks.Framework.IO.Csv;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace DataPersistancy
{
    public static class DemoIO
    {

        public static List<CPM_Node> nodes { get; set; } = new List<CPM_Node>();

        public static List<JD> jobs { get; set; } = new List<JD>();

        public static CPM_Node getNode(int id)
        {
            return nodes.First(n => n.ID == id);
        }


        public static void Demo_ImportGraph()
        {

            //nodes first
            var csvTable = new DataTable();
            using (var csvReader = new CsvReader(new StreamReader(System.IO.File.OpenRead(@"C:\Users\Public\Public_fork\demo_nodes2.csv")), true))
            {
                csvTable.Load(csvReader);
            }

            for (int i = 0; i < csvTable.Rows.Count; i++)
            {
                string tmp_growth = csvTable.Rows[i][13].ToString();
                double growth_val;
                if (tmp_growth == "+")
                {
                    growth_val = GetRandomNumber(0, 7);
                }
                else if (tmp_growth == "-")
                {
                    growth_val = GetRandomNumber(-7, 0);
                }
                else
                {
                    growth_val = 0;
                }

                List<string> tmp_strengths = new List<string>();
                tmp_strengths.Add(csvTable.Rows[i][14].ToString());
                tmp_strengths.Add(csvTable.Rows[i][15].ToString());

                int tmpID = Convert.ToInt32(csvTable.Rows[i][0].ToString());

                var tmpNode = new CPM_Node(ID: tmpID, Name: csvTable.Rows[i][1].ToString(), Summary: csvTable.Rows[i][6].ToString(), Growth: growth_val, Strengths: tmp_strengths, NextSteps: getNextStepsByID(tmpID));

                tmpNode.Salary_AR = csvTable.Rows[i][2].ToString() == "" ? -1 : Convert.ToDouble(csvTable.Rows[i][2].ToString());
                tmpNode.Salary_TN = csvTable.Rows[i][3].ToString() == "" ? -1 : Convert.ToDouble(csvTable.Rows[i][3].ToString());
                tmpNode.Salary_R = csvTable.Rows[i][4].ToString() == "" ? -1 : Convert.ToDouble(csvTable.Rows[i][4].ToString());
                tmpNode.Salary_X = csvTable.Rows[i][5].ToString() == "" ? -1 : Convert.ToDouble(csvTable.Rows[i][5].ToString());

                var tmpActivities = new List<Tuple<string, string>>();

                if (csvTable.Rows[i][7].ToString() != "")
                {
                    tmpActivities.Add(new Tuple<string, string>(csvTable.Rows[i][7].ToString(), csvTable.Rows[i][8].ToString()));
                    if (csvTable.Rows[i][9].ToString() != "")
                    {
                        tmpActivities.Add(new Tuple<string, string>(csvTable.Rows[i][9].ToString(), csvTable.Rows[i][10].ToString()));
                    }
                }

                tmpNode.Actions = tmpActivities;

                //internal/external status
                var inexstring = csvTable.Rows[i][16].ToString();
                if (inexstring.Equals("External"))
                {
                    tmpNode.InExStatus = NodeInternalExternal.External;
                }
                else if (inexstring.Equals("Internal"))
                {
                    tmpNode.InExStatus = NodeInternalExternal.Internal;
                }
                else if (inexstring.Equals("Both"))
                {
                    tmpNode.InExStatus = NodeInternalExternal.Both;
                }
                else
                {
                    tmpNode.InExStatus = NodeInternalExternal.Both;
                }

                //add to key list
                nodes.Add(tmpNode);
            }

        }


        private static Random random = new Random();
        public static double GetRandomNumber(double minimum, double maximum)
        {
            //a Random random = new Random();
            return random.NextDouble() * (maximum - minimum) + minimum;
        }

        private static List<int> getNextStepsByID(int ID)
        {
            switch (ID)
            {
                case 0:
                    return new List<int> { 1, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 14, 16, 17, 38 };
                case 1:
                    return new List<int> { 35, 23, 34, 37, 27, 26, 7, 6, 24, 22, 11 };
                case 2:
                    return new List<int> { 4, 1, 13, 6, 7, 14, 22, 11 };
                case 3:
                    return new List<int> { 1, 4, 6, 7, 5, 13, 14, 22, 11 };
                case 4:
                    return new List<int> { 23, 27, 26, 7, 6, 24, 22, 11 };
                case 5:
                    return new List<int> { 23, 27, 26, 7, 6, 24, 22, 11 };
                case 6:
                    return new List<int> { 30, 32, 33, 11 };
                case 7:
                    return new List<int> { 30, 32, 33, 11 };
                case 8:
                    return new List<int> { 6, 7, 22 };
                case 9:
                    return new List<int> { 8, 22, 6, 7 };
                case 10:
                    return new List<int> { 8, 22, 6, 7 };
                case 11:
                    return new List<int> { 27, 28, 6, 7 };
                case 12:
                    return new List<int> { 24, 25, 6, 7 };
                case 13:
                    return new List<int> { 27, 11, 6, 7 };
                case 14:
                    return new List<int> { 19 };
                case 15:
                    return new List<int> { 1, 4, 6, 7, 5, 13, 14, 22, 11 };
                case 16:
                    return new List<int> { 1, 4, 6, 7, 5, 13, 14, 22, 11 };
                case 23:
                    return new List<int> { 6, 34, 7, 35, 36, 37 };
                case 26:
                    return new List<int> { 29 };
                case 27:
                    return new List<int> { 28 };
                case 34:
                    return new List<int> { 23, 36, 37, 6, 7 };
                case 35:
                    return new List<int> { 34, 23, 36, 37, 6, 7 };
                case 36:
                    return new List<int> { 34, 23, 37, 6, 7 };
                case 37:
                    return new List<int> { 34, 23, 36, 6, 7 };
                case 38:
                    return new List<int> { 1, 6, 7, 13, 14, 15, 35 };
                default:
                    return new List<int> { 6, 7 };
            }
        }






        public static void Demo_ImportJobs()
        {

            //nodes first
            var csvTable = new DataTable();
            using (var csvReader = new CsvReader(new StreamReader(System.IO.File.OpenRead(@"C:\Users\Public\Public_fork\demo_jobs.csv")), true))
            {
                csvTable.Load(csvReader);
            }

            for (int i = 0; i < csvTable.Rows.Count; i++)
            {
                var tmpNode = new JD(JobTitle: csvTable.Rows[i][0].ToString(), url: csvTable.Rows[i][4].ToString(), company: csvTable.Rows[i][1].ToString(), location: csvTable.Rows[i][2].ToString(), salary: csvTable.Rows[i][3].ToString(), search_term: csvTable.Rows[i][5].ToString(), description: csvTable.Rows[i][6].ToString());

                tmpNode.isAR = tmpNode.location.Contains("AR");
                tmpNode.isTN = tmpNode.location.Contains("TN");

                jobs.Add(tmpNode);

            }

        }




    }



}
