using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using static Analytics.Constants;

namespace Raw_Job_Processing
{
    static class JobAnalysis
    {
        static void AnalyzeJobs()
        {
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            MongoClient dbClient = new MongoClient(MongoStrings.CONNECTION);
            IMongoDatabase database = dbClient.GetDatabase(MongoStrings.JOB_DB);
            var raw_collection = database.GetCollection<BsonDocument>(MongoStrings.JOB_COLLECTION);



        }
    }
}
