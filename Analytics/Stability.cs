using System;
using System.Collections.Generic;
using System.Text;
using static ForkAnalyticsSettings.GlobalConstants;

namespace Analytics
{
    public class StabilityAnalysis
    {
        #region Variables
        private double DevAbove_One;// = Mu + Sigma;
        private double DevAbove_Two;// = Mu + 2 * Sigma;
        private double DevAbove_Three;// = Mu + 3 * Sigma;
        private double DevBelow_One;// = Mu - Sigma;
        private double DevBelow_Two;// = Mu - 2 * Sigma;
        private double DevBelow_Three;// = Mu - 3 * Sigma;

        private int n_Rule1;
        private int n_Rule2;
        private int n_Rule3;
        private int n_Rule4;
        private int n_Rule5;
        private int n_Rule6;
        private int n_Rule7;
        private int n_Rule8;
        private int n_Rule9;

        private readonly double Mu;
        private readonly double Sigma;
        #endregion

        #region Constructor
        public StabilityAnalysis() { }
        public StabilityAnalysis(double Mu, double Sigma, ControlRulesets activeRuleset)
        {
            this.Mu = Mu;
            this.Sigma = Sigma;
            DevAbove_One = Mu + Sigma;
            DevAbove_Two = Mu + 2 * Sigma;
            DevAbove_Three = Mu + 3 * Sigma;
            DevBelow_One = Mu - Sigma;
            DevBelow_Two = Mu - 2 * Sigma;
            DevBelow_Three = Mu - 3 * Sigma;
            setNvalues(activeRuleset);
        }
        #endregion

        #region Setting 'n' Values
        public void setNvalues(ControlRulesets RuleSet)
        {
            switch (RuleSet)
            {
                case ControlRulesets.AIAG:
                    setNvalues(Rule1: 1, Rule2: 2, Rule3: 4, Rule4: 7, Rule5: 6, Rule6: 15, Rule7: 14, Rule8: 8, Rule9: 0);
                    break;
                case ControlRulesets.IHI:
                    setNvalues(Rule1: 1, Rule2: 2, Rule3: 0, Rule4: 8, Rule5: 6, Rule6: 15, Rule7: 0, Rule8: 0, Rule9: 0);
                    break;
                case ControlRulesets.Westgard:
                    setNvalues(Rule1: 1, Rule2: 2, Rule3: 0, Rule4: 8, Rule5: 7, Rule6: 0, Rule7: 0, Rule8: 0, Rule9: 4);
                    break;
                case ControlRulesets.Nelson:
                    setNvalues(Rule1: 1, Rule2: 2, Rule3: 4, Rule4: 9, Rule5: 6, Rule6: 15, Rule7: 14, Rule8: 8, Rule9: 0);
                    break;
                case ControlRulesets.Montgomery:
                    setNvalues(Rule1: 1, Rule2: 2, Rule3: 4, Rule4: 8, Rule5: 6, Rule6: 15, Rule7: 14, Rule8: 8, Rule9: 0);
                    break;
                case ControlRulesets.WesternElectric:
                    setNvalues(Rule1: 1, Rule2: 2, Rule3: 4, Rule4: 8, Rule5: 0, Rule6: 0, Rule7: 0, Rule8: 0, Rule9: 0);
                    break;
            }
        }
        public void setNvalues(int Rule1 = 0, int Rule2 = 0, int Rule3 = 0, int Rule4 = 0, int Rule5 = 0, int Rule6 = 0, int Rule7 = 0, int Rule8 = 0, int Rule9 = 0)
        {
            this.n_Rule1 = Rule1;
            this.n_Rule2 = Rule2;
            this.n_Rule3 = Rule3;
            this.n_Rule4 = Rule4;
            this.n_Rule5 = Rule5;
            this.n_Rule6 = Rule6;
            this.n_Rule7 = Rule7;
            this.n_Rule8 = Rule8;
            this.n_Rule9 = Rule9;
        }

        public int numActiveRules
        {
            get
            {
                int i = 0;
                if (n_Rule1 > 0) { i += 1; }
                if (n_Rule2 > 0) { i += 1; }
                if (n_Rule3 > 0) { i += 1; }
                if (n_Rule4 > 0) { i += 1; }
                if (n_Rule5 > 0) { i += 1; }
                if (n_Rule6 > 0) { i += 1; }
                if (n_Rule7 > 0) { i += 1; }
                if (n_Rule8 > 0) { i += 1; }
                if (n_Rule9 > 0) { i += 1; }
                return i;
            }
        }
        #endregion

        #region Scores
        public List<double> getPeriodicStabilityScores(List<double> R, int Period, int FirstPeriodIndex, double MaxScore = 3)
        {
            var tmpScoreList = new List<double>();

            for (int i = FirstPeriodIndex + Period - 1; i + Period < R.Count; i++)
            {
                tmpScoreList.Add(getStabilityScore(R.GetRange(i, Period), MaxScore));
            }
            return tmpScoreList;
        }

        public int getStabilityScore(List<double> R, double MaxScore)
        {
            int tmpInt = getNumRuleViolationsForPeriod(R);
            double tmpDbl = ((double)tmpInt / numActiveRules);
            return ((int)(MaxScore * tmpDbl));
        }


