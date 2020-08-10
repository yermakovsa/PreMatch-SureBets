using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bet.Data
{
    public class Match
    {
        public string matchName;
        public string date = "01.01.2020";
        public string url = "emptyUrl";
        public DateTime dateTime;
        public List<Bet> listOfBets;

        public Match(string name, List<Bet> bets)
        {
            matchName = name;
            listOfBets = bets;
        }
    }
}
