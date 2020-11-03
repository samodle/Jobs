using Oden;
using Oden.Enums;
using Oden.Profession;
using System;
using System.Globalization;
using System.Linq;

namespace OdenGenerator
{
    public static class OdenGen
    {
        public static Pay getPayFromString(string s)
        {
            Pay p = new Pay();

            if (s.Contains("hr", StringComparison.OrdinalIgnoreCase) || s.Contains("hour", StringComparison.OrdinalIgnoreCase))
            {
                p.PayType = JobPayType.Hourly;
            }
            else if (s.Contains("yr", StringComparison.OrdinalIgnoreCase) || s.Contains("year", StringComparison.OrdinalIgnoreCase))
            {
                p.PayType = JobPayType.Salary;
            }
            else { p.PayType = JobPayType.Unknown; }

            if ((p.PayType == JobPayType.Hourly || p.PayType == JobPayType.Salary) && s.Contains("-"))
            {
                string[] separatingStrings = { "-" };

                string[] words = s.Split(separatingStrings, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < words.Count(); i++)
                {
                    if (p.PayType == JobPayType.Hourly)
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

                if (words.Count() == 2)
                {
                    decimal highVal, lowVal;

                    if (s.Contains("£"))
                    {
                        p.Currency = CurrencyType.BritishPound;

                        if (p.PayType == JobPayType.Salary)
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

                    if (p.PayType == JobPayType.Hourly)
                    {
                        p.HourlySalaryRange = new Tuple<double, double>(Convert.ToDouble(lowVal), Convert.ToDouble(highVal));
                        p.AnnualSalaryRange = new Tuple<double, double>(p.HourlySalaryRange.Item1 * Constants.HRS_PER_YEAR, p.HourlySalaryRange.Item2 * Constants.HRS_PER_YEAR);
                    }
                    else // if (PayType == JobPayType.Salary)
                    {
                        var a = Convert.ToDouble(lowVal);
                        var b = Convert.ToDouble(highVal);

                        if (a < 1000)
                        {
                            a = a * 1000;
                            b = b * 1000;
                        }

                        p.AnnualSalaryRange = new Tuple<double, double>(a, b);
                        p.HourlySalaryRange = new Tuple<double, double>(a / Constants.HRS_PER_YEAR, b / Constants.HRS_PER_YEAR);
                    }

                    p.EstAnnualSalary = (p.AnnualSalaryRange.Item1 + p.AnnualSalaryRange.Item2) / 2;
                }
                else
                {
                    Oden.ConsoleIO.printEmphStatus("WHY ARE WE HERE????? PayGeneration.cs line 102ish");
                }

                
            }
            return p;
        }

    }

}


