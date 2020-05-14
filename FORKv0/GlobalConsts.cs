
public static class Globals
{
    public enum MultiConstraintAnalysis
    {
        SingleConstraint = 0,
        RateLossAsStops = 1,
        NoRateLossStops = 2
    }

    public enum LossCompassViews
    {
        DTpct,
        DTmin,
        SPD,
        MTBFmin,
        StopsActual

    }


    public enum LossCompassStates
    {
        A,
        B,
        C,
        D,
        E

    }
    public const string AddSecondaryKPI_defaultcomboboxstring = "Select a KPI";


    public enum Lang
    {
        English = 0,
        German = 1,
        Spanish = 2,
        French = 3,
        Portuguese = 4,
        Chinese_Simplified = 5
    }



    //Other
    public const string BLANK_INDICATOR = "BLANK";
    public const int availabilitybarMAXsize = 200;
    public const int LossCompass_chart_barMaxSize = 160;
    public const int LossCompass_chart_barMaxSizeC = 150;
    public const int LossCompass_datalabel_baseheight = 160;
    public const int LossCompass_bubble_baseheight = 225;
    public const int LossCOmpass_chart_TierC_labelbaseheight = 150;
    public const int LossCompass_chart_toplineresults_maxwidth = 900;

    public static class HTML
    {
        //"C:\Users\Public\"
        public const string SERVER_FOLDER_PATH = PATH_FORK + "x86registry\\";
        public const string PATH_FORK = "C:\\Users\\Public\\Public_assetfuel\\";
        public const string PATH_FORK_SETTINGS = PATH_FORK + "PrivateAssemblies\\";
        public const string PATH_FORK_TARGETS = PATH_FORK + "Common\\";
        public const string PATH_FORK_GLIDEPATH = PATH_FORK + "packagemanifests\\";

        public const string PATH_FORK_RAWDATA = PATH_FORK + "Clientx64\\";
        public const string PATH_FORK_RAWDATA_RAW = PATH_FORK_RAWDATA + "raw\\";
        public const string PATH_FORK_RAWDATA_INTERFACE = PATH_FORK_RAWDATA + "interface\\";

        public const string FILE_RAWTARGETS_CSV = "assetfuel_dtpct_targets.csv";
        //names for individual HTML files
        public const string HTML_UPTIME_VIEWER = "UptimeViewer";
        public const string HTML_LOSS_TREE = "LossTree";

        public const string HTML_LOSS_TREEMAP = "LossTreeMap";
        //colors
        public const string HTMLCOLOR_BrightGreen = "'00FF00'";
        public const string HTMLCOLOR_BrightRed = "'FF0000'";
        public const string HTMLCOLOR_BrightYellow = "'FFFF00'";
        public const string HTMLCOLOR_BrightBlue = "'0000FF'";
        public const string HTMLCOLOR_LightGrey = "'D8D8D8'";
    }

}