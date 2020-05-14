using System;
using System.IO;

namespace Windows_Desktop
{
    class Initialization_Support
    {
        public static void verifyFolderStructure()
        {
            createFolder(Globals.HTML.PATH_FORK);
            createFolder(Globals.HTML.PATH_FORK_GLIDEPATH);
            createFolder(Globals.HTML.PATH_FORK_RAWDATA);
            createFolder(Globals.HTML.SERVER_FOLDER_PATH);
        }
        private static void createFolder(string folderName)
        {
            if ((!Directory.Exists(folderName)))
            {
                Directory.CreateDirectory(folderName);
            }
        }
    }
}
