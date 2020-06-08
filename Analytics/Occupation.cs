using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Analytics
{
    public class Occupation //: IEquatable<Occupation>
    {
        public string Name { get; set; }
        public string SOCCode { get; set; }
        public string Description { get; set; }
        public List<string> AlternateNames { get; set; }
        public List<JobAttribute> Skills { get; set; } = new List<JobAttribute>();
        public List<JobAttribute> Knowledge { get; set; } = new List<JobAttribute>();
        public List<JobAttribute> Abilities { get; set; } = new List<JobAttribute>();
        public Constants.JobZone Zone { get; set; }

        public Occupation(string name, string socCode, string descriptions)
        {
            this.Name = name;
            this.SOCCode = socCode;
            this.Description = descriptions;
        }

        public override string ToString()
        {
            return Name + ", Skills:" + Skills.Count + ", Knowledge: " + Knowledge.Count + ", Abilities:" + Abilities.Count;
        }

        public List<Tuple<Constants.AttributeType, double>> calculateSimilarity(Occupation other, List<Constants.AttributeType> typeList)
        {
            // List<Constants.AttributeType> aTypeList = new List<Constants.AttributeType>() { Constants.AttributeType.Ability, Constants.AttributeType.Knowledge, Constants.AttributeType.Skill };
            var retList = new List<Tuple<Constants.AttributeType, double>>();
            double netDist = 0;

            foreach(Constants.AttributeType a in typeList)
            {
                var rawAttributeMatrix = getAttributeSimilarityMatrix(other, a);
                double newDist = rawAttributeMatrix.Where(c => c.Distance > Constants.INVALID_DISTANCE).Sum(c => c.Distance);
                netDist += newDist;
                retList.Add(new Tuple<Constants.AttributeType, double>(a, newDist));
            }

            retList.Add(new Tuple<Constants.AttributeType, double>(Constants.AttributeType.Net, netDist));
            return retList;
        }


        public List<OccupationAttributeSimilarityMatrixItem> getAttributeSimilarityMatrix(Occupation other, Constants.AttributeType type)
        {
            var retList = new List<OccupationAttributeSimilarityMatrixItem>();
            List<JobAttribute> listA = getAttributesByType(type);
            List<JobAttribute> listB = other.getAttributesByType(type);

            List<JobAttribute> inA_notB = listA.Where(w => !listB.Contains(w)).ToList();
            List<JobAttribute> inB_notA = listB.Where(w => !listA.Contains(w)).ToList();

            //add attributes shared by both occupations
            foreach (JobAttribute a in listA)
            {
                if (listB.Contains(a))
                {
                    JobAttribute b = listB.Single(s => s.Equals(a));
                    double dist = a.calculateSimilarity(b);
                    retList.Add(new OccupationAttributeSimilarityMatrixItem(type, this.Name, other.Name, a.Name, dist));
                }
            }
            //add attributes shared by only one of the occupations
            foreach(JobAttribute a in inA_notB)
            {
                retList.Add(new OccupationAttributeSimilarityMatrixItem(type, this.Name, "", a.Name, a.getDistance()));
            }
            foreach (JobAttribute b in inB_notA)
            {
                retList.Add(new OccupationAttributeSimilarityMatrixItem(type, "", other.Name, b.Name, b.getDistance()));
            }

            return retList;
        }

        public List<JobAttribute> getAttributesByType(Constants.AttributeType type)
        {
            switch (type)
            {
                case Constants.AttributeType.Ability:
                    return Abilities;
                case Constants.AttributeType.Knowledge:
                    return Knowledge;
                case Constants.AttributeType.Skill:
                    return Skills;
                default:
                    return new List<JobAttribute>();
            }
        }

        /*   public bool Equals(Occupation other)
           {
               if (other.SOCCode.Equals(SOCCode))
               {
                   return true;
               }
               else
               {
                   return false;
               }
           }*/
    }
}
