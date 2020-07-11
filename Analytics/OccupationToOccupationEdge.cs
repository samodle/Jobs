using Microsoft.Office.Interop.Excel;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Analytics
{
    [BsonIgnoreExtraElements]
    public class SimpleOccupationEdge
    {
        public string OccupationAName { get; set; }
        public string OccupationBName { get; set; }
        public List<Tuple<Constants.AttributeType, double>> Distances { get; set; }

        public SimpleOccupationEdge(string nameA, string nameB, List<Tuple<Constants.AttributeType, double>> distances)
        {
            this.OccupationAName = nameA;
            this.OccupationBName = nameB;
            this.Distances = distances;
        }


        public double getDistance(Constants.AttributeType type = Constants.AttributeType.Net)
        {
            var targetTuple = Distances.First(a => a.Item1 == type);
            return targetTuple.Item2;
        }

        public override string ToString()
        {
            return OccupationAName + " - " + OccupationBName + ": " + getDistance();
        }
    }

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

        public SimpleOccupationEdge getSimpleEdge()
        {
            return new SimpleOccupationEdge(this.OccupationAName, this.OccupationBName, this.Distances);
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
