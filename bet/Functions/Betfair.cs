using bet.Data;
using DuoVia.FuzzyStrings;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace bet.Functions
{
    class Betfair
    {
        public static Bookmaker bookmaker;
        public static Dictionary<string, string> map = new Dictionary<string, string>();
        public static List<Match> matches = new List<Match>(), tmpMatches = new List<Match>();

        public static string JsonRequestBetfair(string json)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.betfair.com/exchange/betting/json-rpc/v1");
            httpWebRequest.Method = "POST";
            httpWebRequest.Headers.Add("X-Application", "QvTGCXsIS9JNYASP");
            httpWebRequest.Headers.Add("X-Authentication", "m2PZwpqTYfEl2rEk2ES/LcbO3jzuBFDLC+FrVt5seMs=");
            httpWebRequest.ContentType = "application/json";
            //Console.WriteLine(httpWebRequest.Headers + " ");
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(json);
            }
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var responseText = streamReader.ReadToEnd();
                return responseText;
            }
        }

        public static string GetJson(string info, string id)
        {
            if (info == "listSports")
            {
                return "{\"jsonrpc\": \"2.0\", \"method\": \"SportsAPING/v1.0/listEventTypes\", \"params\": {\"filter\":{ }}, \"id\": 1}";
            }
            else if (info == "listMatches")
            {
                DateTime dateTime = DateTime.Now;
                string today = dateTime.Year + "-" + dateTime.Month + "-" + dateTime.Day;
                dateTime = dateTime.AddDays(1);
                string tomorrow = dateTime.Year + "-" + dateTime.Month + "-" + dateTime.Day;
                return "{\"jsonrpc\": \"2.0\", \"method\": \"SportsAPING/v1.0/listEvents\", \"params\": " +
                    "{\"filter\":" +
                    "{\"eventTypeIds\":[\"" + id + "\"] }," +
                    "\"marketStartTime\": {" +
                    "\"from\": \"" + today + "T00:00Z\"," +
                    "\"to\": \"" + tomorrow + "T23:59Z\"" +
                    "}}, \"id\": 1}";
            }
            else if (info == "listBets")
            {
                return "{\"jsonrpc\": \"2.0\",\"method\": \"SportsAPING/v1.0/listMarketCatalogue\",\"params\": { \"filter\": { \"eventIds\": [\"" + id + "\"]},\"maxResults\": \"200\",\"marketProjection\": [\"RUNNER_METADATA\"]},\"id\": 1}";
            }
            else if (info == "betInfo")
            {
                return "{\"jsonrpc\": \"2.0\",\"method\": \"SportsAPING/v1.0/listMarketBook\",\"params\": {\"marketIds\": [" + id + "],\"priceProjection\": {\"priceData\": [\"EX_BEST_OFFERS\"],  \"virtualise\": \"true\"}},\"id\": 1}";
            }
            return "error: incorrect string 'INFO'";
        }

        public static void StartMatch(string name, string date, string[] A, string json, string url)
        {
            Dictionary<string, List<String>> mapNames = new Dictionary<string, List<String>>();
            string jsonResponse = JsonRequestBetfair(json);
            MarketCatalogue marketCatalogue = JsonConvert.DeserializeObject<MarketCatalogue>(jsonResponse);
            List<Bet> bets = new List<Bet>();
            Match match = new Match(name, bets);
            match.url = url;
            match.date = date;
            if (marketCatalogue.result == null) return;
            string ids = "";
            List<string> runnerNames = new List<string>();
            for (int i = 0; i < marketCatalogue.result.Length; i++)
            {
                //double totalMatched = double.Parse(marketCatalogue.result[i].totalMatched.Replace('.', ','));
                //if (totalMatched < 10) continue;
                string a = marketCatalogue.result[i].marketName;
                a = a.Replace(A[0], "H1").Replace(A[1], "H2");
                if (!map.ContainsKey(a)) continue;
                ids += "\"" + marketCatalogue.result[i].marketId + "\"" + ",";
                mapNames[marketCatalogue.result[i].marketId] = new List<string>();
                for (int j = 0; j < marketCatalogue.result[i].runners.Length; j++)
                    mapNames[marketCatalogue.result[i].marketId].Add(marketCatalogue.result[i].runners[j].runnerName);
                continue;
                json = GetJson("betInfo", marketCatalogue.result[i].marketId);
                jsonResponse = JsonRequestBetfair(json);
                BetInfo betInfo = JsonConvert.DeserializeObject<BetInfo>(jsonResponse);
                if (betInfo.result == null) continue;
                for (int j = 0; j < betInfo.result[0].runners.Length; j++)
                {
                    if (betInfo.result[0].runners[j].ex.availableToBack.Length == 0 ||
                        betInfo.result[0].runners[j].ex.availableToLay.Length == 0) continue;

                    if (!map.ContainsKey(marketCatalogue.result[i].runners[j].runnerName))
                    {

                        if (A.Length == 2)
                        {
                            string curr = marketCatalogue.result[i].runners[j].runnerName;
                            if (curr.Contains(A[0])) curr = curr.Replace(A[0], "H1");
                            else if (curr.Contains(A[1])) curr = curr.Replace(A[1], "H2");
                            else continue;
                            if (!map.ContainsKey(curr)) continue;
                            Bet bet2 = new Bet(map[curr], 1);
                            bet2.tagName = marketCatalogue.result[i].marketName;
                            double priceBack2 = double.Parse(betInfo.result[0].runners[j].ex.availableToBack[0].price);
                            //if (priceBack2 < 0) priceBack2 = -100.0 / priceBack2;
                            //else priceBack2 = priceBack2 / 100.0;
                            //bet2.coef = 1 + (priceBack2 - 1) * 0.95;
                            bet2.coef = priceBack2;
                            bets.Add(bet2);

                        }
                        continue;
                    }
                    if (!marketCatalogue.result[i].marketName.Contains("Over/Under") || marketCatalogue.result[i].marketName.Length != 20) continue;
                    Bet bet = new Bet(map[marketCatalogue.result[i].runners[j].runnerName], 1);
                    bet.tagName = marketCatalogue.result[i].marketName;
                    double priceBack = double.Parse(betInfo.result[0].runners[j].ex.availableToBack[0].price);
                    /*if (priceBack < 0) priceBack = -100.0 / priceBack;
                    else priceBack = priceBack / 100.0;
                    bet.coef = 1 + (priceBack - 1) * 0.95;*/
                    bet.coef = priceBack;
                    bets.Add(bet);
                }
            }
            if (ids.Length < 4) return;
            ids = ids.Substring(0, ids.Length - 1);
            json = GetJson("betInfo", ids);
            jsonResponse = JsonRequestBetfair(json);
            JObject jsonOb = JObject.Parse(jsonResponse);
            foreach(var jsn in jsonOb["result"])
            {
                Result result = JsonConvert.DeserializeObject<Result>(jsn.ToString());
                for(int i = 0; i < result.runners.Count(); i++)
                {
                    string a = mapNames[result.marketId][i].Replace(A[0], "H1").Replace(A[1], "H2");
                    if (!map.ContainsKey(a) || result.runners[i].ex.availableToBack.Length == 0) continue;
                    Bet bet = new Bet(map[a], (double.Parse(result.runners[i].ex.availableToBack[0].price) - 1.0) * 0.95 + 1);
                    bets.Add(bet);
                }
            }
            match.listOfBets = bets;
            matches.Add(match);
        }
        static bool isSimilar(String[] A, String[] B)
        {
            for (int i = 0; i < 2; i++)
            {
                A[i] = A[i].ToLower().Replace(" ", "").Trim();
                B[i] = B[i].ToLower().Replace(" ", "").Trim();
            }
            double sum, min;
            sum = (A[0].DiceCoefficient(B[0]) + A[1].DiceCoefficient(B[1]));
            min = Math.Min(A[0].DiceCoefficient(B[0]), A[1].DiceCoefficient(B[1]));
            return (sum >= 1.2 && min >= 0.4);
        }
        public static void Init(Bookmaker book)
        {
            foreach (var match in book.listOfMatches)
                tmpMatches.Add(match);
        }
        public static void Start()
        {
            Console.WriteLine("Betfair start: " + DateTime.Now.ToLongTimeString());
            Console.WriteLine(tmpMatches.Count());


            map.Add("Over/Under 0.5 Goals", "Over/Under 0.5 Goals");
            map.Add("Over/Under 1.5 Goals", "Over/Under 1.5 Goals");
            map.Add("Over/Under 2.5 Goals", "Over/Under 2.5 Goals");
            map.Add("Over/Under 3.5 Goals", "Over/Under 3.5 Goals");
            map.Add("Over/Under 4.5 Goals", "Over/Under 4.5 Goals");
            map.Add("Over/Under 5.5 Goals", "Over/Under 5.5 Goals");
            map.Add("Over/Under 6.5 Goals", "Over/Under 6.5 Goals");

            map.Add("Over 0.5 Goals", "Total Over 0.5");
            map.Add("Over 1.5 Goals", "Total Over 1.5");
            map.Add("Over 2.5 Goals", "Total Over 2.5");
            map.Add("Over 3.5 Goals", "Total Over 3.5");
            map.Add("Over 4.5 Goals", "Total Over 4.5");
            map.Add("Over 5.5 Goals", "Total Over 5.5");
            map.Add("Over 6.5 Goals", "Total Over 6.5");
            map.Add("Over 7.5 Goals", "Total Over 7.5");
            map.Add("Under 0.5 Goals", "Total Under 0.5");
            map.Add("Under 1.5 Goals", "Total Under 1.5");
            map.Add("Under 2.5 Goals", "Total Under 2.5");
            map.Add("Under 3.5 Goals", "Total Under 3.5");
            map.Add("Under 4.5 Goals", "Total Under 4.5");
            map.Add("Under 5.5 Goals", "Total Under 5.5");
            map.Add("Under 6.5 Goals", "Total Under 6.5");
            map.Add("Under 7.5 Goals", "Total Under 7.5");

            map.Add("H1 -4", "H1 -4.5");
            map.Add("H1 -3", "H1 -3.5");
            map.Add("H1 -2", "H1 -2.5");
            map.Add("H1 -1", "H1 -1.5");
            map.Add("H1 0", "H1 -0.5");
            map.Add("H1 +1", "H1 +0.5");
            map.Add("H1 +2", "H1 +1.5");
            map.Add("H1 +3", "H1 +2.5");
            map.Add("H1 +4", "H1 +3.5");

            map.Add("H2 -4", "H2 -4.5");
            map.Add("H2 -3", "H2 -3.5");
            map.Add("H2 -2", "H2 -2.5");
            map.Add("H2 -1", "H2 -1.5");
            map.Add("H2 0", "H2 -0.5");
            map.Add("H2 +1", "H2 +0.5");
            map.Add("H2 +2", "H2 +1.5");
            map.Add("H2 +3", "H2 +2.5");
            map.Add("H2 +4", "H2 +3.5");


            bookmaker = new Bookmaker("betfair", matches);
            string json = GetJson("listSports", "4");
            string jsonResponse = JsonRequestBetfair(json);
            ListEventTypes listSports = JsonConvert.DeserializeObject<ListEventTypes>(jsonResponse);
            for (int k = 0; k < 1/*listSports.result.Length*/; k++)
            {
                json = GetJson("listMatches", listSports.result[k].eventType.id);
                jsonResponse = JsonRequestBetfair(json);
                jsonResponse = jsonResponse.Replace("event", "event_");
                ListMatches listMatches = JsonConvert.DeserializeObject<ListMatches>(jsonResponse);
                for (int ind = 0; ind < listMatches.result.Length; ind++)
                {
                    int cnt = 0;
                    string name = listMatches.result[ind].event_.name, date = "";
                    string[] A = listMatches.result[ind].event_.name.Split(new string[] { " v " }, StringSplitOptions.None);
                    if (A.Length != 2) continue;
                    if (listMatches.result[ind].event_.openDate != null)
                    {
                        string tmp = listMatches.result[ind].event_.openDate;
                        if (tmp.Length > 9) date = tmp[8].ToString() + tmp[9].ToString() + "/" + tmp[5] + tmp[6];
                    }
                    foreach(var match in tmpMatches)
                    {
                        string[] B = match.matchName.Split(new string[] { " v " }, StringSplitOptions.None);
                        string tmpDate = match.date;
                        if (date == tmpDate && isSimilar(A, B)) cnt++;
                    }
                    if (cnt == 0) continue;
                    string url = "betfair.com/exchange/plus/football/event/" + listMatches.result[ind].event_.id;
                    json = GetJson("listBets", listMatches.result[ind].event_.id);
                    StartMatch(name, date, A, json, url);
                    //Task task = Task.Run(() => StartMatch(name, date, A, json));
                   // break;
                }
                Console.WriteLine("END " + DateTime.Now.ToLongTimeString());
                bookmaker.listOfMatches = matches;
                Console.WriteLine("Betfair end: " + DateTime.Now.ToLongTimeString());
                Console.WriteLine("matches count: " + matches.Count());
            }
        }
        public static Bookmaker GetBetfair()
        {
            return bookmaker;
        }
        struct ListEventTypes
        {
            public String jsonrpc;
            public ListItem[] result;
            public String id;

            public struct ListItem
            {
                public string marketCount;
                public EventType eventType;
            }
            public struct EventType
            {
                public string id;
                public string name;
            }
        }
        struct MarketCatalogue
        {
            public string jsonrpc;
            public BetInfo[] result;
            public string id;

            public struct RunnerName
            {
                public string runnerName;
            }
            public struct BetInfo
            {
                public string marketId;
                public string marketName;
                public string totalMatched;
                public RunnerName[] runners;
            }
        }
        struct ListMatches
        {
            public string jsonrpc;
            public Match[] result;
            public string id;

            public struct Match
            {
                public Event event_;
                public string marketCount;
            }
            public struct Event
            {
                public string id;
                public string name;
                public string timezone;
                public string openDate;
            }
        }

        struct BetInfo
        {
            public Result[] result;
        }
        public struct Result
        {
            public string marketId;
            public string status;
            public Runners[] runners;
            public struct Runners
            {
                public string status;
                public string selectionId;
                public Ex ex;
            }
            public struct Ex
            {
                public Bet[] availableToBack;
                public Bet[] availableToLay;
            }
            public struct Bet
            {
                public string price;
                public string size;
            }

        }

        struct BetfairMatch
        {
            public string name;
            public string tag;
            public List<BetfairBet> listOfBets;
        }
        public struct BetfairBet
        {
            public string marketName;
            public string name;
            public string marketId;
            public string selectionId;
            public double handicap;
            public double priceBack;
            public double priceLay;
        }


    }
}
