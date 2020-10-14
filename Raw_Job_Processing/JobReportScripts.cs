using System;
using System.Collections.Generic;
using System.Text;

namespace Raw_Job_Processing
{
    public static class JobReportScripts
    {
        public static void WeeklyReport()
        {
            var thisPastWeek = new ClassJobReport(DateTime.Now.AddDays(-7).Date, DateTime.Now.Date, ClassJobReportType.AllInTimePeriod);
            thisPastWeek.PopulateIDList(); //find IDs for current query
            thisPastWeek.AnalyzeIDs(); //populate KPIs
            thisPastWeek.DatabaseSave(); //save to db
        }
    }
}
