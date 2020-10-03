using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Raw_Job_Processing
{
    public class ClassJobKPI
    {
        [BsonId]
        public ObjectId ID { get; set; }
        public bool isRemote { get; set; }
    }
}
