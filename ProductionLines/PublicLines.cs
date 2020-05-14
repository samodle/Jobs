using System;
using System.Collections.Generic;
using System.Text;

namespace ProductionLines
{
    public class ProductionUnit
    {
        public string Name { get; set; }

        public ProductionUnit(string Name)
        {
            this.Name = Name;
        }

        public override string ToString()
        {
            return this.Name;
        }
    }


    public class LineConfig
    {

        #region Variables
        public int MappingLevelA { get; set; }
        public int MappingLevelB { get; set; }
        public int Mapping_DTschedPlannedUnplanned { get; set; }
        public double ShiftStartFirst_Hr { get; set; }
        public double ShiftStartSecond_Hr { get; set; }
        public double ShiftStartThird_Hr { get; set; }
        public int NumberOfShifts { get; set; }
        public string Reason1Name { get; set; }
        public string Reason2Name { get; set; }
        public string Reason3Name { get; set; }
        public string Reason4Name { get; set; }
        public string FaultCodeName
        {
            get { return "Fault Code"; }
        }
        public string DTgroupName
        {
            get { return "DT Group"; }
        }

        //showing fields
        private bool _doIuseProductGroup = false;
        public bool FieldCheck_ProductGroup
        {
            get { return _doIuseProductGroup; }
            set { _doIuseProductGroup = value; }
        }
        #endregion

        #region Constructor
        public LineConfig() { }
        public LineConfig(int DTschedMapping, int assetfuelMapping, int shapeMapping, int formatMapping, string serverName, string serverPassword, string serverUsername)
        { }
        //  publi
        #endregion
    }



}
