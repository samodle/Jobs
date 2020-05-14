/*using System;
using System.Collections.Generic;
using Telerik.Windows.Controls.ChartView;
using System.Windows.Media;

namespace TelerikChartObjects
{
    public class StepChangeChartMaster
    {
        public List<double> Actual;
        public List<double> Steps;

        public List<DateTime> Dates;

        public StepChangeChartMaster(List<double> Actual, List<double> Steps, List<DateTime> Dates)
        {
            this.Steps = Steps;
            this.Actual = Actual;
            this.Dates = Dates;
        }

        public List<StepChangeChart> getChartData()
        {
            var tmpList = new List<StepChangeChart>();
            for (int i = 0; i < Actual.Count; i++)
            {
                tmpList.Add(new StepChangeChart(Actual[i], Dates[i], Steps[i]));
            }
            return tmpList;
        }
    }



    public class StepChangeChart
    {
        public double Actual { get; set; }
        public DateTime Date { get; set; }
        public double Step { get; set; }

        public StepChangeChart(double Actual, DateTime Date, Double Step)
        {
            this.Actual = Actual;
            this.Date = Date;
            this.Step = Step;
        }

    }
}
*/