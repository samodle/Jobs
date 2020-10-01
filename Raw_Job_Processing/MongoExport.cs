using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Analytics.Constants;

namespace Raw_Job_Processing
{
    public static class MongoExport
    {
        public static List<BsonDocument> getSomeJDs(int indexA, int indexB)
        {
            //connect to database, get appropriate database and collection
            MongoClient dbClient = new MongoClient(MongoStrings.CONNECTION);
            IMongoDatabase database = dbClient.GetDatabase(MongoStrings.JOB_DB);
            var raw_collection = database.GetCollection<BsonDocument>(MongoStrings.JOB_COLLECTION);

            return raw_collection.Find(FilterDefinition<BsonDocument>.Empty).Skip(indexA).Limit(indexB - indexA).ToList();
        }

        public static void JSON_Export_JD(BsonDocument bDoc, string FileName, string FileType = ".json")
        {
            var exportObject = BsonSerializer.Deserialize<RawJobDescription>(bDoc);
            string jsonData = JsonConvert.SerializeObject(exportObject);
            string fileName = Helper.Publics.FILEPATHS.PATH_BIG_EXPORT + FileName + FileType;
            FileStream fcreate = File.Open(fileName, FileMode.Create);
            using (StreamWriter writer = new StreamWriter(fcreate))
            {
                writer.Write(jsonData);
                writer.Close();
            }
        }

        public static void ExportEachDocument()
        {
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            //connect to database, get appropriate database and collection
            MongoClient dbClient = new MongoClient(MongoStrings.CONNECTION);
            IMongoDatabase database = dbClient.GetDatabase(MongoStrings.JOB_DB);
            var raw_collection = database.GetCollection<BsonDocument>(MongoStrings.JOB_COLLECTION);

            //find total number of documents
            long docsInCollection = raw_collection.CountDocuments(new BsonDocument());

            //figure out what the chunk indices will be
            long num_chunks = docsInCollection / MongoStrings.CHUNK_SIZE;

            if(num_chunks > 0)
            {
                int chunk_remainder = (int)(docsInCollection % MongoStrings.CHUNK_SIZE);

                int start_incrementer = 0;
                int chunk_counter = 0;

                var db_chunks = new List<Tuple<int, int>>();

                for(int i = 0; i < num_chunks; i++)
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
                Console.WriteLine($"Setup Complete: " + elapsedTime);

                var tmp_i = 0;

                //do we want to start in the middle?
                var chunks_to_skip = 8;

                if(chunks_to_skip > 0 && chunks_to_skip < db_chunks.Count)
                {
                    tmp_i = (chunks_to_skip * MongoStrings.CHUNK_SIZE) + 1;
                    chunk_counter = chunks_to_skip;
                    db_chunks = db_chunks.Skip(chunks_to_skip).ToList();
                }

                foreach (var chunk in db_chunks)
                {
                        // get the chunk
                        var bsonDocs = getSomeJDs(chunk.Item1, chunk.Item2);

                    if (bsonDocs.Count > 0)
                    {
                            foreach (var b in bsonDocs)
                            {
                                JSON_Export_JD(b, "fork_jobs_" + tmp_i.ToString());
                                tmp_i++;
                            }
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
                    Console.WriteLine(chunk_counter.ToString() + " of " + db_chunks.Count.ToString() + " in " + elapsedTime + ". " + tmp_i.ToString() + " Jobs Saved.");
                }

            }
            else
            {
                Console.WriteLine("NO CHUNKS!!!");
            }
        }
    }
}
