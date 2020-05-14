using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Windows_Desktop
{
    public class Import_RawData
    {
        public void importAllRawData()
        {
            List<string> allCSVs;
            int fileIncrementer;
            object[,] tmpData;
            allCSVs = getAllCSVinFolder(Globals.HTML.PATH_FORK_RAWDATA);

            for (fileIncrementer = 0; fileIncrementer < allCSVs.Count; fileIncrementer++)
            {
                //1) get the object
                // tmpData = getObjectArrFromCSV(allCSVs[fileIncrementer]);
                //2 ...

            }
        }

        public List<string> getAllCSVinFolder(string FolderPath)
        {
            DirectoryInfo d = new DirectoryInfo(FolderPath);//Assuming Test is your Folder
            FileInfo[] Files = d.GetFiles("*.csv"); //Getting Text files
            List<string> tmpFileNames = new List<string>();// = "";
            foreach (FileInfo file in Files)
            {
                tmpFileNames.Add(file.Name);
            }

            return tmpFileNames;
        }

        /* private static object[,] getObjectArrFromCSV(string FilePath)
         {
         }*/




    }
}
