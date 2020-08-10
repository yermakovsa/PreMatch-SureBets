using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using bet.Data;
using HtmlAgilityPack;

namespace bet.Functions
{
    class BetfairSB
    {
        public static List<Data.Match> matches;
        
        public static string GetName(string a, string b)
        {
            a = a.Replace(" ", ""); b = b.Replace(" ", "");
            string[] A = a.Split(","), B = b.Split(",");
            string res = A[A.Length - 1] + " " + A[0] + " v " + B[B.Length - 1] + " " + B[0];
            return res;
        }
        public static void Start(string html)
        {
            matches = new List<Data.Match>();
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            //HtmlNode[] nodes = htmlDoc.DocumentNode.SelectNodes("//li[@class='com-coupon-line-new-layout layout-2593174 avb-row avb-table  market-avb set-template market-1-columns']/div").ToArray();
            HtmlNode[] nodes = htmlDoc.DocumentNode.SelectNodes("//li/div").ToArray();
            foreach (var div in nodes)
            {
                HtmlDocument curr = new HtmlDocument();
                curr.LoadHtml(div.InnerHtml);
                if (curr.DocumentNode.SelectNodes("//div[3]/div/a/div/span") == null) continue;
                HtmlNode[] tmp = curr.DocumentNode.SelectNodes("//div[3]/div/a/div/span").ToArray();
                if (tmp.Length != 2) continue;
                List<Bet> bets = new List<Bet>();
                Data.Match match = new Data.Match(GetName(tmp[0].Attributes["title"].Value, tmp[1].Attributes["title"].Value), bets);
                if (curr.DocumentNode.SelectNodes("//div[2]/div/div/ul/li/a/span") == null) continue;
                tmp = curr.DocumentNode.SelectNodes("//div[2]/div/div/ul/li/a/span").ToArray();
                if (tmp.Length != 2 || tmp[0].InnerText.Contains("n") || tmp[1].InnerText.Contains("n")) continue;
                //Console.WriteLine(Regex.Replace(tmp[0].InnerText, @"\t|\n|\r", "").Replace(" ", "") + " " + Regex.Replace(tmp[1].InnerText, @"\t|\n|\r", "").Replace(" ", ""));
                bets.Add(new Bet("1", double.Parse(Regex.Replace(tmp[0].InnerText, @"\t|\n|\r", "").Replace(" ", ""))));
                bets.Add(new Bet("2", double.Parse(Regex.Replace(tmp[1].InnerText, @"\t|\n|\r", "").Replace(" ", ""))));
                match.listOfBets = bets;
                match.url = "betfair.com/sport/table-tennis";
                matches.Add(match);
            }
            Console.WriteLine("ENDbetfairSB: " + matches.Count());
        }
        public static Bookmaker GetBetfair()
        {
            Bookmaker bookmaker = new Bookmaker("betfairSB", matches);
            return bookmaker;
        }
    }
}
