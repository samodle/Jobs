using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Oden.Mongo;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Raw_Job_Processing
{
    public class RemoveDupes
    {
        public static void removeDupes()
        {
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            MongoClient dbClient = new MongoClient(Connection.LOCAL);
            IMongoDatabase database = dbClient.GetDatabase(DB.JOB);
            var raw_collection = database.GetCollection<BsonDocument>(Collection.JOB);

            var unique_companies = raw_collection.Distinct<string>("company", FilterDefinition<BsonDocument>.Empty).ToList();

            int complete_counter = 0;
            int delete_counter = 0;

            Oden.ConsoleIO.printTimeStatus(watch.Elapsed, "Setup Complete: ");

            foreach (string company in unique_companies)
            {
                var jdsToDelete = new List<ObjectId>();
                var jdsToUpdate = new List<ObjectId>();

                var filter2 = Builders<BsonDocument>.Filter.Eq("company", company);
                var rawData = raw_collection.Find(filter2).ToList();

                if (rawData.Count > 1)
                {
                    List<RawJobDescription> rawJobs = new List<RawJobDescription>();

                    foreach (var a in rawData)
                    {
                        var b = BsonSerializer.Deserialize<RawJobDescription>(a);
                        rawJobs.Add(ClassRawJob.CleanJobDescription(b));
                    }

                    //for each job description, check if there are any duplicates
                    for (int i = 0; i < rawJobs.Count - 1; i++)
                    {
                        for (int j = i + 1; j < rawJobs.Count; j++)
                        {
                            if (rawJobs[i].Equals(rawJobs[j]))
                            {
                                // if i is newer/more recent, delete j so swap the items
                                if (DateTime.Compare(rawJobs[i].dates_found.Max(), rawJobs[j].dates_found.Max()) > 0)
                                {
                                    Console.WriteLine($"Swap {rawJobs[i].ToString()} // and // {rawJobs[j].ToString()}");
                                    //swap the two JDs
                                    var tmp = rawJobs[i];
                                    rawJobs[i] = rawJobs[j];
                                    rawJobs[j] = tmp;
                                }

                                // delete the first one, save the second
                                jdsToDelete.Add(rawJobs[i].ID);
                                jdsToUpdate.Add(rawJobs[j].ID);

                                // get any other data we need before we delete [i]
                                rawJobs[j].search_terms = rawJobs[j].search_terms.Union(rawJobs[i].search_terms).ToList();
                                rawJobs[j].dates_found = rawJobs[j].dates_found.Union(rawJobs[i].dates_found).ToList();

                                break; //break out of this loop because we've handled the [i] JD
                            }
                        }
                    }

                    if (jdsToDelete.Count > 0)
                    {
                        delete_counter += jdsToDelete.Count;

                        jdsToUpdate = jdsToUpdate.Distinct().ToList();
                        jdsToUpdate = jdsToUpdate.Except(jdsToDelete).ToList();

                        //make updates
                        foreach (var jb in jdsToUpdate)
                        {
                            var job = rawJobs.Single(s => s.ID == jb);

                            var filterx = Builders<BsonDocument>.Filter.Eq("_id", job.ID);
                            var update = Builders<BsonDocument>.Update.Set("search_terms", job.search_terms)
                                        .Set("dates_found", job.dates_found);
                            var updateResult = raw_collection.UpdateOne(filterx, update);
                        }

                        //make deletions
                        var filter3 = Builders<BsonDocument>.Filter.In("_id", jdsToDelete);
                        raw_collection.DeleteMany(filter3);
                    }
                }

                //update the console
                complete_counter++;
                Oden.ConsoleIO.printTimeStatus(watch.Elapsed, Math.Round(complete_counter * 100.0 / unique_companies.Count, 1).ToString() + "%, " + delete_counter.ToString() + " Deleted, " + complete_counter.ToString() + "/" + unique_companies.Count.ToString() + " " + company + " Complete in ");
            }
        }
    }
}
