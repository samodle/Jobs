using Analytics;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using Windows_Desktop;

namespace DataPersistancy
{
    public static class JSON_IO
    {
        public static void JSON_Export_OccupationList(List<Occupation> exportObject, string FileName, string FileType = ".txt")
        {
            string jsonData = JsonConvert.SerializeObject(exportObject);
            string fileName = Publics.FILEPATHS.PATH_FORK_JSON + FileName + FileType;
            FileStream fcreate = File.Open(fileName, FileMode.Create);
            using (StreamWriter writer = new StreamWriter(fcreate))
            {
                writer.Write(jsonData);
                writer.Close();
            }
        }

     public static List<Occupation> JSON_Import_OccupationList(string fileName)
        {
            List<Occupation> tmpData;
            string rawJSONstring = File.ReadAllText(Publics.FILEPATHS.PATH_FORK_JSON + fileName);
            tmpData = JsonConvert.DeserializeObject<List<Occupation>>(rawJSONstring);
            return tmpData;
        }
    }
}
