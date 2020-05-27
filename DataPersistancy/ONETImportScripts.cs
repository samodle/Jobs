using System;
using LumenWorks.Framework.IO.Csv;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;
using Analytics;

namespace DataPersistancy
{
    public static class ONETImportScripts
    {
        public static void ONET_importOccupations()
        {
            //C:\Users\Sam\Desktop\Fork\db_24_3_excel\csv\
            var csvTable = new DataTable();
            using (var csvReader = new CsvReader(new StreamReader(System.IO.File.OpenRead(@"C:\Users\Sam\Desktop\Fork\db_24_3_excel\csv\OccupationData.csv")), true))
            {
                csvTable.Load(csvReader);
            }


            List<Occupation> MasterOccupationList = new List<Occupation>();

            for (int i = 0; i < csvTable.Rows.Count; i++)
            {
                MasterOccupationList.Add(new Occupation (name: csvTable.Rows[i][0].ToString(), socCode: csvTable.Rows[i][1].ToString(), descriptions: csvTable.Rows[i][2].ToString() ));
            }


        }
    }
}
