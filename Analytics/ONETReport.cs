using System;
using System.Collections.Generic;
using System.Text;
using EO.WebBrowser.DOM;
using MongoDB.Bson;
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

        public void saveEdgesToDB()
        {
            MongoClient dbClient = new MongoClient("mongodb://forkAdmin:ForkAdmin123@localhost:27017");
            var database = dbClient.GetDatabase("adjacencies");
            var collection = database.GetCollection<BsonDocument>("edges_professions");

            for (int i = 0; i < MasterOccupationList.Count - 1; i++)
            {
                for (int j = i + 1; j < MasterOccupationList.Count; j++)
                {
                   var newEdge = MasterOccupationList[i].getEdge(MasterOccupationList[j]);

                    var document = newEdge.ToBsonDocument();
                    try
                    {
                        collection.InsertOne(document);
                    }
                    catch(Exception e)
                    {
                        int ie = 0;
                    }
                }
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
    }
}
