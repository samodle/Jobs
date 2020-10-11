using System;
using System.Collections.Generic;
using System.Text;

namespace Raw_Job_Processing
{
    public static class Helpers
    {
        public static void printTimeStatus(TimeSpan ts, string messageA = "Time Elapsed: ", string messageB = "")
        {
            // Format and display the TimeSpan value.
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);

            Console.WriteLine($"{messageA} {elapsedTime} {messageB}");
        }
    }
}