        private int getNumRuleViolationsForPeriod(List<double> R)
        {
            int numRuleViolations = 0;
            if (isRule1Violation(R)) { numRuleViolations += 1; }
            if (isRule2Violation(R)) { numRuleViolations += 1; }
            if (isRule3Violation(R)) { numRuleViolations += 1; }
            if (isRule4Violation(R)) { numRuleViolations += 1; }
            if (isRule5Violation(R)) { numRuleViolations += 1; }
            if (isRule6Violation(R)) { numRuleViolations += 1; }
            if (isRule7Violation(R)) { numRuleViolations += 1; }
            if (isRule8Violation(R)) { numRuleViolations += 1; }
            if (isRule9Violation(R)) { numRuleViolations += 1; }

            return numRuleViolations;
        }

        #endregion

        #region Atomic Rule Violations
        /* n points above UCL or below LCL */
        public bool isRule1Violation(List<double> R)
        {
            if (n_Rule1 == 0) { return false; }
            for (int i = 0; i < R.Count; i++)
            {
                if (R[i] > DevAbove_Three || R[i] < DevBelow_Three) { return true; }
            }
            return false;
        }

        /* Zone A: n of n+1 points above/below 2 sigma */
        public bool isRule2Violation(List<double> R)
        {
            if (n_Rule2 == 0) { return false; }
            int plus2sig = 0;
            int min2sig = 0;
            for (int i = 0; i < R.Count; i++)
            {
                if (R[i] > DevAbove_Two) { plus2sig += 1; }
                else if (R[i] < DevBelow_Two) { min2sig += 1; }
            }
            if (plus2sig >= n_Rule3 - 1 || min2sig >= n_Rule3 - 1) { return true; }
            return false;
        }

        /* Zone B: n of n+1 points above/below 1 sigma */
        public bool isRule3Violation(List<double> R)
        {
            if (n_Rule3 == 0) { return false; }
            int plus1sig = 0;
            int min1sig = 0;
            for (int i = 0; i < R.Count; i++)
            {
                if (R[i] > DevAbove_One) { plus1sig += 1; }
                else if (R[i] < DevBelow_One) { min1sig += 1; }
            }
            if (plus1sig >= n_Rule3 - 1 || min1sig >= n_Rule3 - 1) { return true; }
            return false;
        }

        /* n points in a row above/ below center line */
        public bool isRule4Violation(List<double> R)
        {
            if (n_Rule4 == 0) { return false; }
            //core
            int ptsAbove = 0;
            int ptsBelow = 0;
            for (int i = 0; i < R.Count; i++)
            {
                if (R[i] > Mu) { ptsAbove += 1; ptsBelow = 0; }
                if (R[i] < Mu) { ptsBelow += 1; ptsAbove = 0; }

                if (ptsAbove == n_Rule4 || ptsBelow == n_Rule4) { return true; }
            }
            //
            return false;
        }

        /* Trends of n points in a row increasing or decreasing */
        public bool isRule5Violation(List<double> R)
        {
            if (n_Rule5 == 0) { return false; }
            int ptsInc = 0;
            int ptsDec = 0;
            for (int i = 1; i < R.Count; i++)
            {
                if (R[i] > R[i - 1]) { ptsInc += 1; ptsDec = 0; }
                else { ptsDec += 1; ptsInc = 0; }
            }
            if (ptsInc == n_Rule5 || ptsDec == n_Rule5) { return true; }
            return false;
        }

        /* Zone C: n points in a row inside Zone C (hugging) */
        public bool isRule6Violation(List<double> R)
        {
            if (n_Rule6 == 0) { return false; }
            int ptsHugging = 0;
            for (int i = 0; i < R.Count; i++)
            {
                if (DevBelow_One < R[i] && R[i] < DevAbove_One) { ptsHugging += 1; if (ptsHugging == n_Rule6) { return true; } } else { ptsHugging = 0; }
            }
            return false;
        }

        /* n points in a row alternating up and down */
        public bool isRule7Violation(List<double> R)
        {
            if (n_Rule7 == 0) { return false; }
            int Counter = 0;

            for (int i = 0; i < R.Count - 2; i++)
            {
                if ((R[i] < R[i + 1] && R[i + 2] < R[i + 1]) || (R[i] > R[i + 1] && R[i + 2] > R[i + 1]))
                { Counter += 1; if (Counter == n_Rule7) { return true; } }
                else { Counter = 0; }
            }
            return false;
        }

        /* Zone C:n points in a row outside Zone C */
        public bool isRule8Violation(List<double> R)
        {
            if (n_Rule8 == 0) { return false; }
            int Counter = 0;
            for (int i = 0; i < R.Count; i++)
            {
                if (R[i] > DevAbove_Three || R[i] < DevBelow_Three) { Counter += 1; if (Counter == n_Rule8) { return true; } } else { Counter = 0; }
            }
            return false;
        }

        /* Zone B: n points above/ below 1 sigma; 2 points one above, one below 2 sigma */
        public bool isRule9Violation(List<double> R)
        {
            if (n_Rule9 == 0) { return false; }
            bool Above2 = false;
            bool Below2 = false;
            int Counter = 0;

            for (int i = 0; i < R.Count; i++)
            {
                if (R[i] > DevBelow_One && R[i] < DevAbove_One) { Above2 = false; Below2 = false; Counter = 0; }
                else
                {
                    Counter += 1;
                    if (R[i] > DevAbove_Two) { Above2 = true; } else if (R[i] < DevBelow_Two) { Below2 = true; }
                    if (Counter == n_Rule9 && Above2 && Below2) { return true; }
                }
            }

            return false;
        }
        #endregion







    }
}
