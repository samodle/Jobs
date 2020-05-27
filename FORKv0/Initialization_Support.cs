using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Windows_Desktop
{
    class Initialization_Support
    {
        public static void verifyFolderStructure()
        {
         /*   createFolder(Globals.HTML.PATH_FORK);
            createFolder(Globals.HTML.PATH_FORK_GLIDEPATH);
            createFolder(Globals.HTML.PATH_FORK_RAWDATA);
            createFolder(Globals.HTML.SERVER_FOLDER_PATH);*/
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
        public static System.Windows.Input.MouseButtonEventArgs f { get; set; }
        public static EventArgs g { get; set; }

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
