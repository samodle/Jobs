using System;
using System.Collections.Generic;
using System.Text;

namespace Analytics
{
    public class ONETReport
    {
        public List<Occupation> MasterOccupationList = new List<Occupation>();
        public List<Attribute> MasterSkillList = new List<Attribute>();
        public List<Attribute> MasterKnowledgeList = new List<Attribute>();
        public List<Attribute> MasterAbilityList = new List<Attribute>();

        public List<OccupationEdge> OccupationEdges = new List<OccupationEdge>();

        public void setOccupationEdges(int numOccupations)
        {
            int startIndex = 0;
            int stopIndex = startIndex + numOccupations;


            for(int i = startIndex; i < stopIndex - 1; i++)
            {
                for(int j = i + 1; j < MasterOccupationList.Count; j++)
                {
                    OccupationEdges.Add(MasterOccupationList[i].getEdge(MasterOccupationList[j]));
                }
            }
        }
    }
}
