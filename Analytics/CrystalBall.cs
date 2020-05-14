using System;
using System.Collections.Generic;
using System.Text;
using static ForkAnalyticsSettings.GlobalConstants;
using static Windows_Desktop.Window_Dashboard_Settings;

namespace Analytics
{
    //comprehensive summary of a simulation - ie could be shared w/ other users
    public class CrystalBallAnalysis
    {
        #region Variables
        public string Name { get; set; }
        public DateTime SaveDate { get; set; }

        public List<DowntimeField> MappingA { get; set; }
        public List<DowntimeField> MappingB { get; set; }

        public List<CrystalBallSimulation> Changeset { get; set; } = new List<CrystalBallSimulation>();
        public List<double> OEE_Steps { get; set; } = new List<double>();
        #endregion

        #region Constructor
        public CrystalBallAnalysis() { }
        public CrystalBallAnalysis(string Name, DateTime SaveDate) : this()
        {
            this.Name = Name;
            this.SaveDate = SaveDate;
        }
        public CrystalBallAnalysis(string Name, DateTime SaveDate, List<DowntimeField> MappingA, List<DowntimeField> MappingB) : this(Name, SaveDate)
        {
            this.MappingA = MappingA;
            this.MappingB = MappingB;
        }
        #endregion

        public void ClearAllSimulations()
        {
            Changeset.Clear();
            OEE_Steps.Clear();
        }

        public void RemoveSimulation(string Name, CardTier Card)
        {
            int i = Changeset.IndexOf(new CrystalBallSimulation(Name, Card));
            if (i > -1)
            {
                Changeset.RemoveAt(i);
                OEE_Steps.RemoveAt(i);
            }
        }

        public List<Tuple<double, string>> getGlidepath(List<DateTime> DateList)
        {
            var tmpList = new List<Tuple<double, string>>();
            for (int i = 0; i < DateList.Count; i++)
            {
                tmpList.Add(new Tuple<double, string>(FindOEEatTime(DateList[i]), "Change #" + i));
            }
            return tmpList;
        }

        private double FindOEEatTime(DateTime targetDate)
        {
            if (Changeset.Count > 0)
            {
                if (targetDate < Changeset[0].DueDate) { return OEE_Steps[0]; }
                else if (targetDate >= Changeset[Changeset.Count - 1].DueDate) { return OEE_Steps[OEE_Steps.Count - 1]; }
                else
                {
                    for (int i = 0; i < Changeset.Count - 1; i++)
                    {
                        if (targetDate >= Changeset[i].DueDate && targetDate < Changeset[i + 1].DueDate) { return OEE_Steps[i + 1]; }
                    }
                }

            }
            return -1; //this is bad...
        }

        public void addNewSimulation(string Name, List<string> ParentNames, CardTier Tier, double ScaleFactor_Stops = 1, double ScaleFactor_DT = 1)
        {
            Changeset.Add(new CrystalBallSimulation(Name, ParentNames, ScaleFactor_Stops, ScaleFactor_DT, Tier));
        }
        public void addNewSimulation(string Name, List<string> ParentNames, DateTime DueDate, CardTier Tier, double ScaleFactor_Stops = 1, double ScaleFactor_DT = 1)
        {
            Changeset.Add(new CrystalBallSimulation(Name, ParentNames, ScaleFactor_Stops, ScaleFactor_DT, Tier));
            Changeset[Changeset.Count - 1].DueDate = DueDate;
        }


    }

    public class CrystalBallSimulation : IEquatable<CrystalBallSimulation>
    {
        #region Variables
        public string Name { get; set; } //name of failure mode
        public List<string> ParentNames { get; set; } = new List<string>(); //names to establish drill down position
        public double ScaleFactor_Stops { get; set; } = 1; //user entered scale factor (i.e. Original x 'this' = New)
        public double ScaleFactor_DT { get; set; } = 1; //ditto
        public DateTime DueDate { get; set; } //assigned due date
        public CardTier OriginalCardTier { get; set; }
        #endregion

        public int getCurrentLevel(int TierALevel) { return ParentNames.Count - TierALevel; }

        public bool Equals(CrystalBallSimulation other)
        {
            if (other.Name == this.Name && other.OriginalCardTier == this.OriginalCardTier) { return true; }
            else { return false; }
        }

        //   public string getCardName(int TierALevel) { return getStringForEnum_Card((CardTier) getCurrentLevel(TierALevel)); }

        #region Constructor
        public CrystalBallSimulation() { }
        public CrystalBallSimulation(string Name, CardTier Card)
        {
            this.Name = Name;
            this.OriginalCardTier = Card;
        }
        public CrystalBallSimulation(string Name, List<string> ParentNames)
        {
            this.Name = Name;
            this.ParentNames = ParentNames;
        }
        public CrystalBallSimulation(string Name, List<string> ParentNames, double ScaleFactor_Stops, double ScaleFactor_DT, CardTier Tier) : this(Name, ParentNames)
        {
            this.ScaleFactor_Stops = ScaleFactor_Stops;
            this.ScaleFactor_DT = ScaleFactor_DT;
            this.OriginalCardTier = Tier;
        }
        #endregion


    }
}
