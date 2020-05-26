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
        public LineConfig(int DTschedMapping, int assetfuelMapping, int shapeMapping, int formatMapping, string serverName, string serverPassword, string serverUsername)
        { }
    }



}
