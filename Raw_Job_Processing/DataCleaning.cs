using Analytics;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Raw_Job_Processing
{
    //[BsonId]
    //public ObjectId _id { get; set; }
    //[BsonElement("Id")]
    // public string Id { get; set; }
    // [BsonElement("Title")]

    //Job Description Straight From The Database
    [BsonIgnoreExtraElements]
    [JsonObject(MemberSerialization.OptIn)]
    public class RawJobDescription : IEquatable<RawJobDescription>
    {
        [JsonProperty]
        public string JobTitle { get; set; }
        [BsonId]
        [JsonProperty]
        public ObjectId ID { get; set; }
        public string url { get; set; }
        public int CompanyID { get; set; }
        [JsonProperty]
        public string company { get; set; }
        [JsonProperty]
        public string location { get; set; }
        public string? rating { get; set; }
        public string? salary { get; set; }
        public string? commitment { get; set; }
        [JsonProperty]
        public string search_term { get; set; }
        [JsonProperty]
        public string source { get; set; }
        [JsonProperty]
        public string description { get; set; }
        [JsonProperty]
        public string description_cleaned { get; set; }
        public DateTime date_found { get; set; }
        public string post_date { get; set; }
        [JsonProperty]
        public List<string> search_terms { get; set; } = new List<string>();
        public List<DateTime> dates_found { get; set; } = new List<DateTime>();


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

            if(rawJob.search_terms.Count == 0)
            {
                rawJob.search_terms.Add(rawJob.search_term);
            }

            if (rawJob.dates_found.Count == 0 && rawJob.date_found != null) // && rawJob.date_found.Length > 2)
            {
                //  rawJob.dates_found.Add(DateTime.Parse(rawJob.date_found));
                rawJob.dates_found.Add(rawJob.date_found);
            }

            return rawJob;
        }
    }
}
