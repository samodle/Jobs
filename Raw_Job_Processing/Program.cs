using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Raw_Job_Processing
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            MongoClient dbClient = new MongoClient("mongodb://forkAdmin:ForkAdmin123@localhost:27017");
            var database = dbClient.GetDatabase("graphs");
            var raw_collection = database.GetCollection<BsonDocument>("job_descriptions");
            var clean_collection = database.GetCollection<BsonDocument>("jobs_cleaned");
        }
    }
}
