using System;
using System.Collections.Generic;
using System.Text;
//using Windows.Media.Streaming.Adaptive;

namespace Analytics
{
    public static class Constants
    {

        //Adjacency Constance
        public const double MAX_DISTANCE = 1.0;
        public const double MIN_DISTANCE = 0.0;
        public const double INVALID_DISTANCE = 10000.0;
        public const double IMPORTANCE_MATCH_THRESHOLD = 0.5;
        public const double LEVEL_MATCH_THRESHOLD = 0.5;
        public const double LEVEL_OVER_IMPORTANCE_FACTOR = 2;
        //level steps? (ie threshold at which level distance becomes greater)
        //importance steps?

        public enum WageSnapshotType
        {
            Hourly = 0,
            Annual = 1
        }

        public enum JobZone
        {
            One = 1,
            Two = 2,
            Three = 3,
            Four = 4, 
            Five = 5
        }

        public enum AttributeType
        {
            Skill = 0,
            Ability = 1,
            Knowledge = 2,
            Net = 3,
            Word2VecWIKI = 4,
            Word2VecFORK = 5,
            TFIDF = 6,
            Word2VecGOOGNEWS = 7
        }

        public static string getStringForAttributeType(AttributeType type)
        {
            switch (type)
            {
                case AttributeType.Ability:
                    return "Ability";
                case AttributeType.Knowledge:
                    return "Knowledge";
                case AttributeType.Skill:
                    return "Skill";
                case AttributeType.Net:
                    return "Net";
                default:
                    return "Unknown";
            }
        }

        public static AttributeType GetAttributeTypeFromString(string s)
        {
            switch (s)
            {
                case "Ability":
                    return AttributeType.Ability;
                case "Knowledge":
                    return AttributeType.Knowledge;
                case "Skill":
                    return AttributeType.Skill;
                default:
                    return AttributeType.Skill;
            }
        }
    }
}
