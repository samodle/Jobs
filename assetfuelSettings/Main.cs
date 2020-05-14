using System;
using System.Collections.Generic;
using System.Text;

namespace ForkAnalyticsSettings
{
    static class Main
    {
        public static bool isAvailabilityMode = false;

    }

    static class DataAnalysis
    {
        public static double minMinutesBetweenPlannedEvents = 0;
    }

    static class ChronicSporadic
    {
        public static double minSchedTime = 300;
    }

    static class Mapping
    {
        public static int PrimaryField = 14;
        public static int SecondaryField = -1;
    }
}



/*   __            _______.     _______. _______. ___________. _______  __    __   _______  __      
    /   \         /       |    /       ||   ____||           ||   ____||  |  |  | |   ____||  |     
   /  ^  \       |   (----`   |   (----`|  |__   `---|  |----`|  |__   |  |  |  | |  |__   |  |     
  /  /_\  \       \   \        \   \    |   __|      |  |     |   __|  |  |  |  | |   __|  |  |     
 /  _____  \  .----)   |   .----)   |   |  |____     |  |     |  |     |  `--'  | |  |____ |  `----.
/__/     \__\ |_______/    |_______/    |_______|    |__|     |__|      \______/  |_______||_______|
                                             © 2015 CTIS LLC
*/

