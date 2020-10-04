﻿using DataPersistancy;
using System;
using System.Collections.Generic;
using System.Text;
using static Analytics.Constants;

namespace Analytics
{
    public class CPM_Graph
    {
        public ActiveLocations Loc { get; set; }

        public CPM_Node OneA {get;set;}
        public CPM_Node TwoA { get; set; }
        public CPM_Node TwoB { get; set; }
        public CPM_Node ThreeA { get; set; }
        public CPM_Node ThreeB { get; set; }
        public CPM_Node ThreeC { get; set; }
        public CPM_Node ThreeD { get; set; }

        public void Delete_Node(CPM_Node n)
        {
            if(OneA.ID == n.ID)
            {

            } else if(TwoA.ID == n.ID)
            {
                Delete_From_1(n.ID);
            }
            else if (TwoB.ID == n.ID)
            {
                Delete_From_1(n.ID);
            }
            else if (ThreeA.ID == n.ID)
            {
                Delete_From_2A(n.ID);
            }
            else if (ThreeB.ID == n.ID)
            {
                Delete_From_2A(n.ID);
            }
            else if (ThreeC.ID == n.ID)
            {
                Delete_From_2B(n.ID);
            }
            else if (ThreeD.ID == n.ID)
            {
                Delete_From_2B(n.ID);
            }

            setOneA(OneA);
        }

        public void Delete_From_2A(int i)
        {
            int index = TwoA.NextSteps.FindIndex(m => m == i);
            TwoA.NextSteps.RemoveAt(index);
            TwoA.NextSteps.Add(i);

        }

        public void Delete_From_2B(int i)
        {
            int index = TwoB.NextSteps.FindIndex(m => m == i);
            TwoB.NextSteps.RemoveAt(index);
            TwoB.NextSteps.Add(i);
        }

        public void Delete_From_1(int i)
        {
            int index = OneA.NextSteps.FindIndex(m => m == i);
            OneA.NextSteps.RemoveAt(index);
            OneA.NextSteps.Add(i);
        }




        public CPM_Graph(CPM_Node startingPoint, ActiveLocations l)
        {
            foreach (CPM_Node n in DemoIO.nodes) { n.JuiceSalary(l);  }

            setOneA(startingPoint);
            Loc = l;
        }

        public void setOneA(CPM_Node n)
        {
            OneA = n;

            setTwoA(n.NextNode(0, Loc));
            setTwoB(n.NextNode(1, Loc));
        }

        public void setTwoA(CPM_Node n)
        {
            TwoA = n;

            setThreeA(n.NextNode(0, Loc));
            setThreeB(n.NextNode(1, Loc));
        }

        public void setThreeA(CPM_Node n)
        {
            ThreeA = n;
        }
        public void setThreeB(CPM_Node n)
        {
            ThreeB = n;
        }

        public void setTwoB(CPM_Node n)
        {
            TwoB = n;

            setThreeC(n.NextNode(0, Loc));
            setThreeD(n.NextNode(1, Loc));
        }

        public void setThreeC(CPM_Node n)
        {
            ThreeC = n;
        }
        public void setThreeD(CPM_Node n)
        {
            ThreeD = n;
        }
    }
}