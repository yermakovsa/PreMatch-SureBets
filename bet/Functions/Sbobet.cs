using bet.Data;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bet.Functions
{
    class Sbobet
    {
        public static List<Match> matches;
        public static Dictionary<string, string> map;

        static string GetDate(string a)
        {
            string[] A = a.Split(" ");
            string date = A[1] + "/" + map[A[0]];
            return date;
        }
        static string GetName(string a, string b)
        {
            string name = "";
            string[] A = a.Trim().Split(" "), B = b.Trim().Split(" ");
            name += A[0] + " ";
            if (A.Length == 3) name += A[2];
            else name += A[1];
            name += " v ";
            name += B[0] + " ";
            if (B.Length == 3) name += B[2];
            else name += B[1];
            return name;
        }
        static DateTime GetDateTime(string a, string b)
        {
            string[] A = a.Split(" "), B = b.Split(":");
            DateTime dateTime = new DateTime();
            dateTime = dateTime.AddYears(2019);
            dateTime = dateTime.AddMonths(int.Parse(map[A[0]]) - 1);
            dateTime = dateTime.AddDays(int.Parse(A[1]) - 1);
            dateTime = dateTime.AddHours(int.Parse(B[0]));
            dateTime = dateTime.AddMinutes(int.Parse(B[1]));
            return dateTime;
        }
        public static void Init()
        {
            map = new Dictionary<string, string>();
            matches = new List<Match>();
            map.Add("Jul", "07");

        }
        public static void Start(string html)
        {
            Console.WriteLine("SbobetSTART: " + DateTime.Now.ToLongTimeString());
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            HtmlNode[] nodes = htmlDoc.DocumentNode.SelectNodes("//table/tbody/tr").ToArray();
            foreach(var tr in nodes)
            {
                HtmlDocument curr = new HtmlDocument();
                curr.LoadHtml(tr.InnerHtml);
                HtmlNode[] tds = curr.DocumentNode.SelectNodes("td").ToArray();
                if (tds.Length != 5) continue;
                HtmlNode[] tmp = curr.DocumentNode.SelectNodes("td/div/div[@class='DateTimeTxt']/span").ToArray();
                if (tmp[1].InnerText == "Live") continue;
                string date = GetDate(tmp[0].InnerText);
                DateTime dateTime = GetDateTime(tmp[0].InnerText, tmp[1].InnerText);
                tmp = curr.DocumentNode.SelectNodes("td/a/span[@class='OddsL']").ToArray();
                if (tmp.Length != 2) continue;
                string name = GetName(tmp[0].InnerText, tmp[1].InnerText);
                if (curr.DocumentNode.SelectNodes("td/a/span[@class='OddsM']") != null) continue;
                List<Bet> bets = new List<Bet>();
                tmp = curr.DocumentNode.SelectNodes("td/a/span[@class='OddsR']").ToArray();
                bets.Add(new Bet("1", double.Parse(tmp[0].InnerText)));
                bets.Add(new Bet("2", double.Parse(tmp[1].InnerText)));
                Match match = new Match(name, bets);
                match.dateTime = dateTime;
                match.date = date;
                matches.Add(match);
            }
            /*foreach(Match match1 in matches)
            {
                Console.WriteLine(match1.matchName + " " + match1.dateTime);
                for (int i = 0; i < match1.listOfBets.Count(); i++)
                    Console.WriteLine(match1.listOfBets[i].name + " " + match1.listOfBets[i].coef);
            }*/
            Console.WriteLine("Sbobet END: " + DateTime.Now.ToLongTimeString() + " " + matches.Count());
        }

        public static Bookmaker GetSbobet()
        {
            Bookmaker bookmaker = new Bookmaker("sbobet", matches);
            return bookmaker;
        }
    }
}
