using System;
using System.Collections.Generic;
using System.Linq;
using Analytics;
using DataPersistancy;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using static Analytics.Constants;

namespace Raw_Job_Processing
{
    class Program
    {


        enum RunModes { removeDuplicates, documentExport, populateProfessionNNs, jobKPIs, jobReports }

        static void Main(string[] args)
        {
            var currentMode = RunModes.jobKPIs;

            Console.WriteLine("Launching Job Database Admin Tool. Mode: {0}", Enum.GetName(typeof(RunModes), currentMode));

            switch (currentMode)
            {
                case RunModes.removeDuplicates:
                    removeDupes();
                    break;

                case RunModes.documentExport:
                    MongoExport.ExportEachDocument();
                    break;

                case RunModes.populateProfessionNNs:
                    PopulateAllProfessionNearestNeighbors(20);
                    break;

                case RunModes.jobKPIs:
                    JobAnalysis.AnalyzeJobs();
                    break;

                case RunModes.jobReports:

                    break;
            }


            Console.WriteLine(" - - - - - -");
            Console.WriteLine("Execution Complete");
        }


        //
        private static void PopulateAllProfessionNearestNeighbors(int n)
        {
            MongoClient dbClient = new MongoClient(MongoStrings.CONNECTION);
            IMongoDatabase database = dbClient.GetDatabase(MongoStrings.GRAPH_DB);

            var edge_collection = database.GetCollection<BsonDocument>("edges_professions");
            var destination_collection = database.GetCollection<BsonDocument>("nearest_neighbors_profession");

            var ForkReport = new ONETReport();
            ForkReport.MasterOccupationList = JSON_IO.Import_OccupationList(Helper.Publics.FILENAMES.OCCUPATIONS + ".txt");

            Console.WriteLine("Iterating Through Occupations...");

            foreach (Occupation o in ForkReport.MasterOccupationList)
            {
                Console.WriteLine("                                ..." + o.Name);
                var occupationFilterA = Builders<BsonDocument>.Filter.Eq("OccupationAName", o.Name);
                var targetAdjacenciesA = edge_collection.Find(occupationFilterA).ToList();

                var occupationFilterB = Builders<BsonDocument>.Filter.Eq("OccupationBName", o.Name);
                var targetAdjacenciesB = edge_collection.Find(occupationFilterB).ToList();

                var MasterAdjacencyList = new List<SimpleOccupationEdge>();

                //retrieve adjacencies from database
                foreach (var a in targetAdjacenciesA)
                {
                    MasterAdjacencyList.Add(BsonSerializer.Deserialize<SimpleOccupationEdge>(a));
                }
                foreach (var b in targetAdjacenciesB)
                {
                    MasterAdjacencyList.Add(BsonSerializer.Deserialize<SimpleOccupationEdge>(b));
                }


                ProfessionNearestNeighbors newNeighborList = new ProfessionNearestNeighbors(o.Name);

                //sort adjacencies
                foreach (AttributeType foo in Enum.GetValues(typeof(AttributeType)))
                {
                    if (foo == AttributeType.Word2VecWIKI || foo == AttributeType.Skill || foo == AttributeType.Net || foo == AttributeType.Knowledge || foo == AttributeType.Ability)
                    {
                        List<SimpleOccupationEdge> SortedAdjacencyList = MasterAdjacencyList.OrderBy(oy => oy.getDistance(foo)).ToList();
                        var newList = SortedAdjacencyList.Take(n).Select(c => c.getOtherName(o.Name)).ToList();

                        newNeighborList.NearestNeighbors.Add(new Tuple<AttributeType, List<string>>(foo, newList));
                    }
                }

                destination_collection.InsertOne(newNeighborList.ToBsonDocument());

            }
        }


        //Job Description Data: Removes Duplicates and Cleans
        private static void removeDupes()
        {
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            MongoClient dbClient = new MongoClient(MongoStrings.CONNECTION);
            IMongoDatabase database = dbClient.GetDatabase(MongoStrings.JOB_DB);
            var raw_collection = database.GetCollection<BsonDocument>(MongoStrings.JOB_COLLECTION);

            var unique_companies = raw_collection.Distinct<string>("company", FilterDefinition<BsonDocument>.Empty).ToList();

            int complete_counter = 0;
            int delete_counter = 0;

            // Get the elapsed time as a TimeSpan value.
            TimeSpan ts = watch.Elapsed;

            // Format and display the TimeSpan value.
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
            Console.WriteLine($"Setup Complete: {elapsedTime}");

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
                        rawJobs.Add(DataCleaning.CleanJobDescription(b));
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

                // Get the elapsed time as a TimeSpan value.
                ts = watch.Elapsed;

                // Format and display the TimeSpan value.
                elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
                Console.WriteLine(Math.Round(complete_counter * 100.0 / unique_companies.Count, 1).ToString() + "%, " + delete_counter.ToString() + " Deleted, " + complete_counter.ToString() + "/" + unique_companies.Count.ToString() + " " + company + " Complete in " + elapsedTime);
            }
        }
    }
}
