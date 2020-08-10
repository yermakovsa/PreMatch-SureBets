using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Http;
using Titanium.Web.Proxy.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Xml.Serialization;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Threading;
using System.Runtime.InteropServices;
using bet.Functions;
using bet.Data;
using DuoVia.FuzzyStrings;
using Org.BouncyCastle.Pkix;
using OpenQA.Selenium.Remote;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;

namespace bet
{
    class Program
    {
        static PServer pServer = new PServer();

        static string pathToFile;

        static Dictionary<string, int> used = new Dictionary<string, int>();
        
        
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

        static bool isOpposite(string a, string b)
        {
            if (a.Contains("Over") && a.Replace("Over", "Under") == b)
            {
                return true;
            }
            else if (a.Contains("Under") && a.Replace("Under", "Over") == b)
            {
                return true;
            }
            else if((a == "1" && b == "2") || (a == "2" && b == "1"))
            {
                return true;
            }
            else
            {
                if (a.Contains("H1"))
                {
                    if (a.Contains("+") && a.Replace("H1", "H2").Replace("+", "-") == b)
                    {
                        return true;
                    }
                    else if (a.Replace("H1", "H2").Replace("-", "+") == b)
                    {
                        return true;
                    }
                }
                else if (a.Contains("H2"))
                {
                    if (a.Contains("+") && a.Replace("H2", "H1").Replace("+", "-") == b)
                    {
                        return true;
                    }
                    else if (a.Replace("H2", "H1").Replace("-", "+") == b)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        static void sendMessage(string text)
        {
            string urlString = "https://api.telegram.org/bot{0}/sendMessage?chat_id={1}&text={2}";
            string apiToken = "950038087:AAEoJY_FOlnmuBmr6U-sG6BbpVKI5yfoS8o";
            string chatId = "-1001493789616";
            urlString = String.Format(urlString, apiToken, chatId, text);
            WebRequest request = WebRequest.Create(urlString);
            Stream rs = request.GetResponse().GetResponseStream();
            /*StreamReader reader = new StreamReader(rs);
            string line = "";
            StringBuilder sb = new StringBuilder();  urlString "https://api.telegram.org/bot950038087:AAEoJY_FOlnmuBmr6U-sG6BbpVKI5yfoS8o/sendMessage?chat_id=-1001493789616&text=" string

            while (line != null)
            {
                line = reader.ReadLine();
                if (line != null)
                    sb.Append(line);
            }
            string response = sb.ToString();*/
            // Do what you want with response
        }

        static LiveMatch liveParse(string s)
        {
            LiveMatch match = new LiveMatch();
            JObject json = JObject.Parse(s);
            JToken value = json["eventTypes"][0]["eventNodes"][0]["marketNodes"];
            foreach (JToken token in value)
            {
                string marketId = token["marketId"].ToString();
                foreach(JToken bet in token["runners"])
                {
                    match.add(marketId, bet["selectionId"].ToString(), bet["exchange"]["availableToBack"][0]["price"].ToString(), bet["exchange"]["availableToBack"][0]["size"].ToString());
                }
            }
            return match;
        }

        static void Main(string[] args)
        {
            Console.WriteLine("START " + DateTime.Now.ToLongTimeString());
            //Thread.Sleep(10000);
            //Environment.Exit(0);
            pServer.Start();

            string pathToFile = AppDomain.CurrentDomain.BaseDirectory + '\\';

            
            //IWebDriver driver = new RemoteWebDriver(DesiredCapabilities.HtmlUnit());

            /*string html = @"<td><span class='no'>4713</span></td><td><!--evd 13/07/20 14:00 evd-->13/07<br>14:00</td><td class='l'>Salomon Daniyel<br>Brozhik Karel</td><td><u><a id='r7227_176_m3K_4V6QQbahIIxnuzhJ'>1.76</a></u></td><td><u><a id='r7227_200_jQ31g4NgZMRQcLd39Eu ^ '>2.00</a></u>";
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);
            HtmlNode node = htmlDocument.DocumentNode.SelectSingleNode("//td[@class='l']");
            Console.WriteLine(node.InnerHtml);
            Console.ReadLine();*/
            List<string> vilki = new List<string>();
            //string[] lines = System.IO.File.ReadAllLines(@"C:\Users\MainUser\Documents\tennis\bet\bet\bin\Debug\netcoreapp3.1\vilki.txt");
            //for (int i = 0; i < lines.Count(); i++)
              //  used[lines[i]] = 1;

            //ProxyList proxyList = new ProxyList();
            //proxyList.Start();
            //Task task1 = Task.Run(Pinnacle.Start);
            //Task task2 = Task.Run(Betfair.Start);
           
            //Task task3 = Task.Run(Xbet.Start);
            //task1.Wait();
            //task2.Wait();
            //task3.Wait();
            Pinnacle.Start();
            //Thread.Sleep(4000);
            //Betfair.Start(Pinnacle.GetPinnacle());
            Thread.Sleep(2000);
            Betfair.Init(Pinnacle.GetPinnacle());
            Betfair.Start();
            List<Bookmaker> bookmakers = new List<Bookmaker>();
            bookmakers.Add(Pinnacle.GetPinnacle());

            bookmakers.Add(Betfair.GetBetfair());
            //bookmakers.Add(Xbet.GetXbet());
            foreach(Bookmaker bookmaker in bookmakers)
            {
                Console.WriteLine(bookmaker.name);
                foreach(Match match in bookmaker.listOfMatches)
                {
                    match.matchName = match.matchName.Replace("Gaming", "").Replace("Academy", "").Replace("Esports", "").Replace("eSports", "");
                    /*Console.WriteLine(match.matchName + " " + match.date);
                    foreach(Bet bet in match.listOfBets)
                    {
                        Console.WriteLine(bet.name + " " + bet.coef);
                    }*/
                }
            }
            //Console.ReadLine();
            for (int kk = 0; kk < 10; kk++)
                Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            Console.WriteLine("count: " + bookmakers.Count());
            string res = "";
            for (int i = 0; i < bookmakers.Count; i++)
            {
             //   res += bookmakers[i].name + " " + bookmakers[i].listOfMatches.Count() + "\n";
                Console.WriteLine(bookmakers[i].name + " " + bookmakers[i].listOfMatches.Count());
            }
         
            using (StreamWriter sw = new StreamWriter("anssss.txt", true, System.Text.Encoding.Default))
            {
                for (int i = 0; i < bookmakers.Count; i++)
                {
                    for (int j = i + 1; j < bookmakers.Count; j++)
                    {
                        Bookmaker bk1 = bookmakers[i];
                        Bookmaker bk2 = bookmakers[j];
                        int cnt = 0, cnt2 = 0;
                        foreach (Match match1 in bk1.listOfMatches)
                        {
                            foreach (Match match2 in bk2.listOfMatches)
                            {

                                if (!match1.date.Equals(match2.date) && bk1.name != "betfairSB" && bk2.name != "betfairSB") continue;
                                if (bk1.name == "pinnacle" && DateTime.Now.Subtract(match1.dateTime).TotalMinutes > 0) continue;
                                //if (match1.dateTime.Subtract(match2.dateTime).TotalMinutes > 45) continue;
                                string[] A = match1.matchName.Split(new string[] { " v " }, StringSplitOptions.None);
                                string[] B = match2.matchName.Split(new string[] { " v " }, StringSplitOptions.None);
                                if (A.Length != 2 || B.Length != 2) continue;
                                if (!isSimilar(A, B)) continue;
                                ++cnt;


                                foreach (Bet bet1 in match1.listOfBets)
                                {
                                    foreach (Bet bet2 in match2.listOfBets)
                                    {
                                        if (isOpposite(bet1.name, bet2.name))
                                        {
                                            cnt2++;
                                            sw.WriteLine(bet1.name + " " + bet2.name);
                                            double sum = 1.0 / bet1.coef + 1.0 / bet2.coef;
                                            double profit = 1.0 / sum - 1.0;
                                            string a = bk1.name + bk2.name + match1.matchName + bet1.name + bet1.coef + bet2.coef;
                                            if (profit >= 0 && profit <= 0.1 && !used.ContainsKey(a))
                                            {
                                                double needF = 1.0 / bet1.coef / sum, needS = 1.0 / bet2.coef / sum;
                                                used[a] = 1;
                                                vilki.Add(a);
                                                res += bk1.name + " | " + bk2.name + "\n";
                                                res += match1.date + "\n";
                                                res += match1.matchName + " | " + match2.matchName + "\n";
                                                res += bet1.name + " | " + bet2.name + "\n";
                                                res += bet1.coef + " | " + bet2.coef + "|" + Math.Round(profit * 100, 2) + "%\n";
                                                //res += 2 * Math.Round(needF, 1) + " | " + Math.Round((2 * Math.Round(needF, 1) / needF) * needS, 3) + "\n";
                                                //res += 0.5 + " | " + Math.Round(0.5 / needF * needS, 2) + "\n";
                                                res += Math.Round(10.0 / needS * needF, 2) + " | " + 10 + "\n";
                                                //res += 1.5 + " | " + Math.Round(1.5 / needF * needS, 2) + "\n";
                                                //res += 2 + " | " + Math.Round(2.0 / needF * needS, 2) + "\n";
                                                //res += 2 * Math.Round(needF, 3) + " | " + 2 * Math.Round(needS, 3) + "\n";
                                                res += match1.url + "\n";
                                                res += match2.url + "\n";
                                                res += "_____________\n";
                                            }

                                            /*Console.WriteLine(bk1.name + " " + bk2.name);
                                            Console.WriteLine(match1.matchName + " " + match2.matchName);
                                            Console.WriteLine(bet1.name + " " + bet2.name);
                                            Console.WriteLine(bet1.coef + " " + bet2.coef + " " + profit);*/
                                        }
                                    }
                                }
                            }
                        }
                        Console.WriteLine(bk1.name + " " + bk2.name + " " + cnt + " " + cnt2);
                    }
                }
            }
            /*using (System.IO.StreamWriter sw =
                File.AppendText(@"C:\Users\MainUser\Documents\tennis\bet\bet\bin\Debug\netcoreapp3.1\vilki.txt"))
            {
                for (int i = 0; i < vilki.Count(); i++)
                    sw.WriteLine(vilki[i]);
            }*/
            if (res != null && res.Length > 4)
            {
                Console.WriteLine(res);
                sendMessage(res);
            }
            Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!END " + DateTime.Now.ToLongTimeString());
            Thread.Sleep(2000);
            Environment.Exit(0);
            //Console.ReadLine();
            //1121
        }
    }
}
