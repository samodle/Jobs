using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Oden.Mongo;

namespace Raw_Job_Processing
{
    class Program
    {
        enum RunModes { removeDuplicates, documentExport, populateProfessionNNs, jobKPIs, jobReports }

        static void Main(string[] args)
        {
            var currentModes = new List<RunModes> { RunModes.removeDuplicates, RunModes.jobKPIs };
            //            var currentModes = new List<RunModes> { RunModes.jobReports};

            foreach (var currentMode in currentModes)
            {
                Console.WriteLine("Launching Job Genome Express v2.3.1. Mode: {0}", Enum.GetName(typeof(RunModes), currentMode));

                switch (currentMode)
                {
                    case RunModes.removeDuplicates:
                        RemoveDupes.removeDupes();
                        break;

                    case RunModes.documentExport:
                        MongoExport.ExportEachDocument();
                        break;

                    case RunModes.populateProfessionNNs:
                        //PopulateAllProfessionNearestNeighbors(20); //not fully implemented post oden migration
                        break;

                    case RunModes.jobKPIs:
                        JobAnalysis.AnalyzeJobs();
                        break;

                    case RunModes.jobReports:
                        JobReportScripts.WeeklyReport();
                        break;
                }
            }



            Console.WriteLine(" - - - - - -");
            Console.WriteLine("Execution Complete");
        }


        //
        private static void PopulateAllProfessionNearestNeighbors(int n)
        {
            MongoClient dbClient = new MongoClient(Connection.LOCAL);
            IMongoDatabase database = dbClient.GetDatabase(DB.GRAPH);

            var edge_collection = database.GetCollection<BsonDocument>("edges_professions");
            var destination_collection = database.GetCollection<BsonDocument>("nearest_neighbors_profession");

           // var ForkReport = new ONETReport();
            //  ForkReport.MasterOccupationList = JSON_IO.Import_OccupationList(Helper.Publics.FILENAMES.OCCUPATIONS + ".txt");
            throw new Exception("Not Implemented");

            /*
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
            */
        }

    }
}
