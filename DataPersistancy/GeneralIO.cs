using System;
using System.Collections.Generic;
using System.IO;

namespace DataPersistancy
{
    public static class GeneralIO
    {
        public static List<string> getAllFileNamesinFolder(string FolderPath)//, string FileType = "txt")
        {
            var d = new DirectoryInfo(FolderPath);//Assuming Test is your Folder
            FileInfo[] Files = d.GetFiles();// searchPattern: FileType); //Getting Text files
            var tmpFileNames = new List<string>();
            foreach (FileInfo file in Files)
            {
                tmpFileNames.Add(file.Name);
            }
            return tmpFileNames;
        }
    }
}
