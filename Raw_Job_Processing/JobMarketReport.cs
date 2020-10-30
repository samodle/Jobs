using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Oden.Mongo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Analytics.Constants;

namespace Raw_Job_Processing
{
    /// <summary>
    /// Two Job Report Types Available:
    /// - All includes every JD that was found in the given time period
    /// - Unique includes on those JDs that FIRST appeared during that time period
    /// </summary>
    public enum JobMarketReportType
    {
        AllInTimePeriod = 0,
        UniqueInTimePeriod = 1
    }
    public class JobMarketReport
    {
        [BsonId]
        public ObjectId ID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public JobMarketReportType Type { get; set; }

        public List<ObjectId> TargetIDs { get; set; } = new List<ObjectId>();

        //metrics
        public int RemoteCount { get; set; } = 0;

        public List<Tuple<JobCommitment, int>> Commitments = new List<Tuple<JobCommitment, int>>();


        public JobMarketReport(DateTime start, DateTime end, JobMarketReportType type)
        {
            StartDate = start;
            EndDate = end;
            Type = type;
        }

        /// <summary>
        /// Saves instance of ClassJobReport to MongoDB
        /// </summary>
        public void DatabaseSave()
        {
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            MongoClient dbClient = new MongoClient(Connection.LOCAL);
            IMongoDatabase database = dbClient.GetDatabase(DB.JOB);

            var target_collection = database.GetCollection<BsonDocument>(Collection.JOB_REPORT);

            // Step Final: Store Results in Database. Strategy -> upsert document based on: start date, end date and type
            var filter = Builders<BsonDocument>.Filter.Eq("StartDate", this.StartDate) & Builders<BsonDocument>.Filter.Eq("EndDate", this.EndDate) & Builders<BsonDocument>.Filter.Eq("Type", this.Type);

            var options = new ReplaceOptions { IsUpsert = true };
            target_collection.ReplaceOne(filter, this.ToBsonDocument(), options);

            Oden.ConsoleIO.printTimeStatus(watch.Elapsed, "DB Save Complete: ");
        }


        /// <summary>
        /// Generate metrics for IDs (break into chunks, call sub routine to make metrics
        /// </summary>
        public void AnalyzeIDs()
        {
            if (TargetIDs.Count > 0)
            {
                var watch = new System.Diagnostics.Stopwatch();
                watch.Start(); 

                //Create chunks out of our IDs
                if (TargetIDs.Count <= MongoStrings.CHUNK_SIZE) { analyzeChunk(TargetIDs); }
                else
                {
                    long num_chunks = TargetIDs.Count / MongoStrings.CHUNK_SIZE;

                    if (num_chunks > 0)
                    {
                        int chunk_remainder = (int)(TargetIDs.Count % MongoStrings.CHUNK_SIZE);
                        chunk_remainder--; //for the list we're counting 0

                        int start_incrementer = 0;
                        int chunk_counter = 0;

                        var list_chunks = new List<Tuple<int, int>>();

                        for (int i = 0; i < num_chunks; i++)
                        {
                            list_chunks.Add(new Tuple<int, int>(start_incrementer, start_incrementer + MongoStrings.CHUNK_SIZE));
                            start_incrementer += MongoStrings.CHUNK_SIZE;
                        }
                        if (chunk_remainder > 0)
                            list_chunks.Add(new Tuple<int, int>(start_incrementer, start_incrementer + chunk_remainder));

                        Oden.ConsoleIO.printTimeStatus(watch.Elapsed, "Chunk Setup Complete: ");


                        //Step 3: Analyze Each Chunk (if there are chunks)
                        foreach (Tuple<int, int> chunk in list_chunks)
                        {
                            analyzeChunk(TargetIDs.Skip(chunk.Item1).Take(chunk.Item2 - chunk.Item1).ToList());

                            chunk_counter++;
                            Oden.ConsoleIO.printTimeStatus(watch.Elapsed, $"{chunk_counter} of {list_chunks.Count} in");
                        }
                    }
                    else { Console.WriteLine("ERROR - NO CHUNKS"); }
                }
            }
            else { Console.WriteLine("No IDs found! Populate ID list before running this function."); }
        }

