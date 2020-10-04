﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Analytics.Constants;

namespace Raw_Job_Processing
{
    public static class JobAnalysis
    {
        public static async void AnalyzeJobs()
        {
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            MongoClient dbClient = new MongoClient(MongoStrings.CONNECTION);
            IMongoDatabase database = dbClient.GetDatabase(MongoStrings.JOB_DB);

            var raw_collection = database.GetCollection<BsonDocument>(MongoStrings.JOB_COLLECTION);
            var kpi_collection = database.GetCollection<BsonDocument>(MongoStrings.JOB_KPI_COLLECTION);

            //find total number of documents
            long docsInCollection = raw_collection.CountDocuments(new BsonDocument());

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
                    db_chunks.Add(new Tuple<int, int>(start_incrementer, start_incrementer + MongoStrings.CHUNK_SIZE));
                    start_incrementer += MongoStrings.CHUNK_SIZE;
                }
                db_chunks.Add(new Tuple<int, int>(start_incrementer, start_incrementer + chunk_remainder));

                // Get the elapsed time as a TimeSpan value.
                TimeSpan ts = watch.Elapsed;

                // Format and display the TimeSpan value.

                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
                Console.WriteLine($"Setup Complete: {elapsedTime}");

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
                    var bsonDocs = MongoExport.getSomeJDs(chunk.Item1, chunk.Item2);

                    if (bsonDocs.Count > 0)
                    {
                       // var kpiWriteList = new List<JobKPI>();

                        foreach (var b in bsonDocs)
                        {
                            //convert to C# class object
                            var jd = BsonSerializer.Deserialize<RawJobDescription>(b);

                            //use class function to generate job kpi report
                            var jd_kpi = jd.getJobKPI();

                            //clean up dates (and search terms?)
                            jd_kpi.Clean();

                            //add it to the list
                            var options = new ReplaceOptions { IsUpsert = true };
                            var result = await kpi_collection.ReplaceOneAsync(new BsonDocument(), jd_kpi.ToBsonDocument(), options);

                            //keep track of how many we've done
                            tmp_i++;
                        }

                        //update||insert job kpi into database
                        /*
                        var EmpInfoArray = new List<BsonDocument>();

                        foreach (JobKPI j in kpiWriteList)
                        {
                            EmpInfoArray.Add(j.ToBsonDocument());
                        }

                        var options = new UpdateOptions { IsUpsert = true };
                        var result = await kpi_collection.UpdateManyAsync(new BsonDocument(), EmpInfoArray, options);
                        */
                    }
                    else
                    {
                        Console.WriteLine("ERROR - EMPTY CHUNK!!!!");
                    }

                    // Get the elapsed time as a TimeSpan value.
                    ts = watch.Elapsed;

                    // Format and display the TimeSpan value.
                    elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                    ts.Hours, ts.Minutes, ts.Seconds,
                    ts.Milliseconds / 10);

                    chunk_counter++;
                    Console.WriteLine(chunk_counter.ToString() + " of " + db_chunks.Count.ToString() + " in " + elapsedTime + ". " + tmp_i.ToString() + " Jobs Analyzed.");
                }

            }
            else
            {
                Console.WriteLine("NO CHUNKS!!!");
            }

        }
    }
}