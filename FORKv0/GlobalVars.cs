
using System;
using System.Collections.Generic;
using System.Windows;

namespace Windows_Desktop
{
    static class Publics
    {
        //     public static List<productionLine> AllProductionLines { get; set; } = new List<productionLine>();
        public static List<productionModule> AllProductionModules = new List<productionModule>();
        public static List<productionSite> AllProductionSites = new List<productionSite>();
        public static List<BusinessUnit> AllProductionSectors = new List<BusinessUnit>();

        public static int selectedindexofLine_temp { get; set; }
        public static System.DateTime starttimeselected { get; set; }
        public static System.DateTime endtimeselected { get; set; }

        public static string tempreasonlevel { get; set; }

        public static bool IsAnalyzeButtonClickSource_Analyze { get; set; } = false;

        public static System.Windows.Input.MouseButtonEventArgs f { get; set; }
        public static EventArgs g { get; set; }

    }
}
