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

            //removeDupes();

            PopulateAllProfessionNearestNeighbors(20);



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


            foreach (Occupation o in ForkReport.MasterOccupationList)
            {

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



            //save to DB


            var EmpInfoArray = new List<BsonDocument>();
            foreach (Occupation j in ForkReport.MasterOccupationList)
            {
                EmpInfoArray.Add(j.ToBsonDocument());
            }
            destination_collection.InsertMany(EmpInfoArray);
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
