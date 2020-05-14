using DataInterface;
using ForkAnalyticsSettings;
using ProductionLines;
using RawData;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows_Desktop.Properties;

namespace Windows_Desktop
{

    #region Line
    public class productionLine : IEquatable<productionLine>
    {

        #region Variables
        //parent site/line
        public productionModule parentModule;

        public productionSite parentSite;

        //targets
        private bool _doIhaveTargets = false;

        //static fields
        protected string _lineName;
        private int _defaultDowntimeFieldA = -1;

        private int _defaultDowntimeFieldB = -1;
        //dual constraint stuff
        internal bool _isDualConstraint;
        protected string _mainProdUnit;
        protected string _mainProfProd;
        internal string _rateLossDisplay;
        protected long _rateLossEvents;

        private object[,] _rawRateLossData;
        protected int _prStoryMapping;


        //shift configuration, critical for production data
        protected int _numberOfShifts;
        protected double _shiftDurationHrs;

        protected double _DayStartTimeHrs;
        protected double _SecondShiftStartHrs;

        protected double _ThirdShiftStartHrs;
        //raw data
        private DateTime _rawProfStartTime;

        private DateTime _rawProfEndTime;
        public downtimeInterface rawDowntimeData;

        private object[,] _rawProficyData;
        private object[,] _rawProficyProductionData;

        public object[,] rawProficyProductionData
        {
            get { return _rawProficyProductionData; }
            set { _rawProficyProductionData = value; }
        }


        public bool isFilterByBrandcode = false;
        public List<string> BrandCodesWeWant = new List<string>();
        //production based
        public List<string> BrandCodeReport = new List<string>();
        //dt based
        public List<string> ShiftReport = new List<string>();
        //dt based
        public List<string> TeamReport = new List<string>();
        //dt based
        public List<string> ProductReport = new List<string>();

        //properties
        public int SQLdowntimeProcedure
        {
            get { return parentModule.SQLprocedure; }
        }
        public int SQLproductionProcedure
        {
            get { return parentModule.SQLprocedurePROD; }
        }

        public string Sector
        {
            get { return (string)parentModule.Sector; }
        }
        public string SiteName
        {
            get { return parentSite.Name; }
        }
        public double ShiftStartFirst_Hr
        {
            get { return _DayStartTimeHrs; }
        }
        public double ShiftStartSecond_Hr
        {
            get { return _SecondShiftStartHrs; }
        }
        public double ShiftStartThird_Hr
        {
            get
            {
                if (_numberOfShifts == 3)
                {
                    return _ThirdShiftStartHrs;
                }
                else
                {
                    //MsgBox("Only " & _numberOfShifts & " Shifts! You asked for 3! Will Return -1. Best of Luck...")
                    return -1;
                }
            }
        }

        public string Name
        {
            get { return _lineName; }
        }


        //showing fields
        private bool _doIuseProductGroup = false;

        #endregion


        public override string ToString()
        {

            return parentSite.Name + " " + parentModule.Name + " " + _lineName;
        }

