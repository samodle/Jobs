using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raw_Job_Processing
{
    public enum JobCommitment 
    {
        FullTime = 0,
        PartTime = 1,
        Contractor = 2,
        Unknown = 3
    }

    public enum JobPayType
    {
        Hourly = 0,
        Salary = 1,
        Unknown = 2
    }

    public class PaySummary
    {
        public JobPayType pType { get; set; }
    }


    public class JobKPI
    {
        [BsonId]
        public ObjectId ID { get; set; }
        public bool isRemote { get; set; }
        public string JobTitle { get; set; }
        public JobCommitment Commitment { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public List<DateTime> DatesFound { get; set; }

        //pay low
        //pay high

        public void Clean()
        { 
            foreach(DateTime d in DatesFound)
            {

            }
        }

    }
}
