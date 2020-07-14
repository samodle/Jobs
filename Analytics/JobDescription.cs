using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Analytics
{

    //[BsonId]
    //public ObjectId _id { get; set; }
    //[BsonElement("Id")]
   // public string Id { get; set; }
   // [BsonElement("Title")]

    //Job Description Straight From The Database
    [BsonIgnoreExtraElements]
    public class RawJobDescription : IEquatable<RawJobDescription>
    {
        public string JobTitle { get; set; }
        public string url { get; set; }
        public int CompanyID { get; set; }
        public string company { get; set; }
        public string location { get; set; }
        public double rating { get; set; }
        public string salary { get; set; }
        public string commitment { get; set; }
        public string search_term { get; set; }
        public string source { get; set; }
        public string description { get; set; }


        public bool Equals(RawJobDescription other)
        {
               if (other.company.Equals(company, StringComparison.OrdinalIgnoreCase))
               {
                    if (other.location.Equals(location, StringComparison.OrdinalIgnoreCase))
                    {
                        if (other.JobTitle.Equals(JobTitle, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                }
            }
            return false;
         }

    }
}
