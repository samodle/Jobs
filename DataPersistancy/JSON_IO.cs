using Analytics;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;


namespace DataPersistancy
{
    public static class JSON_IO
    {
        public static void CrystalBall_Changelog_Export(CrystalBallAnalysis exportObject, string FileName, string FileType = ".txt")
        {
            string jsonData = JsonConvert.SerializeObject(exportObject);
            string fileName = Globals.HTML.PATH_FORK_GLIDEPATH + FileName + FileType;
            FileStream fcreate = File.Open(fileName, FileMode.Create);
            using (StreamWriter writer = new StreamWriter(fcreate))
            {
                writer.Write(jsonData);
                writer.Close();
            }
        }

        public static CrystalBallAnalysis CrystalBall_Changelog_Import(string fileName)
        {
            CrystalBallAnalysis tmpData;
            string rawJSONstring = File.ReadAllText(Globals.HTML.PATH_FORK_GLIDEPATH + fileName);
            tmpData = JsonConvert.DeserializeObject<CrystalBallAnalysis>(rawJSONstring);
            return tmpData;
        }
    }
}
