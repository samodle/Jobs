using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EO.WebBrowser.DOM;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Analytics
{
    public class ONETReport
    {
        public List<Occupation> MasterOccupationList = new List<Occupation>();
        public List<Attribute> MasterSkillList = new List<Attribute>();
        public List<Attribute> MasterKnowledgeList = new List<Attribute>();
        public List<Attribute> MasterAbilityList = new List<Attribute>();

        public List<OccupationEdge> OccupationEdges = new List<OccupationEdge>();

        

        #region Graph Building
        public void saveEdgesToDB()
        {
            MongoClient dbClient = new MongoClient("mongodb://forkAdmin:ForkAdmin123@localhost:27017");
            var database = dbClient.GetDatabase("graphs");
            var collection = database.GetCollection<BsonDocument>("edges_professions");

            for (int i = 0; i < MasterOccupationList.Count - 1; i++)
            {
                var EmpInfoArray = new List<BsonDocument>();

                for (int j = i + 1; j < MasterOccupationList.Count; j++)
                {
                   var newEdge = MasterOccupationList[i].getEdge(MasterOccupationList[j]);
                   EmpInfoArray.Add(newEdge.getSimpleEdge().ToBsonDocument());
                }

                try
                {
                    collection.InsertMany(EmpInfoArray);
                }
                catch (Exception e)
                {
                    int ie = 0;
                }



            }
        }


        public void findTopAdjacencies()
        {
            MongoClient dbClient = new MongoClient("mongodb://forkAdmin:ForkAdmin123@localhost:27017");
            var database = dbClient.GetDatabase("graphs");
            var collection = database.GetCollection<BsonDocument>("edges_professions");

            foreach (Occupation o in MasterOccupationList)
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
                foreach(var a in targetAdjacenciesA)
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
            }
        }

        public void setOccupationEdges(int numOccupations)
        {
            int startIndex = 0;
            int stopIndex = Math.Min(startIndex + numOccupations, MasterOccupationList.Count);


            for(int i = startIndex; i < stopIndex - 1; i++)
            {
                for(int j = i + 1; j < stopIndex; j++)
                {
                    OccupationEdges.Add(MasterOccupationList[i].getEdge(MasterOccupationList[j]));
                }
            }
        }
        #endregion
    }
}
