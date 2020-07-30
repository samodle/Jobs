using System;
using System.Collections.Generic;
using System.Linq;
using Analytics;
using DataPersistancy;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using static Analytics.Constants;
using Attribute = Analytics.Attribute;

namespace Raw_Job_Processing
{
    class Program
    {


        static void Main(string[] args)
        {
            Console.WriteLine("Connecting To Database...");

            removeDupes();

            //PopulateAllProfessionNearestNeighbors(20);



            Console.WriteLine("Shutting Down...");
        }


        //
        private static void PopulateAllProfessionNearestNeighbors(int n)
        {
            MongoClient dbClient = new MongoClient("mongodb://forkAdmin:ForkAdmin123@localhost:27017");
            IMongoDatabase database = dbClient.GetDatabase("graphs");

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
                    if (foo == AttributeType.Word2VecWIKI || foo == AttributeType.Skill || foo == AttributeType.Net || foo == AttributeType.Knowledge || foo == AttributeType.Ability) {
                        List<SimpleOccupationEdge> SortedAdjacencyList = MasterAdjacencyList.OrderBy(oy => oy.getDistance(foo)).ToList();
                        var newList = SortedAdjacencyList.Take(n).Select(c => c.getOtherName(o.Name)).ToList();

                        newNeighborList.NearestNeighbors.Add(new Tuple<AttributeType, List<string>>(foo, newList));
                    }
                }

