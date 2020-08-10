using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bet.Data
{
    public class Bet
    {
        public string name;
        public string tagName = "0";
        public double coef;
        public double handicap = 0;

        public Bet(string BetName, double BetCoef)
        {
            name = BetName;
            coef = BetCoef;
        }
    }
}
