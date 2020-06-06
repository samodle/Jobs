using System;
using System.Collections.Generic;
using System.Text;
using Windows.Media.Streaming.Adaptive;

namespace Analytics
{
    public static class Constants
    {
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
            Skill,
            Ability,
            Knowledge
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
