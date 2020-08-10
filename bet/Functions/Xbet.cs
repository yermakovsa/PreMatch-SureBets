using bet.Data;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using Org.BouncyCastle.Bcpg;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace bet.Functions
{
    class Xbet
    {
        public static IWebDriver driver;
        public static string pathToFile = AppDomain.CurrentDomain.BaseDirectory + '\\';
        static List<IWebElement> elements = new List<IWebElement>();
        static Random rand = new Random();
        public static List<Match> listOfMatches;
        public static string period;
        public static List<Match> Parse(string s)
        {
            //Console.WriteLine("ssss");
            List<Match> listOfMatches = new List<Match>();
            JObject json = JObject.Parse(s);
            var value = json["Value"];
            foreach (var mch in value)
            {
                List<Bet> listOfBets = new List<Bet>();
                Match match = new Match("1", listOfBets);
                match.date = "";
                JToken t2 = mch["O2"];
                string team1, team2;
                if (t2 != null)
                {
                    team1 = mch["O1"].ToString();
                    team2 = mch["O2"].ToString();
                    match.matchName = team1 + " v " + team2;
                }
                else
                {
                    team1 = mch["O1"].ToString();
                    match.matchName = team1 + " v empty";
                }
                string matchID = mch["CI"].ToString();
                string matchName = match.matchName.Replace(" v ", " ").Replace(" ", "-").Replace(".", "");
                string champID = mch["LI"].ToString();
                string champName = mch["L"].ToString().Replace(" ", "-").Replace(".", "");
                string sportName = mch["SN"].ToString();
                if (sportName.ToLower().Contains("table")) sportName = "Table-Tennis";
                champName = champName.Replace(":", "");
                string tmpUrl = "ua-1x-bet.com/en/line/" + sportName + "/" + champID + "-" + champName + "/";
                tmpUrl += matchID + "-" + matchName + "/";
                string url = "";
                foreach(char x in tmpUrl)
                {
                    if ((x >= 'a' && x <= 'z') || (x >= 'A' && x <= 'Z') || (x >= '0' && x <= '9') || 
                        x == '-' || x == '/' || x == '.' || x == ':' || x == '-') url += x;
                }
                match.url = url;
                int sec;
                sec = int.Parse(mch["S"].ToString());
                DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                dtDateTime = dtDateTime.AddSeconds(sec).ToLocalTime();
                match.dateTime = dtDateTime;
                //string date = dtDateTime.Day + "/" + dtDateTime.Month;
                string date = "";
                if (dtDateTime.Day <= 9)
                {
                    date += "0" + dtDateTime.Day + "/";
                }
                else
                {
                    date += dtDateTime.Day + "/";
                }
                if (dtDateTime.Month <= 9)
                {
                    date += "0" + dtDateTime.Month;
                }
                else
                {
                    date += dtDateTime.Month.ToString();
                }
                match.date = date;
                var bets = mch["E"];
                Bet b1 = null;
                Bet b2 = null;
                Bet b3 = null;
                foreach (var bet in bets)
                {
                    JToken tmp = bet["T"];
                    if (tmp != null)
                    {
                        if (bet["T"].ToString() == "7" || bet["T"].ToString() == "2826")
                        {
                            JToken token = bet["P"];
                            string betName = "H1 ";
                            if (token != null)
                            {
                                if (token.ToString().Contains("-"))
                                {
                                    betName += token.ToString().Trim().Replace(',', '.');
                                }
                                else
                                {
                                    betName += "+" + token.ToString().Trim().Replace(',', '.');
                                }
                            }
                            else
                            {
                                betName += "0";
                            }
                            listOfBets.Add(new Bet(period + betName, double.Parse(bet["C"].ToString())));
                        }
                        else if (bet["T"].ToString() == "8" || bet["T"].ToString() == "2827")
                        {
                            JToken token = bet["P"];
                            string betName = "H2 ";
                            if (token != null)
                            {
                                if (token.ToString().Contains("-"))
                                {
                                    betName += token.ToString().Trim().Replace(',', '.');
                                }
                                else
                                {
                                    betName += "+" + token.ToString().Trim().Replace(',', '.');
                                }
                            }
                            else
                            {
                                betName += "0";
                            }
                            listOfBets.Add(new Bet(period + betName, double.Parse(bet["C"].ToString())));
                        }
                        else if (bet["T"].ToString() == "9" || bet["T"].ToString() == "2824")
                        {
                            listOfBets.Add(new Bet(period + "Total Over " + bet["P"].ToString().Trim().Replace(',', '.'), double.Parse(bet["C"].ToString())));
                        }
                        else if (bet["T"].ToString() == "10" || bet["T"].ToString() == "2825")
                        {
                            listOfBets.Add(new Bet(period + "Total Under " + bet["P"].ToString().Trim().Replace(',', '.'), double.Parse(bet["C"].ToString())));
                        }
                        else if (bet["T"].ToString() == "11")
                        {
                            listOfBets.Add(new Bet(period + "Total1 Over " + bet["P"].ToString().Trim().Replace(',', '.'), double.Parse(bet["C"].ToString())));
                        }
                        else if (bet["T"].ToString() == "12")
                        {
                            listOfBets.Add(new Bet(period + "Total1 Under " + bet["P"].ToString().Trim().Replace(',', '.'), double.Parse(bet["C"].ToString())));
                        }
                        else if (bet["T"].ToString() == "13")
                        {
                            listOfBets.Add(new Bet(period + "Total2 Over " + bet["P"].ToString().Trim().Replace(',', '.'), double.Parse(bet["C"].ToString())));
                        }
                        else if (bet["T"].ToString() == "14")
                        {
                            listOfBets.Add(new Bet(period + "Total2 Under " + bet["P"].ToString().Trim().Replace(',', '.'), double.Parse(bet["C"].ToString())));
                        }
                        else if(bet["T"].ToString() == "1")
                        {
                            b1 = new Bet(period + "1", double.Parse(bet["C"].ToString()));
                        }
                        else if (bet["T"].ToString() == "2")
                        {
                            b2 = new Bet(period + "X", double.Parse(bet["C"].ToString()));
                        }
                        else if (bet["T"].ToString() == "3")
                        {
                            b3 = new Bet(period + "2", double.Parse(bet["C"].ToString()));
                        }
                    }
                }
                if(b1 != null && b2 == null && b3 != null)
                {
                    listOfBets.Add(b1);
                    listOfBets.Add(b3);
                }
                listOfMatches.Add(match);
            }
            return listOfMatches;
        }
        public static string Request(string url)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.Method = "GET";
            httpWebRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.97 Safari/537.36";
            //   httpWebRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9";
            // httpWebRequest.Headers.Add("accept-encoding", "gzip, deflate, br");
            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var responseText = streamReader.ReadToEnd();
                return responseText;
            }
        }
        static string getXbetUrl(string champ, string sportId, string period)
        {
            string url = "https://1xbet.com/LineFeed/Get1x2_VZip?champs={0}&count=100&lng=en&mode=7&typeGames={1}";
            url = String.Format(url, champ, period);
            return url;
        }

        public static int cnt(string i)
        {
            if(i == "40")
            {
                return 3;
            }
            return 0;
        }

        public static void StartLocal()
        {
            // football - 1, 4 - tennis, 10 - table tennis
            // esport - 40
            string[] arr = { /*"1",*/"4","10","40"};
            Random rand = new Random();
            foreach (string id in arr)
            {
                string s = Request("https://1xbet.com/LineFeed/GetSportsShortZip?sports=" + id + "&lng=en&tf=2200000&tz=0&country=76&virtualSports=true&group=70");
                JObject json = JObject.Parse(s);
                var value = json["Value"];
                foreach (var elem in value)
                {
                    if (elem["L"] != null)
                    {
                        var matches = elem["L"];
                        Console.WriteLine("matches count: " + matches.Count());
                        foreach (var match in matches)
                        {
                            if (match["L"].ToString().ToLower().Contains("fifa")) continue;
                            for (int per = 0; per <= cnt(id); ++per)
                            {
                                Console.WriteLine(getXbetUrl(match["LI"].ToString(), "1", per.ToString()));
                                Thread.Sleep(rand.Next(750, 1250));
                                if(per == 0)
                                {
                                    period = "";
                                }
                                else
                                {
                                    period = "Period" + per.ToString() + " ";
                                }
                                List<Match> LocalLitOfMatches = Parse(Request(getXbetUrl(match["LI"].ToString(), "1", per.ToString())));
                                if (LocalLitOfMatches.Count() == 0) break;
                                Console.WriteLine("list of matches count: " + LocalLitOfMatches.Count());
                                //if (LocalLitOfMatches.Count() == 0) break;
                                foreach (Match tmp in LocalLitOfMatches)
                                {
                                    listOfMatches.Add(tmp);
                                }
                            }
                        }
                    }
                }
            }
            /*IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            Thread.Sleep(1000);
            for (int i = 0; i < 3; i++)
                Console.WriteLine("VNIMANIE !XBET IS ABOUT TO START");
            Console.WriteLine("1xbet start: " + DateTime.Now.ToLongTimeString());
            driver.Navigate().GoToUrl("https://1xbet.com/us/line/Football/");

            Thread.Sleep(30000);
            elements = driver.FindElements(By.XPath("//a[@href='line/Football/']/div[@class='b-filters__sport-name']")).ToList();
            Actions action = new Actions(driver);
            action.MoveToElement(elements[0]).Perform();
            Thread.Sleep(1000);
            List<string> links = new List<string>();
            elements = driver.FindElements(By.XPath("//div[@class='b-filters__leagues']/div/div/a")).ToList();
            for (int i = 0; i < elements.Count(); i++)
            {
                string s = elements[i].GetAttribute("href");
                if (s.ToLower().Contains("special") || s.ToLower().Contains("statistic") ||
                    s.ToLower().Contains("fifa")) continue;
                links.Add(s);
            }
            for (int j = 0; j < 20; j++)
            {
                js.ExecuteScript("window.open()");
            }
            List<String> tabs = new List<String>(driver.WindowHandles);
            for (int j = 0; j < 20; j++)
            {
                driver.SwitchTo().Window(tabs[20 - j]);
                driver.Navigate().GoToUrl("https://www.york.ac.uk/teaching/cws/wws/webpage1.html");
            }
            driver.SwitchTo().Window(tabs[0]);
            driver.Navigate().GoToUrl(links[0]);
            Thread.Sleep(5000);
            for (int i = 1; i < Math.Min(100, links.Count()); i++)
            {
                Console.WriteLine("VNIMANIE|||||||||||||||||||||||||||||||||||||||||| LINK: " + links[i - 1]);
                try
                {
                    driver.SwitchTo().Window(tabs[0]);
                    Thread.Sleep(10000);
                    elements = driver.FindElements(By.XPath("//a[@class='c-events__name']")).ToList();
                    List<string> linksMatches = new List<string>();
                    for (int j = 0; j < elements.Count; j++)
                    {
                        linksMatches.Add(elements[j].GetAttribute("href"));
                        Console.WriteLine("!!!!!!!!!!!!!!!!!!!!BETFORE LINK:" + elements[j].GetAttribute("href"));
                    }
                    driver.Navigate().GoToUrl(links[i]);
                    int curr = Math.Min(elements.Count(), rand.Next(20, 20));
           
                    List<String> tabs = new List<String>(driver.WindowHandles);
                    //int curr = 5;
                    for (int j = 0; j < curr; j++)
                    {
                        //Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!LINK: " + linksMatches[j]);
                        Console.WriteLine("CurrLinkOpen: " + DateTime.Now.ToLongTimeString());
                        driver.SwitchTo().Window(tabs[curr - j]);
                        driver.Navigate().GoToUrl(linksMatches[j] );

                        Thread.Sleep(rand.Next(500, 500));
                    }
                    Thread.Sleep(5000);
                    //Thread.Sleep(Math.Max(10000, curr * 1000));
                    for (int j = 0; j < curr; j++)
                    {
                        driver.SwitchTo().Window(tabs[curr - j]);
                        Thread.Sleep(rand.Next(3500, 3500));
                        driver.Navigate().GoToUrl("https://www.york.ac.uk/teaching/cws/wws/webpage1.html");
                    }
                    
                }
                catch (Exception e)
                {
                    for (int kk = 0; kk < 5; kk++)
                        Console.WriteLine("VNIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIMAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAANOIIIIIIIIIIIIIIIIIIIIEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE: exception and reboot");
                    Console.WriteLine(e.Message);
                    for (int kk = 0; kk < 5; kk++)
                        Console.WriteLine("VNIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIMAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAANOIIIIIIIIIIIIIIIIIIIIEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE: exception and reboot");

                    Thread.Sleep(10000);
                    //return;
                    
                }
                //break;
            }
            Thread.Sleep(10000);
            */
            Console.WriteLine("1xbet end local: " + DateTime.Now.ToLongTimeString());
            Console.WriteLine("LIST: " + listOfMatches.Count());
        }
        public static void Start()
        {
            listOfMatches = new List<Match>();
            
      
            
            StartLocal(); 
            Console.WriteLine("1xbet end: " + DateTime.Now.ToLongTimeString());
            //Parimatch.Start(html);
            //Task task = Task.Run(Parimatch.Start(html));
            //driver.Manage().Window.Maximize();
            //PServer.SetExternalProxy("212.42.113.96", 3128);

        }
        public static Bookmaker GetXbet()
        {
            Bookmaker bookmaker = new Bookmaker("xbet", listOfMatches);
            return bookmaker;
        }
    }
    
}
