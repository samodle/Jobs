using Analytics;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Raw_Job_Processing
{
    public static class DataCleaning
    {
        public static RawJobDescription CleanJobDescription(RawJobDescription rawJob)
        {
            rawJob.company.Trim();
            rawJob.location.Trim();
            rawJob.JobTitle.Trim();


            return rawJob;
        }
    }
}
