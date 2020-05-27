using System;
using System.Windows.Forms;

namespace Windows_Desktop
{

    static class Import_CSV
    {
        public static void CSV_readTargetsFile()
        {
            int i = 0;
            int tmpCard = 0;
            double tmpValue = 0;
            string tmpFieldName = "";
            string tmpLineSite = "";
            string tmpLine = "";
            string tmpSite = "";
            int tmpCharIndex = 0;
            int lineIndex = 0;
            bool isFirstLine = true;
            try
            {
                using (Microsoft.VisualBasic.FileIO.TextFieldParser MyReader = new Microsoft.VisualBasic.FileIO.TextFieldParser("Globals.HTML.PATH_FORK_TARGETS + Globals.HTML.FILE_RAWTARGETS_CSV"))
                {
                    MyReader.TextFieldType = Microsoft.VisualBasic.FileIO.FieldType.Delimited;
                    MyReader.SetDelimiters(",");
                    string[] currentRow = null;
                    while (!MyReader.EndOfData)
                    {
                        try
                        {
                            currentRow = MyReader.ReadFields();
                            string currentField = null;
                            i = 0;
                            foreach (string currentField_loopVariable in currentRow)
                            {
                                currentField = currentField_loopVariable;
                                switch (i)
                                {
                                    case 0:
                                        tmpLineSite = currentField;
                                        tmpCharIndex = tmpLineSite.IndexOf(":");
                                        if (tmpCharIndex > -1)
                                        {
                                            tmpLine = tmpLineSite.Substring(0, tmpCharIndex);
                                            tmpSite = tmpLineSite.Substring(tmpCharIndex + 2, tmpLineSite.Length - tmpLine.Length - 2);
                                        }
                                        break;
                                    case 1:
                                        tmpCard = Convert.ToInt32(currentField);
                                        break;
                                    case 2:
                                        tmpFieldName = currentField;
                                        break;
                                    case 3:
                                        tmpValue = Convert.ToDouble(currentField);
                                        break;
                                }
                                i += 1;
                            }
                    

                        }
                        catch (Microsoft.VisualBasic.FileIO.MalformedLineException ex)
                        {
                            MessageBox.Show("Line " + ex.Message + "is not valid and will be skipped.");
                        }
                    }

                }


            }
            catch (System.IO.FileNotFoundException ex)
            {
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Importing Default Settings.");
            }
        }
    }
}
