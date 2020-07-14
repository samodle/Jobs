using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Helper
{
    class Initialization_Support
    {
        public static void verifyFolderStructure()
        {
            createFolder(Publics.FILEPATHS.PATH_FORK);
            createFolder(Publics.FILEPATHS.PATH_FORK_JSON);
            createFolder(Publics.FILEPATHS.PATH_FORK_HTML);
        }
        private static void createFolder(string folderName)
        {
            if ((!Directory.Exists(folderName)))
            {
                Directory.CreateDirectory(folderName);
            }
        }
    }

    static class Publics
    {
        public static class FILEPATHS
        {
            public const string PATH_FORK = "C:\\Users\\Public\\Public_fork\\";
            public const string PATH_FORK_JSON = PATH_FORK + "Common\\";
            public const string PATH_FORK_HTML = PATH_FORK + "html\\";
        }
        public static class FILENAMES
        {
            public const string OCCUPATIONS = "ForkOccupationList";
            public const string SKILLS = "ForkSkillList";
            public const string ABILITIES = "ForkAbilityList";
            public const string KNOWLEDGE = "ForkKnowledgeList";
        }
    }

    static class GlobalFcns
    {
        public static string onlyDigits(string s)
        {
            string resultString = null;

            Regex regexObj = new Regex(@"[^\d]");
            resultString = regexObj.Replace(s, "");
            return resultString;

        }
    }
}
