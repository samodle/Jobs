using System;
using LumenWorks.Framework.IO.Csv;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;
using Analytics;
//using System.Windows.Media.TextFormatting;
using Attribute = Analytics.Attribute;
using System.Drawing.Text;

namespace DataPersistancy
{
    public static class ONETImportScripts
    {
        public static List<Occupation> MasterOccupationList = new List<Occupation>();
        public static List<Attribute> MasterSkillList = new List<Attribute>();
        public static List<Attribute> MasterKnowledgeList = new List<Attribute>();
        public static List<Attribute> MasterAbilityList = new List<Attribute>();

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

            JSON_IO.JSON_Export_OccupationList(MasterOccupationList, Helper.Publics.FILENAMES.OCCUPATIONS);
        }


        //create skills, add them to the occupations
        public static void ONET_importOccupations()
        {
            MasterOccupationList = JSON_IO.Import_OccupationList(Helper.Publics.FILENAMES.OCCUPATIONS + ".txt");
            foreach (Occupation o in MasterOccupationList)
            {
                o.Skills = new List<JobAttribute>();
                o.Abilities = new List<JobAttribute>();
                o.Knowledge = new List<JobAttribute>();
                o.AlternateNames = new List<string>();
            }

            var csvTable = new DataTable();
            using (var csvReader = new CsvReader(new StreamReader(System.IO.File.OpenRead(@"C:\Users\Sam\Desktop\Fork\db_24_3_excel\csv\Skills.csv")), true))
            {
                csvTable.Load(csvReader);
            }

            for (int j = 0; j < csvTable.Rows.Count; j+=2)
            {
                int i = j + 1;
                const double blankIndicator = -1;
                AttributeImportance tmpLevel;
                AttributeLevel tmpImportance;
               // if (9660 <= i && i <= 9728)
               if(csvTable.Rows[j][7] is System.DBNull)
                {
                    tmpImportance = new AttributeLevel(Convert.ToDouble(value: csvTable.Rows[i][6].ToString()), n: blankIndicator, stdError: blankIndicator, lowerCI: blankIndicator, upperCI: blankIndicator, suppress: "N", date: Convert.ToDateTime(csvTable.Rows[i][13].ToString()), source: csvTable.Rows[i][14].ToString(), notRelevant: "N");
                    tmpLevel = new AttributeImportance(Convert.ToDouble(value: csvTable.Rows[j][6].ToString()), n: blankIndicator, stdError: blankIndicator, lowerCI: blankIndicator, upperCI: blankIndicator, suppress: "N", date: Convert.ToDateTime(csvTable.Rows[j][13].ToString()), source: csvTable.Rows[j][14].ToString());
                }
                else
                {
                    tmpImportance = new AttributeLevel(Convert.ToDouble(value: csvTable.Rows[i][6].ToString()), n: Convert.ToDouble(csvTable.Rows[i][7].ToString()), stdError: Convert.ToDouble(csvTable.Rows[i][8].ToString()), lowerCI: Convert.ToDouble(csvTable.Rows[i][9].ToString()), upperCI: Convert.ToDouble(csvTable.Rows[i][10].ToString()), suppress: csvTable.Rows[i][11].ToString(), date: Convert.ToDateTime(csvTable.Rows[i][13].ToString()), source: csvTable.Rows[i][14].ToString(), notRelevant: csvTable.Rows[i][12].ToString());             
                    tmpLevel = new AttributeImportance(Convert.ToDouble(value: csvTable.Rows[j][6].ToString()), n: Convert.ToDouble(csvTable.Rows[j][7].ToString()), stdError: Convert.ToDouble(csvTable.Rows[j][8].ToString()), lowerCI: Convert.ToDouble(csvTable.Rows[j][9].ToString()), upperCI: Convert.ToDouble(csvTable.Rows[j][10].ToString()), suppress: csvTable.Rows[j][11].ToString(), date: Convert.ToDateTime(csvTable.Rows[j][13].ToString()), source: csvTable.Rows[j][14].ToString());
                }

                string elementID = csvTable.Rows[i][2].ToString();
                string elementName = csvTable.Rows[i][3].ToString();
                string occupationID = csvTable.Rows[j][0].ToString();
                int occupationIndex = MasterOccupationList.FindIndex(p => p.SOCCode.Equals(occupationID));

                JobAttribute tmpSkill = new JobAttribute(elementName, elementID, tmpImportance, tmpLevel, Constants.AttributeType.Skill);
                MasterOccupationList[occupationIndex].Skills.Add(tmpSkill);

                int skillIndex = MasterSkillList.FindIndex(p => p.ElementID.Equals(elementID));
                if (skillIndex == -1)
                {
                    MasterSkillList.Add(new Attribute(elementName, elementID, MasterOccupationList[occupationIndex].SOCCode, Constants.AttributeType.Skill));
                }
                else
                {
                    MasterSkillList[skillIndex].OccupationIDs.Add(MasterOccupationList[occupationIndex].SOCCode);
                }
            }

            importAbility();
            importKnowledge();
            importJobZones();
            importAltOccNames();

            JSON_IO.JSON_Export_OccupationList(MasterOccupationList, Helper.Publics.FILENAMES.OCCUPATIONS);

            JSON_IO.Export_AttributeList(MasterSkillList, Helper.Publics.FILENAMES.SKILLS);
            JSON_IO.Export_AttributeList(MasterKnowledgeList, Helper.Publics.FILENAMES.KNOWLEDGE);
            JSON_IO.Export_AttributeList(MasterAbilityList, Helper.Publics.FILENAMES.ABILITIES);
        }
        
