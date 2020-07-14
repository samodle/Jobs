using System;
using System.Collections.Generic;
using System.Linq;
using Analytics;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Raw_Job_Processing
{
    class Program
    {


        static void Main(string[] args)
        {
            Console.WriteLine("Connecting To Database...");

            removeDupes();



            Console.WriteLine("Shutting Down...");
        }


        private static void removeDupes()
        {
            MongoClient dbClient = new MongoClient("mongodb://forkAdmin:ForkAdmin123@localhost:27017");
            IMongoDatabase database = dbClient.GetDatabase("jobs");
            var raw_collection = database.GetCollection<BsonDocument>("job_descriptions");
            var clean_collection = database.GetCollection<BsonDocument>("jobs_cleaned");

            var rawData = raw_collection.Find(_=> true).ToList();

            List<RawJobDescription> rawJobs = new List<RawJobDescription>();

            foreach (var a in rawData)
            {
                var b = BsonSerializer.Deserialize<RawJobDescription>(a);       
                rawJobs.Add(DataCleaning.CleanJobDescription(b));
            }

            var cleanedJobs = rawJobs.Distinct().ToList();

            var EmpInfoArray = new List<BsonDocument>();

            foreach(RawJobDescription j in cleanedJobs)
            {
                EmpInfoArray.Add(j.ToBsonDocument());
            }

            try
            {
                clean_collection.InsertMany(EmpInfoArray);
            }
            catch (Exception e)
            {
                int ie = 0;
            }

        }
    }
}
