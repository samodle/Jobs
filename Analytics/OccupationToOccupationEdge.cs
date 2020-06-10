using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Analytics
{
    public class OccupationEdge
    {
        public string OccupationAName { get; set; }
        public string OccupationBName { get; set; }
        public List<Tuple<Constants.AttributeType, List<OccupationAttributeEdge>>> AttributeDetail { get; set; }
        public List<Tuple<Constants.AttributeType, double>> Distances { get; set; }

        public OccupationEdge(string nameA, string nameB, List<Tuple<Constants.AttributeType, List<OccupationAttributeEdge>>> attributeDetail, List<Tuple<Constants.AttributeType, double>> distances)
        {
            this.OccupationAName = nameA;
            this.OccupationBName = nameB;
            this.AttributeDetail = attributeDetail;
            this.Distances = distances;
        }

        public double getDistanceByAttribute(Constants.AttributeType type)
        {
            for(int i = 0; i < Distances.Count; i++)
            {
                if (Distances[i].Item1.Equals(type))
                {
                    return Distances[i].Item2;
                }
            }
            return Constants.INVALID_DISTANCE;
        }

        public override string ToString()
        {
            return OccupationAName + " - " + OccupationBName;
        }
    }
}
