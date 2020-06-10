using System;
using System.Collections.Generic;
using System.Text;

namespace Analytics
{
    public class OccupationAttributeEdge : IComparable<OccupationAttributeEdge>
    {
        public Constants.AttributeType Type;
        public string OccupationA;
        public string OccupationB;
        public string Attribute;
        public double Distance;
        public bool isShared;
        
        public OccupationAttributeEdge(Constants.AttributeType type, string occupationA, string occupationB, string attribute, double distance)
        {
            this.Type = type;
            this.OccupationA = occupationA;
            this.OccupationB = occupationB;
            this.Attribute = attribute;
            this.Distance = distance;

            if(occupationA.Length < 1 || occupationB.Length < 1)
            {
                isShared = false;
            }
            else
            {
                isShared = true;
            }
        }

        public int CompareTo(OccupationAttributeEdge other)
        {
            return this.Distance.CompareTo(other.Distance);
        }

        public override string ToString()
        {
            return OccupationA + " - " + OccupationB + ", " + Attribute + ": " + Distance;
        }
    }
}
