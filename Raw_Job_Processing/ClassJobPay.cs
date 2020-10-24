using Helper;
using Oden;
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

    public enum CurrencyType
    {
        USDollar = 0,
        BritishPound = 1,
        Euro = 2,
        CanadianDollar = 3
    }

    public class JobPay
    {
        private static double HRS_PER_YEAR = 2000;

        public JobPayType PayType { get; set; }
        public CurrencyType Currency { get; set; } = CurrencyType.USDollar;
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
                
                for(int i = 0; i < words.Count(); i++)
                {
                    if(PayType == JobPayType.Hourly)
                    {
                        words[i] = words[i].Replace("/hr", "");
                        words[i] = words[i].Replace("an hour", "");
                        words[i] = words[i].Replace("/hour", "");
                    } 
                    else // if(PayType == JobPayType.Salary)
                    {
                        words[i] = words[i].Replace("/yr", "");
                        words[i] = words[i].Replace("/year", "");
                        words[i] = words[i].Replace("a year", "");
                        words[i] = words[i].Replace("k", "");
                    }
                    words[i] = words[i].Trim();
                }

                if(words.Count() == 2)
                {
                    decimal highVal, lowVal;

                    if (s.Contains("£"))
                    {
                        Currency = CurrencyType.BritishPound;

                        if(PayType == JobPayType.Salary)
                        {
                            lowVal = words[0].OnlyDigits();
                            highVal = words[1].OnlyDigits();
                        }
                        else // hourly
                        {
                            words[0] = words[0].Replace("£", "$");
                            words[1] = words[1].Replace("£", "$");

                            lowVal = decimal.Parse(words[0], NumberStyles.AllowCurrencySymbol | NumberStyles.Number);
                            highVal = decimal.Parse(words[1], NumberStyles.AllowCurrencySymbol | NumberStyles.Number);
                        }

                    }
                    else
                    {
                        lowVal = decimal.Parse(words[0], NumberStyles.AllowCurrencySymbol | NumberStyles.Number);
                        highVal = decimal.Parse(words[1], NumberStyles.AllowCurrencySymbol | NumberStyles.Number);
                    }

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