        private static void importAltOccNames() 
        {
            var csvTable = new DataTable();
            using (var csvReader = new CsvReader(new StreamReader(System.IO.File.OpenRead(@"C:\Users\Sam\Desktop\Fork\db_24_3_excel\csv\Alternate Titles.csv")), true))
            {
                csvTable.Load(csvReader);
            }

            for (int i = 0; i < csvTable.Rows.Count; i ++)
            {
                string occupationID = csvTable.Rows[i][0].ToString();
                int occupationIndex = MasterOccupationList.FindIndex(p => p.SOCCode.Equals(occupationID));
                if (!(csvTable.Rows[i][2] is System.DBNull))
                {
                    MasterOccupationList[occupationIndex].AlternateNames.Add(csvTable.Rows[i][2].ToString());
                }
                if (!(csvTable.Rows[i][3] is System.DBNull))
                {
                    MasterOccupationList[occupationIndex].AlternateNames.Add(csvTable.Rows[i][3].ToString());
                }

            }
        }

        private static void importJobZones()
        {
            var csvTable = new DataTable();
            using (var csvReader = new CsvReader(new StreamReader(System.IO.File.OpenRead(@"C:\Users\Sam\Desktop\Fork\db_24_3_excel\csv\Job Zones.csv")), true))
            {
                csvTable.Load(csvReader);
            }

            for (int i = 0; i < csvTable.Rows.Count; i++)
            {
                string occupationID = csvTable.Rows[i][0].ToString();
                int occupationIndex = MasterOccupationList.FindIndex(p => p.SOCCode.Equals(occupationID));
                if (!(csvTable.Rows[i][2] is System.DBNull))
                {
                    MasterOccupationList[occupationIndex].Zone = (Constants.JobZone)Convert.ToInt32(csvTable.Rows[i][2].ToString());
                }
            }
        }

        private static void importAbility()
        {
            var csvTable = new DataTable();
            using (var csvReader = new CsvReader(new StreamReader(System.IO.File.OpenRead(@"C:\Users\Sam\Desktop\Fork\db_24_3_excel\csv\Abilities.csv")), true))
            {
                csvTable.Load(csvReader);
            }

            for (int j = 0; j < csvTable.Rows.Count; j += 2)
            {
                int i = j + 1;
                const double blankIndicator = -1;
                AttributeImportance tmpLevel;
                AttributeLevel tmpImportance;

                if (csvTable.Rows[j][7] is System.DBNull)
                {
                    tmpImportance = new AttributeLevel(Convert.ToDouble(value: csvTable.Rows[i][6].ToString()), n: blankIndicator, stdError: blankIndicator, lowerCI: blankIndicator, upperCI: blankIndicator, suppress: "N", date: Convert.ToDateTime(csvTable.Rows[i][13].ToString()), source: csvTable.Rows[i][14].ToString(), notRelevant: "N");
                    tmpLevel = new AttributeImportance(Convert.ToDouble(value: csvTable.Rows[j][6].ToString()), n: blankIndicator, stdError: blankIndicator, lowerCI: blankIndicator, upperCI: blankIndicator, suppress: "N", date: Convert.ToDateTime(csvTable.Rows[j][13].ToString()), source: csvTable.Rows[j][14].ToString());
                }
                else
                {
                    tmpImportance = new AttributeLevel(Convert.ToDouble(value: csvTable.Rows[i][6].ToString()), n: Convert.ToDouble(csvTable.Rows[i][7].ToString()), stdError: Convert.ToDouble(csvTable.Rows[i][8].ToString()), lowerCI: Convert.ToDouble(csvTable.Rows[i][9].ToString()), upperCI: Convert.ToDouble(csvTable.Rows[i][10].ToString()), suppress: csvTable.Rows[i][11].ToString(), date: Convert.ToDateTime(csvTable.Rows[i][13].ToString()), source: csvTable.Rows[i][14].ToString(), notRelevant: csvTable.Rows[i][12].ToString());
                    tmpLevel = new AttributeImportance(Convert.ToDouble(value: csvTable.Rows[j][6].ToString()), n: Convert.ToDouble(csvTable.Rows[j][7].ToString()), stdError: Convert.ToDouble(csvTable.Rows[j][8].ToString()), lowerCI: Convert.ToDouble(csvTable.Rows[j][9].ToString()), upperCI: Convert.ToDouble(csvTable.Rows[j][10].ToString()), suppress: csvTable.Rows[j][11].ToString(), date: Convert.ToDateTime(csvTable.Rows[j][13].ToString()), source: csvTable.Rows[j][14].ToString());
                }

                string elementID = csvTable.Rows[i][2].ToString();
                string elementName = csvTable.Rows[i][3].ToString();
                string occupationID = csvTable.Rows[j][0].ToString();
                int occupationIndex = MasterOccupationList.FindIndex(p => p.SOCCode.Equals(occupationID));

                JobAttribute tmpAttribute = new JobAttribute(elementName, elementID, tmpImportance, tmpLevel, Constants.AttributeType.Ability);
                MasterOccupationList[occupationIndex].Abilities.Add(tmpAttribute);

                int attributeIndex = MasterAbilityList.FindIndex(p => p.ElementID.Equals(elementID));
                if (attributeIndex == -1)
                {
                    MasterAbilityList.Add(new Attribute(elementName, elementID, MasterOccupationList[occupationIndex].SOCCode, Constants.AttributeType.Ability));
                }
                else
                {
                    MasterAbilityList[attributeIndex].OccupationIDs.Add(MasterOccupationList[occupationIndex].SOCCode);
                }
            }
        }