        private void analyzeChunk(List<ObjectId> targetIDs)
        {
            MongoClient dbClient = new MongoClient(Connection.LOCAL);
            IMongoDatabase database = dbClient.GetDatabase(DB.JOB);

            var kpi_collection = database.GetCollection<BsonDocument>(Collection.JOB_KPI);

            var filter = Builders<BsonDocument>.Filter.In("_id", targetIDs);
            var kpiList = kpi_collection.Find(filter).ToList();

            foreach(BsonDocument bkpi in kpiList)
            {
                //convert to C# class object
                var kpi = BsonSerializer.Deserialize<JobKPI>(bkpi);

                //remote count
                if (kpi.isRemote)
                    RemoteCount++;

                //commitment
                int index = Commitments.FindIndex(t => t.Item1 == kpi.Commitment);
             //   if (index >= 0)
           //         Commitments[index].Item2;
             //   else
              //      Commitments.Add(new Tuple<JobCommitment, int>(kpi.Commitment, 1));

                //keywords

            }


        }

        /// <summary>
        /// Identifies JobKPI records in range for current analysis
        /// </summary>
        public void PopulateIDList()
        {
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            TargetIDs = new List<ObjectId>();

            MongoClient dbClient = new MongoClient(Connection.LOCAL);
            IMongoDatabase database = dbClient.GetDatabase(DB.JOB);

            var kpi_collection = database.GetCollection<BsonDocument>(Collection.JOB_KPI);

            //find total number of documents
            long docsInCollection = kpi_collection.CountDocuments(new BsonDocument());

            //figure out what the chunk indices will be
            long num_chunks = docsInCollection / MongoStrings.CHUNK_SIZE;
            int chunk_remainder = (int)(docsInCollection % MongoStrings.CHUNK_SIZE);

            if (num_chunks > 0 || chunk_remainder > 0)
            {
                int start_incrementer = 0;
                int chunk_counter = 0;

                var db_chunks = new List<Tuple<int, int>>();

                for (int i = 0; i < num_chunks; i++)
                {
                    db_chunks.Add(new Tuple<int, int>(start_incrementer, start_incrementer + MongoStrings.CHUNK_SIZE));
                    start_incrementer += MongoStrings.CHUNK_SIZE;
                }
                db_chunks.Add(new Tuple<int, int>(start_incrementer, start_incrementer + chunk_remainder));

                Oden.ConsoleIO.printTimeStatus(watch.Elapsed, "Setup Complete:");

                var tmp_i = 0;

                //do we want to start in the middle?
                var chunks_to_skip = 0;
                if (chunks_to_skip > 0 && chunks_to_skip < db_chunks.Count)
                {
                    tmp_i = (chunks_to_skip * MongoStrings.CHUNK_SIZE) + 1;
                    chunk_counter = chunks_to_skip;
                    db_chunks = db_chunks.Skip(chunks_to_skip).ToList();
                }

                foreach (var chunk in db_chunks)
                {
                    // get the chunk
                    var bsonDocs = Oden.Mongo.Helpers.getBSONDocs(chunk.Item1, chunk.Item2, DB.JOB, Collection.JOB_KPI);

                    if (bsonDocs.Count > 0)
                    {
                        foreach (var b in bsonDocs)
                        {
                            //convert to C# class object
                            var jd_kpi = BsonSerializer.Deserialize<JobKPI>(b);

                            //see if we want to keep it, add it to our list
                            if (Type == JobMarketReportType.AllInTimePeriod && jd_kpi.isPresentInRange(StartDate, EndDate))
                            {
                                TargetIDs.Add(jd_kpi.ID);
                            }
                            else if (Type == JobMarketReportType.UniqueInTimePeriod && jd_kpi.isNewInRange(StartDate, EndDate))
                            {
                                TargetIDs.Add(jd_kpi.ID);
                            }
                            //keep track of how many we've done
                            tmp_i++;
                        }
                    }
                    else
                    {
                        Console.WriteLine("ERROR - EMPTY CHUNK!!!!");
                    }

                    chunk_counter++;
                    Oden.ConsoleIO.printTimeStatus(watch.Elapsed, chunk_counter.ToString() + " of " + db_chunks.Count.ToString() + " in", tmp_i.ToString() + " Jobs Analyzed.");
                }
            }
            else
            {
                Console.WriteLine("NO CHUNKS!!!");
            }
        }

        public override string ToString()
        {
            return $"{StartDate} - {EndDate}, Type:  {Enum.GetName(typeof(JobMarketReportType), Type)}";
        }
    }
}