        #region "Sortable & Equitable"
        //implementation of ISEQUITABLE
        public override int GetHashCode()
        {
            int hashA = 0;
            int hashB = 0;
            int hashC = 0;

            hashA = GlobalFcns.GetHashCode(Name) * 100000;
            hashB = GlobalFcns.GetHashCode(parentModule.Name) * 1000;
            hashC = GlobalFcns.GetHashCode(parentSite.Name);

            return hashA + hashB + hashC;
            //  return base.GetHashCode();
        }


        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            productionLine objAsPart = obj as productionLine;
            if (objAsPart == null)
            {
                return false;
            }
            else
            {
                return Equals(objAsPart);
            }
        }
        public bool Equals(productionLine other)
        {
            if (other == null)
            {
                return false;
            }
            return (this.Name.Equals(other.Name) & this.SiteName.Equals(other.SiteName));
        }
        #endregion

    }

    #endregion


    #region "Module / Site / Sector"
    public class productionModule
    {
        protected string _moduleName;
        public productionSite parentSite;
        public BusinessUnit parentSector;
        public List<productionLine> LinesList = new List<productionLine>();

        public int prStory_Mapping;
        private int _SQLprocedure;

        private int _SQLprocedureProduction;
        private int _defaultDowntimeField;

        private int _defaultDowntimeField_Secondary;

        private int _DTschedPlannedUnplannedMapping;
        internal string _Reason1Name;
        internal string _Reason2Name;
        internal string _Reason3Name;

        internal string _Reason4Name;
        public int MappingLevelA
        {
            get { return _defaultDowntimeField; }
        }
        public int MappingLevelB
        {
            get { return _defaultDowntimeField_Secondary; }
        }
        public string Sector
        {
            get { return parentSector.Name; }
        }
        internal int DTschedMap
        {
            get { return _DTschedPlannedUnplannedMapping; }
        }
        public int SQLprocedure
        {
            get { return _SQLprocedure; }
        }
        public int SQLprocedurePROD
        {
            get { return _SQLprocedureProduction; }
        }
        public string ToString()
        {
            dynamic tempString = null;
            tempString = ", Lines: " + Environment.NewLine;
            if (LinesList.Count > 0)
            {
                foreach (productionLine productionLine_loopVariable in LinesList)
                {
                    tempString = tempString + productionLine_loopVariable.Name + Environment.NewLine;
                }
            }
            return "Module: " + _moduleName + tempString;
        }

        //properties for protected variables
        public string Name
        {
            get { return _moduleName; }
        }
    }

    public class productionSite : IEquatable<productionSite>
    {

        #region "Variables & Properties"
        protected string _siteName;
        protected string _ProficyServerAddress;
        private string _ProficyServerUsername;

        private string _ProficyServerPassword;
        protected string _HistorianServerAddress;

        public List<productionModule> ModulesList = new List<productionModule>();

        private string _ThreeLetterID;
        public string ThreeLetterID
        {
            get { return _ThreeLetterID; }
        }

        //properties for protected variables
        public string Name
        {
            get { return _siteName; }
        }
        public string ProficyServer
        {
            get { return _ProficyServerAddress; }
        }
        public string ProficyServer_Password
        {
            get { return _ProficyServerPassword; }
        }
        public string ProficyServer_Username
        {
            get { return _ProficyServerUsername; }
        }

        public string HistorianServer
        {
            get { return _HistorianServerAddress; }
        }
        #endregion

        public string toString()
        {
            return _siteName;
        }

        //, ColloquialName As String) ', Optional languageSelected As Integer = Language.English)
        public productionSite(string siteName, string profServer, string HistServer, string profPassword, string profUsername, string newThreeLetterID)
        {
            _siteName = siteName;
            _ProficyServerAddress = profServer;
            _HistorianServerAddress = HistServer;
            _ProficyServerPassword = profPassword;
            _ProficyServerUsername = profUsername;
            _ThreeLetterID = newThreeLetterID;
        }



        //implementation of ISEQUITABLE
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            productionSite objAsPart = obj as productionSite;
            if (objAsPart == null)
            {
                return false;
            }
            else
            {
                return Equals(objAsPart);
            }
        }
        public bool Equals(productionSite other)
        {
            if (other == null)
            {
                return false;
            }
            return (this.Name.Equals(other.Name));
        }
    }

    public class BusinessUnit : IEquatable<BusinessUnit>
    {
        protected string _BUname;
        //Public LinesList As List(Of productionLine)()

        public List<productionModule> ModuleList = new List<productionModule>();
        public override string ToString()
        {
            return _BUname;
        }
        //CONSTRUCTOR 
        public BusinessUnit(string BUname)
        {
            _BUname = BUname;
        }


        //properties for protected variables
        public string Name
        {
            get { return _BUname; }
        }
        public object isSectorAtSite(string siteName)
        {
            for (int i = 0; i <= ModuleList.Count - 1; i++)
            {
                if (siteName.Equals(ModuleList[i].parentSite))
                    return true;
            }
            return false;
        }


        //implementation of ISEQUITABLE
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            BusinessUnit objAsPart = obj as BusinessUnit;
            if (objAsPart == null)
            {
                return false;
            }
            else
            {
                return Equals(objAsPart);
            }
        }
        public bool Equals(BusinessUnit other)
        {
            if (other == null)
            {
                return false;
            }
            return (this.Name.Equals(other.Name));
        }
    }
    #endregion

}