        private static void importKnowledge()
        {
            var csvTable = new DataTable();
            using (var csvReader = new CsvReader(new StreamReader(System.IO.File.OpenRead(@"C:\Users\Sam\Desktop\Fork\db_24_3_excel\csv\Knowledge.csv")), true))
            {
                csvTable.Load(csvReader);
            }

            for (int j = 0; j < csvTable.Rows.Count; j += 2)
            {
                int i = j + 1;
                const double blankIndicator = -1;
                AttributeImportance tmpLevel;
                AttributeLevel tmpImportance;

                if (csvTable.Rows[j][7] is System.DBNull || csvTable.Rows[j][9] is System.DBNull || csvTable.Rows[i][9] is System.DBNull)
                {
                    tmpImportance = new AttributeLevel(Convert.ToDouble(value: csvTable.Rows[i][6].ToString()), n: blankIndicator, stdError: blankIndicator, lowerCI: blankIndicator, upperCI: blankIndicator, suppress: "N", date: Convert.ToDateTime(csvTable.Rows[i][13].ToString()), source: csvTable.Rows[i][14].ToString(), notRelevant: "N");
                    tmpLevel = new AttributeImportance(Convert.ToDouble(value: csvTable.Rows[j][6].ToString()), n: blankIndicator, stdError: blankIndicator, lowerCI: blankIndicator, upperCI: blankIndicator, suppress: "N", date: Convert.ToDateTime(csvTable.Rows[j][13].ToString()), source: csvTable.Rows[j][14].ToString());
                }
                else
                {
                    tmpImportance = new AttributeLevel(Convert.ToDouble(value: csvTable.Rows[i][6].ToString()), n: Convert.ToDouble(csvTable.Rows[i][7].ToString()), stdError: Convert.ToDouble(csvTable.Rows[i][8].ToString()), lowerCI: Convert.ToDouble(csvTable.Rows[i][9].ToString()), upperCI: Convert.ToDouble(csvTable.Rows[i][10].ToString()), suppress: csvTable.Rows[i][11].ToString(), date: Convert.ToDateTime(csvTable.Rows[i][13].ToString()), source: csvTable.Rows[i][14].ToString(), notRelevant: csvTable.Rows[i][12].ToString());
                    tmpLevel = new AttributeImportance(Convert.ToDouble(value: csvTable.Rows[j][6].ToString()), n: Convert.ToDouble(csvTable.Rows[j][7].ToString()), stdError: Convert.ToDouble(csvTable.Rows[j][8].ToString()), lowerCI: Convert.ToDouble(csvTable.Rows[j][9].ToString()), upperCI: Convert.ToDouble(csvTable.Rows[j][10].ToString()), suppress: csvTable.Rows[j][11].ToString(), date: Convert.ToDateTime(csvTable.Rows[j][13].ToString()), source: csvTable.Rows[j][14].ToString());
                }

                string elementID = csvTable.Rows[i][2].ToString();
                string elementName = csvTable.Rows[i][3].ToString();
                string occupationID = csvTable.Rows[j][0].ToString();
                int occupationIndex = MasterOccupationList.FindIndex(p => p.SOCCode.Equals(occupationID));

                JobAttribute tmpAttribute = new JobAttribute(elementName, elementID, tmpImportance, tmpLevel, Constants.AttributeType.Knowledge);
                MasterOccupationList[occupationIndex].Knowledge.Add(tmpAttribute);

                int attributeIndex = MasterKnowledgeList.FindIndex(p => p.ElementID.Equals(elementID));
                if (attributeIndex == -1)
                {
                    MasterKnowledgeList.Add(new Attribute(elementName, elementID, MasterOccupationList[occupationIndex].SOCCode, Constants.AttributeType.Knowledge));
                }
                else
                {
                    MasterKnowledgeList[attributeIndex].OccupationIDs.Add(MasterOccupationList[occupationIndex].SOCCode);
                }
            }
        }
    }
}