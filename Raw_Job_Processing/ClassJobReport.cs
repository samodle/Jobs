using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Analytics.Constants;

namespace Raw_Job_Processing
{
    public enum ClassJobReportType
    {
        AllInTimePeriod = 0,
        UniqueInTimePeriod = 1
    }
    class ClassJobReport
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ClassJobReportType Type { get; set; }

        public List<ObjectId> TargetIDs { get; set; } = new List<ObjectId>();


        public ClassJobReport(DateTime start, DateTime end, ClassJobReportType type)
        {
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            StartDate = start;
            EndDate = end;
            Type = type;

            // Step 1: Iterate through database to find all KPIs in range, generating an ID list
            findTargetIDs();
            Helpers.printTimeStatus(watch.Elapsed, "Target ID's 100% Identified: ");

            // Step 2: Create chunks out of our IDs
            if (TargetIDs.Count <= MongoStrings.CHUNK_SIZE) { analyzeChunk(TargetIDs); }
            else
            {
                long num_chunks = TargetIDs.Count / MongoStrings.CHUNK_SIZE;

                if (num_chunks > 0)
                {
                    int chunk_remainder = (int)(TargetIDs.Count % MongoStrings.CHUNK_SIZE);

                    int start_incrementer = 0;
                    int chunk_counter = 0;

                    var db_chunks = new List<Tuple<int, int>>();

                    for (int i = 0; i < num_chunks; i++)
                    {
                        db_chunks.Add(new Tuple<int, int>(start_incrementer, start_incrementer + MongoStrings.CHUNK_SIZE));
                        start_incrementer += MongoStrings.CHUNK_SIZE;
                    }
                    db_chunks.Add(new Tuple<int, int>(start_incrementer, start_incrementer + chunk_remainder));

                    Helpers.printTimeStatus(watch.Elapsed, "Chunk Setup Complete: ");
                }
            }


            //Step 3: Analyze Each Chunk (if there are chunks)


            // Step ?: Store Results in Database

            Helpers.printTimeStatus(watch.Elapsed, "Execution Complete: ");
        }


        private void analyzeChunk(List<ObjectId> targetIDs)
        {

        }

        private void findTargetIDs()
        {
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            MongoClient dbClient = new MongoClient(MongoStrings.CONNECTION);
            IMongoDatabase database = dbClient.GetDatabase(MongoStrings.JOB_DB);

            var kpi_collection = database.GetCollection<BsonDocument>(MongoStrings.JOB_KPI_COLLECTION);
            var report_collection = database.GetCollection<BsonDocument>(MongoStrings.JOB_REPORT_COLLECTION);

            //find total number of documents
            long docsInCollection = kpi_collection.CountDocuments(new BsonDocument());

            //figure out what the chunk indices will be
            long num_chunks = docsInCollection / MongoStrings.CHUNK_SIZE;

            if (num_chunks > 0)
            {
                int chunk_remainder = (int)(docsInCollection % MongoStrings.CHUNK_SIZE);

                int start_incrementer = 0;
                int chunk_counter = 0;

                var db_chunks = new List<Tuple<int, int>>();

                for (int i = 0; i < num_chunks; i++)
                {
                    db_chunks.Add(new Tuple<int, int>(i == 0? start_incrementer : start_incrementer++, start_incrementer + MongoStrings.CHUNK_SIZE));
                    start_incrementer += MongoStrings.CHUNK_SIZE;
                }
                db_chunks.Add(new Tuple<int, int>(start_incrementer, start_incrementer + chunk_remainder));

                Helpers.printTimeStatus(watch.Elapsed, "Setup Complete:");

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
                    var bsonDocs = MongoExport.getBSONDocs(chunk.Item1, chunk.Item2, MongoStrings.JOB_DB, MongoStrings.JOB_KPI_COLLECTION);

                    if (bsonDocs.Count > 0)
                    {
                        foreach (var b in bsonDocs)
                        {
                            //convert to C# class object
                            var jd_kpi = BsonSerializer.Deserialize<JobKPI>(b);

                            //see if we want to keep it, add it to our list
                            if (Type == ClassJobReportType.AllInTimePeriod && jd_kpi.isPresentInRange(StartDate, EndDate))
                            {
                                TargetIDs.Add(jd_kpi.ID);
                            }
                            else if (Type == ClassJobReportType.UniqueInTimePeriod && jd_kpi.isNewInRange(StartDate, EndDate))
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
                    Helpers.printTimeStatus(watch.Elapsed, chunk_counter.ToString() + " of " + db_chunks.Count.ToString() + " in", tmp_i.ToString() + " Jobs Analyzed.");
                }

            }
            else
            {
                Console.WriteLine("NO CHUNKS!!!");
            }

        }

        public override string ToString()
        {
            return $"{StartDate} - {EndDate}, Type:  {Enum.GetName(typeof(ClassJobReportType), Type)}";
        }
    }
}
