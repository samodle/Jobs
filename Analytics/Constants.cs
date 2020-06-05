using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
