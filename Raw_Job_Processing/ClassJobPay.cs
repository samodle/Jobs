using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;

namespace Raw_Job_Processing
{
    public enum JobPayType
    {
        Hourly = 0,
        Salary = 1,
        Gig = 2,
        Unknown = 3
    }

    public class JobPay
    {
        private static double HRS_PER_YEAR = 2000;

        public JobPayType PayType { get; set; }

        public double EstAnnualSalary { get; set; } 

        public Tuple<double, double> AnnualSalaryRange { get; set; }
        public Tuple<double, double> HourlySalaryRange { get; set; }


        public JobPay(string s)
        {
            if (s.Contains("hr", StringComparison.OrdinalIgnoreCase) || s.Contains("hour", StringComparison.OrdinalIgnoreCase))
            {
                PayType = JobPayType.Hourly;
            }
            else if (s.Contains("yr", StringComparison.OrdinalIgnoreCase) || s.Contains("year", StringComparison.OrdinalIgnoreCase))
            {
                PayType = JobPayType.Salary;
            }
            else { PayType = JobPayType.Unknown; }

            if ((PayType == JobPayType.Hourly || PayType == JobPayType.Salary) && s.Contains("-"))
            {
                string[] separatingStrings = { "-"};

                string[] words = s.Split(separatingStrings, StringSplitOptions.RemoveEmptyEntries);
                
                foreach(string word in words)
                {
                    word.Replace("-", "");
                    if(PayType == JobPayType.Hourly)
                    {
                        word.Replace("/hr", "");
                        word.Replace("/hour", "");
                    } 
                    else // if(PayType == JobPayType.Salary)
                    {
                        word.Replace("/yr", "");
                        word.Replace("/year", "");
                        word.Replace("k", "");
                    }
                    word.Trim();
                }

                if(words.Count() == 2)
                {

                    var lowVal = decimal.Parse(words[0], NumberStyles.AllowCurrencySymbol | NumberStyles.Number);
                    var highVal = decimal.Parse(words[1], NumberStyles.AllowCurrencySymbol | NumberStyles.Number);

                    if(PayType == JobPayType.Hourly)
                    {
                        HourlySalaryRange = new Tuple<double, double>(Convert.ToDouble(lowVal), Convert.ToDouble(highVal));
                        AnnualSalaryRange = new Tuple<double, double>(HourlySalaryRange.Item1 * HRS_PER_YEAR, HourlySalaryRange.Item2 * HRS_PER_YEAR);
                    }
                    else // if (PayType == JobPayType.Salary)
                    {
                        var a = Convert.ToDouble(lowVal);
                        var b = Convert.ToDouble(highVal);

                        if(a < 1000)
                        {
                            a = a * 1000;
                            b = b * 1000;
                        }

                        AnnualSalaryRange = new Tuple<double, double>(a, b);
                        HourlySalaryRange = new Tuple<double, double>(a / HRS_PER_YEAR, b/HRS_PER_YEAR);
                    }

                    EstAnnualSalary = (AnnualSalaryRange.Item1 + AnnualSalaryRange.Item2) / 2;
                }
                else
                {

                }


            }

        }
    }
}