                destination_collection.InsertOne(newNeighborList.ToBsonDocument());

            }
        }

        //ETL the profession info, add top adjacencies to profession node (this part is bad and needs to go)
        private static void ONET_ETL_Profession()
        {
            var ForkReport = new ONETReport();
            ForkReport.MasterOccupationList = JSON_IO.Import_OccupationList(Helper.Publics.FILENAMES.OCCUPATIONS + ".txt");
            ForkReport.MasterSkillList = JSON_IO.Import_AttributeList(Helper.Publics.FILENAMES.SKILLS + ".txt");
            ForkReport.MasterAbilityList = JSON_IO.Import_AttributeList(Helper.Publics.FILENAMES.ABILITIES + ".txt");
            ForkReport.MasterKnowledgeList = JSON_IO.Import_AttributeList(Helper.Publics.FILENAMES.KNOWLEDGE + ".txt");

            MongoClient dbClient = new MongoClient("mongodb://forkAdmin:ForkAdmin123@localhost:27017");
            IMongoDatabase database = dbClient.GetDatabase("graphs");

            var profession_collection = database.GetCollection<BsonDocument>("node_profession");

            //add those top 5 occupations



            var collection = database.GetCollection<BsonDocument>("edges_professions");

            foreach (Occupation o in ForkReport.MasterOccupationList)
            {
                //   var occupationFilter = Builders<BsonDocument>.Filter.ElemMatch<BsonValue>(
                //     "OccupationAName", 
                //    
                //   );
                var occupationFilterA = Builders<BsonDocument>.Filter.Eq("OccupationAName", o.Name);
                var targetAdjacenciesA = collection.Find(occupationFilterA).ToList();

                var occupationFilterB = Builders<BsonDocument>.Filter.Eq("OccupationBName", o.Name);
                var targetAdjacenciesB = collection.Find(occupationFilterB).ToList();

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

                //sort adjacencies
                List<SimpleOccupationEdge> SortedAdjacencyList = MasterAdjacencyList.OrderBy(oy => oy.getDistance()).ToList();


                //save top adjacencies
                //REMOVING - NOT HOW WE ARE MANAGING THIS INFO 
                //o.TenMostAdjacent = SortedAdjacencyList.Take(10).Select(c => c.getOtherName(o.Name)).ToList();
            }



                //save to DB
                var EmpInfoArray = new List<BsonDocument>();
            foreach (Occupation j in ForkReport.MasterOccupationList)
            {
                EmpInfoArray.Add(j.ToBsonDocument());
            }
            profession_collection.InsertMany(EmpInfoArray);
    

        }



        //ETL the ONET data from origial ONET dowloadable db format to MongoDB
        private static void ONET_ETL_NoProfession()
        {
            var ForkReport = new ONETReport();
            ForkReport.MasterOccupationList = JSON_IO.Import_OccupationList(Helper.Publics.FILENAMES.OCCUPATIONS + ".txt");
            ForkReport.MasterSkillList = JSON_IO.Import_AttributeList(Helper.Publics.FILENAMES.SKILLS + ".txt");
            ForkReport.MasterAbilityList = JSON_IO.Import_AttributeList(Helper.Publics.FILENAMES.ABILITIES + ".txt");
            ForkReport.MasterKnowledgeList = JSON_IO.Import_AttributeList(Helper.Publics.FILENAMES.KNOWLEDGE + ".txt");

            MongoClient dbClient = new MongoClient("mongodb://forkAdmin:ForkAdmin123@localhost:27017");
            IMongoDatabase database = dbClient.GetDatabase("graphs");
            var skill_collection = database.GetCollection<BsonDocument>("node_skill");
            var ability_collection = database.GetCollection<BsonDocument>("node_ability");
           // var profession_collection = database.GetCollection<BsonDocument>("node_profession");
            var knowledge_collection = database.GetCollection<BsonDocument>("node_knowledge");

            //add those top 5 occupations



            //save to DB
            var EmpInfoArray = new List<BsonDocument>();

            foreach (Attribute j in ForkReport.MasterSkillList)
            {
                EmpInfoArray.Add(j.ToBsonDocument());
            }
            skill_collection.InsertMany(EmpInfoArray);

            EmpInfoArray = new List<BsonDocument>();

            foreach (Attribute j in ForkReport.MasterKnowledgeList)
            {
                EmpInfoArray.Add(j.ToBsonDocument());
            }
            knowledge_collection.InsertMany(EmpInfoArray);

            EmpInfoArray = new List<BsonDocument>();

            foreach (Attribute j in ForkReport.MasterAbilityList)
            {
                EmpInfoArray.Add(j.ToBsonDocument());
            }
            ability_collection.InsertMany(EmpInfoArray);

            EmpInfoArray = new List<BsonDocument>();

            /*foreach (Occupation j in ForkReport.MasterOccupationList)
            {
                EmpInfoArray.Add(j.ToBsonDocument());
            }
            profession_collection.InsertMany(EmpInfoArray);
            */

        }


        //Job Description Data: Removes Duplicates and Cleans
        private static async void removeDupes()
        {
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            MongoClient dbClient = new MongoClient("mongodb://forkAdmin:ForkAdmin123@localhost:27017");
            IMongoDatabase database = dbClient.GetDatabase("jobs");
            var raw_collection = database.GetCollection<BsonDocument>("job_descriptions");
            //var clean_collection = database.GetCollection<BsonDocument>("jobs_cleaned");

            //var filter = new BsonDocument();
            var unique_companies = raw_collection.Distinct<string>("company", FilterDefinition<BsonDocument>.Empty).ToList();

          //  var unique_companies = unique_cursors.ToList();

            int complete_counter = 0;

            // Get the elapsed time as a TimeSpan value.
            TimeSpan ts = watch.Elapsed;

            // Format and display the TimeSpan value.

            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
            Console.WriteLine($"Setup Complete: " + elapsedTime);

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
                                // delete the first one, save the second
                                jdsToDelete.Add(rawJobs[i].ID);
                                jdsToUpdate.Add(rawJobs[j].ID);

                                // get any other data we need
                                rawJobs[j].search_terms = rawJobs[j].search_terms.Union(rawJobs[i].search_terms).ToList();
                                rawJobs[j].dates_found = rawJobs[j].dates_found.Union(rawJobs[i].dates_found).ToList();

                                break; //break out of this loop
                            }
                        }
                    }

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
                    await raw_collection.DeleteManyAsync(filter3);
                }

                //update the console
                complete_counter++;
                // Get the elapsed time as a TimeSpan value.
                ts = watch.Elapsed;

                // Format and display the TimeSpan value.
                elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
                Console.WriteLine(complete_counter.ToString() + "/" + unique_companies.Count.ToString() + " " + company + " Complete in " + elapsedTime);
            }
        }
    }
}
