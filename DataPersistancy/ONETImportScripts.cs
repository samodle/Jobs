using System;
using LumenWorks.Framework.IO.Csv;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;
using Analytics;
using System.Windows.Media.TextFormatting;

namespace DataPersistancy
{
    public static class ONETImportScripts
    {
        public static List<Occupation> MasterOccupationList = new List<Occupation>();
        public static List<Skill> MasterSkillList = new List<Skill>();

        //Create the list of occupations
        public static void ONET_importRawOccupations()
        {
            var csvTable = new DataTable();
            using (var csvReader = new CsvReader(new StreamReader(System.IO.File.OpenRead(@"C:\Users\Sam\Desktop\Fork\db_24_3_excel\csv\OccupationData.csv")), true))
            {
                csvTable.Load(csvReader);
            }

            MasterOccupationList = new List<Occupation>();

            for (int i = 0; i < csvTable.Rows.Count; i++)
            {
                MasterOccupationList.Add(new Occupation (name: csvTable.Rows[i][1].ToString(), socCode: csvTable.Rows[i][0].ToString(), descriptions: csvTable.Rows[i][2].ToString() ));
            }

            JSON_IO.JSON_Export_OccupationList(MasterOccupationList, Windows_Desktop.Publics.FILENAMES.FILE_OCCUPATIONS);
        }


        //create skills, add them to the occupations
        public static void ONET_importOccupations()
        {
            MasterOccupationList = JSON_IO.JSON_Import_OccupationList(Windows_Desktop.Publics.FILENAMES.FILE_OCCUPATIONS + ".txt");
            foreach (Occupation o in MasterOccupationList)
            {
                o.Skills = new List<JobSkill>();
            }

            var csvTable = new DataTable();
            using (var csvReader = new CsvReader(new StreamReader(System.IO.File.OpenRead(@"C:\Users\Sam\Desktop\Fork\db_24_3_excel\csv\Skills.csv")), true))
            {
                csvTable.Load(csvReader);
            }

            for (int i = 0; i < csvTable.Rows.Count; i+=2)
            {
                int j = i + 1;
                const double blankIndicator = -1;
                AttributeLevel tmpLevel;
                AttributeImportance tmpImportance;
                if (9660 <= i && i <= 9728)
                {
                    tmpImportance = new AttributeImportance(Convert.ToDouble(value: csvTable.Rows[i][6].ToString()), n: blankIndicator, stdError: blankIndicator, lowerCI: blankIndicator, upperCI: blankIndicator, suppress: "N", date: Convert.ToDateTime(csvTable.Rows[i][13].ToString()), source: csvTable.Rows[i][14].ToString(), notRelevant: "N");
                    tmpLevel = new AttributeLevel(Convert.ToDouble(value: csvTable.Rows[j][6].ToString()), n: blankIndicator, stdError: blankIndicator, lowerCI: blankIndicator, upperCI: blankIndicator, suppress: "N", date: Convert.ToDateTime(csvTable.Rows[j][13].ToString()), source: csvTable.Rows[j][14].ToString());
                }
                else
                {
                    tmpImportance = new AttributeImportance(Convert.ToDouble(value: csvTable.Rows[i][6].ToString()), n: Convert.ToDouble(csvTable.Rows[i][7].ToString()), stdError: Convert.ToDouble(csvTable.Rows[i][8].ToString()), lowerCI: Convert.ToDouble(csvTable.Rows[i][9].ToString()), upperCI: Convert.ToDouble(csvTable.Rows[i][10].ToString()), suppress: csvTable.Rows[i][11].ToString(), date: Convert.ToDateTime(csvTable.Rows[i][13].ToString()), source: csvTable.Rows[i][14].ToString(), notRelevant: csvTable.Rows[i][12].ToString());             
                    tmpLevel = new AttributeLevel(Convert.ToDouble(value: csvTable.Rows[j][6].ToString()), n: Convert.ToDouble(csvTable.Rows[j][7].ToString()), stdError: Convert.ToDouble(csvTable.Rows[j][8].ToString()), lowerCI: Convert.ToDouble(csvTable.Rows[j][9].ToString()), upperCI: Convert.ToDouble(csvTable.Rows[j][10].ToString()), suppress: csvTable.Rows[j][11].ToString(), date: Convert.ToDateTime(csvTable.Rows[j][13].ToString()), source: csvTable.Rows[j][14].ToString());
                }

                string elementID = csvTable.Rows[i][2].ToString();
                string elementName = csvTable.Rows[i][3].ToString();
                string occupationID = csvTable.Rows[j][0].ToString();
                int occupationIndex = MasterOccupationList.FindIndex(p => p.SOCCode.Equals(occupationID));

                JobSkill tmpSkill = new JobSkill(elementName, elementID, tmpImportance, tmpLevel);
                MasterOccupationList[occupationIndex].Skills.Add(tmpSkill);

                int skillIndex = MasterSkillList.FindIndex(p => p.ElementID.Equals(elementID));
                if (skillIndex == -1)
                {
                    MasterSkillList.Add(new Skill(elementName, elementID, MasterOccupationList[occupationIndex].SOCCode));
                }
                else
                {
                    MasterSkillList[skillIndex].OccupationIDs.Add(MasterOccupationList[occupationIndex].SOCCode);
                }
            }

            JSON_IO.JSON_Export_OccupationList(MasterOccupationList, Windows_Desktop.Publics.FILENAMES.FILE_OCCUPATIONS + "2");

            JSON_IO.JSON_Export_SkillList(MasterSkillList, Windows_Desktop.Publics.FILENAMES.FILE_SKILLS + "");
        }
    }
}
