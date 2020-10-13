using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Raw_Job_Processing
{
    public enum JobCommitment
    {
        FullTime = 0,
        PartTime = 1,
        Contractor = 2,
        Unknown = 3
    }

    public enum JobSource
    {
        Monster = 0,
        CareerBuilder = 1,
        Indeed = 2,
        Unknown = 3
    }


    public class PaySummary
    {
        public JobPayType pType { get; set; }
    }

    public class JobKPI
    {
        #region Properties
        [BsonId]
        public ObjectId ID { get; set; }
        public bool isRemote { get; set; } = false;
        public string JobTitle { get; set; }
        public string Company { get; set; }
        public JobCommitment Commitment { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public List<DateTime> DatesFound { get; set; } = new List<DateTime>();
        public JobSource Source { get; set; }

        public List<string> AttributeReport { get; set; } = new List<string>();
        public List<string> Labels { get; set; } = new List<string>();

        public JobPay Pay { get; set; }
        //pay low
        //pay high
        #endregion

        public bool isPresentInRange(DateTime earliest, DateTime latest)
        {
            foreach(DateTime d in DatesFound)
            {
                if(d >= earliest && d <= latest) { return true; }
            }
            return false;
        }

        public bool isNewInRange(DateTime earliest, DateTime latest)
        {
            DateTime firstFound = getDateFirstDiscovered();
            return (firstFound >= earliest && firstFound <= latest);
        }

        //has this jd existed in the last n days?
        public bool isPresentLastNDays(int n)
        {
            var nowDate = DateTime.Now.Date;
            var testDate = nowDate.AddDays(-n);

            return getMostRecentDate() >= testDate;
        }

        //was jd discovered in the last n days?
        public bool isNewLastNDays(int n)
        {
            var nowDate = DateTime.Now.Date;
            var testDate = nowDate.AddDays(-n);

            return getDateFirstDiscovered() >= testDate;
        }

        //latest date = max datetime
        public DateTime getMostRecentDate()
        {
            return DatesFound.Max();
        }

        //earliest date = min datetime
        public DateTime getDateFirstDiscovered()
        {
            return DatesFound.Min();
        }

        public JobKPI(RawJobDescription rjd)
        {
            JobTitle = rjd.JobTitle;
            Company = rjd.company;
            ID = rjd.ID;

            //Dates
            foreach (DateTime d in rjd.dates_found)
            {
                this.DatesFound.Add(d.Date);
            }
            this.DatesFound = this.DatesFound.Distinct().ToList();

            //search terms / labels
            this.Labels = rjd.search_terms.Distinct().ToList();

            //Commitment
            if (rjd.commitment.Contains("Contractor")) { Commitment = JobCommitment.Contractor; }
            else if (rjd.commitment.Contains("Full")) { Commitment = JobCommitment.FullTime; }
            else if (rjd.commitment.Contains("Part")) { Commitment = JobCommitment.PartTime; }
            else { Commitment = JobCommitment.Unknown; }


            //Location & Remote
            if (rjd.location.Contains("remote", StringComparison.OrdinalIgnoreCase))
            {
                isRemote = true;
            }
            else
            {
                SetCityState(rjd.location);

                //detailed location info

                //check description for remote
                if (rjd.description.Contains("remote", StringComparison.OrdinalIgnoreCase)) { isRemote = true; }
            }

            //source
            if (rjd.source.Contains("monster", StringComparison.OrdinalIgnoreCase)) { Source = JobSource.Monster; }
            else if (rjd.source.Contains("indeed", StringComparison.OrdinalIgnoreCase)) { Source = JobSource.Indeed; }
            else if (rjd.source.Contains("career", StringComparison.OrdinalIgnoreCase)) { Source = JobSource.CareerBuilder; }
            else { Source = JobSource.Unknown; }

            //pay
            Pay = new JobPay(rjd.salary);

            //attributes
            foreach(JDAttribute a in JobAnalysis.ALL_JD_ATTRIBUTES)
            {
                if(a.Type == JDAttributeType.ProgrammingLanguage)
                {
                    foreach (string searchTerm in a.SearchTerms)
                    {
                        var newTerm = searchTerm + " ";
                        if (rjd.description.Contains(newTerm))
                        {
                            AttributeReport.Add(a.Name);
                            break;
                        }
                    }
                }
                else
                {
                    foreach (string searchTerm in a.SearchTerms)
                    {
                        if (rjd.description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                        {
                            AttributeReport.Add(a.Name);
                            break;
                        }
                    }
                }
            }
        }

        private void SetCityState(string sAddress)
        {
            //string[] split = sAddress.Split(new Char[] { ' ', });
            //return split;
            sAddress = sAddress.Replace("-", ",");
            sAddress = sAddress.Replace(".", "");
            sAddress = sAddress.Replace(",", ", ");
            sAddress = sAddress.Replace("  ", " ").Trim();

            if (sAddress.Length > 2)
            {
                //Regex addressPattern = new Regex(@"(?<city>[A-Za-z',.\s]+) (?<state>([A-Za-z]{2}|[A-Za-z]{2},))\s*(?<zip>\d{5}(-\d{4})|\d{5})");
                Regex addressPattern = new Regex(@"(?<city>[A-Za-z',.\s]+) (?<state>([A-Za-z]{2}|[A-Za-z]{2}))");

                MatchCollection matches = addressPattern.Matches(sAddress);

                if (matches.Count > 0)
                {  //for (int mc = 0; mc < matches.Count; mc++)
                    var tmpCity = matches[0].Groups["city"].Value;
                    var tmpState = matches[0].Groups["state"].Value;

                    City = tmpCity.Replace(",", "").Trim();
                    State = tmpState.Trim();
                }
                else
                {
                    addressPattern = new Regex(@"(?<state>([A-Za-z]{2}|[A-Za-z]{2})) (?<city>[A-Za-z',.\s]+)");

                    matches = addressPattern.Matches(sAddress);

                    if (matches.Count > 0)
                    {  //for (int mc = 0; mc < matches.Count; mc++)
                        var tmpCity = matches[0].Groups["city"].Value;
                        var tmpState = matches[0].Groups["state"].Value;

                        City = tmpCity.Replace(",", "").Trim();
                        State = tmpState.Trim();
                    }
                    else
                    { }
                }
            }
        }
    }
}
