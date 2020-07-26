using System;
using System.Collections.Generic;
using System.Text;

namespace Analytics
{
    public class ProfessionNearestNeighbors
    {
        public string Name { get; set; }
        public List<Tuple<Constants.AttributeType, List<string>>> NearestNeighbors { get; set; } = new List<Tuple<Constants.AttributeType, List<string>>>();

        public ProfessionNearestNeighbors(string name)
        {
            this.Name = name;
        }


    }
}
