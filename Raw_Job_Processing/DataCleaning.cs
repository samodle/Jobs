using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Raw_Job_Processing
{
    [BsonIgnoreExtraElements]
    public class RawJobDescription : IEquatable<RawJobDescription>
    {
        public string JobTitle { get; set; }
        [BsonId]
        public ObjectId ID { get; set; }
        public string url { get; set; }
        public int CompanyID { get; set; }
        public string company { get; set; }
        public string location { get; set; }
        public string? rating { get; set; }
        public string? salary { get; set; }
        public string? commitment { get; set; }
        public string source { get; set; }
        public string description { get; set; }
        public string description_cleaned { get; set; }
        public string post_date { get; set; }
        public List<string> search_terms { get; set; } 
        public List<DateTime> dates_found { get; set; }


        public JobKPI getJobKPI()
        {
            var newKPI = new JobKPI();

            newKPI.JobTitle = this.JobTitle;

            //consolidate dates
            foreach(DateTime d in dates_found)
            {
                newKPI.DatesFound.Add(d.Date);
            }
            newKPI.DatesFound = newKPI.DatesFound.Distinct().ToList();


            return new JobKPI();
        }

        public bool Equals(RawJobDescription other)
        {
            if (other.company.Equals(this.company, StringComparison.OrdinalIgnoreCase))
            {
                if (other.location.Equals(this.location, StringComparison.OrdinalIgnoreCase))
                {
                    if (other.JobTitle.Equals(this.JobTitle, StringComparison.OrdinalIgnoreCase))
                    {
                        if (other.source.Equals(this.source, StringComparison.OrdinalIgnoreCase))
                        { 
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public override string ToString()
        {
            return company + ", Title: " + JobTitle + ", Location: " + location + ", Source: " + source;
        }
    }


    public static class DataCleaning
    {
        public static RawJobDescription CleanJobDescription(RawJobDescription rawJob)
        {
            rawJob.company.Trim();
            rawJob.location.Trim();
            rawJob.JobTitle.Trim();

            if(rawJob.commitment is null) { rawJob.commitment = ""; }
            if (rawJob.salary is null) { rawJob.salary = ""; }
            if (rawJob.rating is null) { rawJob.rating = ""; }

            return rawJob;
        }
    }
}
