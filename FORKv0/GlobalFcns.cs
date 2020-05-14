using DataInterface;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using static ForkAnalyticsSettings.GlobalConstants;

public static class MyListExtensions
{
    public static double Mean(this List<double> values)
    {
        return values.Count == 0 ? 0 : values.Mean(0, values.Count);
    }

    private static double Mean(this List<double> values, int start, int end)
    {
        double s = 0;

        for (int i = start; i < end; i++)
        {
            s += values[i];
        }

        return s / (end - start);
    }


    public static double Variance(this List<double> values)
    {
        return values.Variance(values.Mean(), 0, values.Count);
    }

    // public static double Variance(this List<double> values, double mean)
    //{
    //   return values.Variance(mean, 0, values.Count);
    //}

    private static double Variance(this List<double> values, double mean, int start, int end)
    {
        double variance = 0;

        for (int i = start; i < end; i++)
        {
            variance += Math.Pow((values[i] - mean), 2);
        }

        int n = end - start;
        if (start > 0) { n -= 1; }

        return variance / (n);
    }



    public static double StandardDeviation(this List<double> values)
    {
        return values.Count == 0 ? 0 : values.StandardDeviation(0, values.Count);
    }

    public static double StandardDeviation(this List<double> values, int start, int end)
    {
        double mean = values.Mean(start, end);
        double variance = values.Variance(mean, start, end);

        return Math.Sqrt(variance);
    }
}

public static class StringExtensions
{
    public static string Left(this string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        maxLength = Math.Abs(maxLength);

        return (value.Length <= maxLength
               ? value
               : value.Substring(0, maxLength)
               );
    }

    public static string Right(this string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        maxLength = Math.Abs(maxLength);

        return (value.Length <= maxLength
               ? value
               : value.Substring(value.Length - maxLength, maxLength)
               );
    }
}


namespace Windows_Desktop
{
    static class GlobalFcns
    {

        public static Tuple<DateTime, DateTime> getMonthStartEndTimes(DateTime dateTime)
        {
            return new Tuple<DateTime, DateTime>(FirstDayOfMonthFromDateTime(dateTime), LastDayOfMonthFromDateTime(dateTime));
        }
        public static DateTime FirstDayOfMonthFromDateTime(DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, 1);
        }

        public static DateTime LastDayOfMonthFromDateTime(DateTime dateTime)
        {
            DateTime firstDayOfTheMonth = new DateTime(dateTime.Year, dateTime.Month, 1);
            return firstDayOfTheMonth.AddMonths(1).AddDays(-1);
        }

        public static int GetHashCode(string value)
        {
            int h = 0;
            for (int i = 0; i < value.Length; i++)
                h += value[i] * 31 ^ value.Length - (i + 1);
            return h;
        }

        public static void sortEventList_ByStops(ref List<DTeventSummary> tgtList)
        {
            int i = 0;
            for (i = 0; i <= tgtList.Count - 1; i++)
            {
                tgtList[i].setSortParam(DowntimeMetrics.Stops);  //sortBy_Stops();
            }
            tgtList.Sort();
        }
        public static void sortEventList_ByDT(ref List<DTeventSummary> tgtList)
        {
            int i = 0;
            for (i = 0; i <= tgtList.Count - 1; i++)
            {
                tgtList[i].setSortParam(DowntimeMetrics.DT);
            }
            tgtList.Sort();
        }



        public static string onlyDigits(string s)
        {
            string resultString = null;

            Regex regexObj = new Regex(@"[^\d]");
            resultString = regexObj.Replace(s, "");
            return resultString;

        }



        public static bool CheckIfFtpFileExists(string fileUri, string username, string password)
        {


            System.Net.FtpWebRequest request = (System.Net.FtpWebRequest)System.Net.WebRequest.Create(fileUri);
            request.Credentials = new System.Net.NetworkCredential(username, password);
            //request.Method = System.Net.WebRequestMethods.Ftp.UploadFile


            //Dim request As FtpWebRequest = WebRequest.Create(fileUri)
            //request.Credentials = New NetworkCredential(username, password)
            request.Method = WebRequestMethods.Ftp.GetFileSize;
            try
            {
                FtpWebResponse response = (System.Net.FtpWebResponse)request.GetResponse();
                // THE FILE EXISTS
            }
            catch (WebException ex)
            {
                FtpWebResponse response = (System.Net.FtpWebResponse)ex.Response;
                if (FtpStatusCode.ActionNotTakenFileUnavailable == response.StatusCode)
                {
                    // THE FILE DOES NOT EXIST
                    return false;
                }
            }
            return true;
        }
        public static object ReturnNullifzero(double number_to_analyze)
        {
            if (number_to_analyze <= 0)
            {
                return "null";
            }
            else
            {
                return number_to_analyze;
            }

        }



    }

}


