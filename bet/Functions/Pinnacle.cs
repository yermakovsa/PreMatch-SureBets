using bet.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace bet.Functions
{
    class Pinnacle
    {
        static List<Match> matches;
        public static IWebDriver driver;
        public static string pathToFile = AppDomain.CurrentDomain.BaseDirectory + '\\';
        static List<IWebElement> elements = new List<IWebElement>();
        static Random rand = new Random();
       

        
        public static Bookmaker GetPinnacle()
        {
            Bookmaker bookmaker = new Bookmaker("pinnacle", matches);
            return bookmaker;
        }
        public static void StartLocal()
        {
            Console.WriteLine("Start local " + DateTime.Now.ToLongTimeString());
            Random rnd = new Random();
            matches = new List<Match>();
            string[] arr = {"https://www.pinnacle.com/en/soccer/leagues"/*, "https://www.pinnacle.com/en/tennis/leagues", "https://www.pinnacle.com/en/table-tennis/leagues", "https://www.pinnacle.com/en/esports/leagues" */};
            foreach (string s in arr)
            {
                List<string> links = new List<string>();
                driver.Navigate().GoToUrl(s);
                Thread.Sleep(7500);
                if(s.Contains("soccer"))
                {
                    elements = driver.FindElements(By.XPath("//div[@class='contentBlock']/div/ul/li/div/a"/*"//div[@class='contentBlock']/ul/li/div/a"*/)).ToList();
                }
                else
                {
                    elements = driver.FindElements(By.XPath("//div[@class='contentBlock']/ul/li/div/a")).ToList();
                }
                if(elements.Count() >= 100)
                {
                    for(int i = 1;i < elements.Count(); i++)
                    {
                        int ind = rand.Next(0, i);
                        var tmp = elements[ind];
                        elements[ind] = elements[i];
                        elements[i] = tmp;
                        
                    }
                }
                for (int i = 0; i < Math.Min(100, elements.Count()); i++)
                {
                    links.Add(elements[i].GetAttribute("href"));
                    Console.WriteLine(elements[i].GetAttribute("href"));
                }
                for (int i = 0; i < Math.Min(100, links.Count()); i++)
                {
                    driver.Navigate().GoToUrl(links[i]);
                    Thread.Sleep(2000);
                }
            }
            Thread.Sleep(3000);
            Console.WriteLine("Pinacle end local: " + DateTime.Now.ToLongTimeString());
        }
        public static void Start()
        {
            ChromeOptions options = new ChromeOptions();
            options.PageLoadStrategy = PageLoadStrategy.None;
            options.AddArguments("--proxy-server=127.0.0.1:8000");
            //options.AddUserProfilePreference("profile.default_content_setting_values.images", 2);
            //options.AddArguments("headless");
            driver = new ChromeDriver(pathToFile, options, TimeSpan.FromSeconds(300));
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            /*driver.Navigate().GoToUrl("https://www.betfair.com/sport/table-tennis");
            Thread.Sleep(10000);
            string html = (string)js.ExecuteScript("return document.documentElement.outerHTML");
            BetfairSB.Start(html);
            driver.Navigate().GoToUrl("https://parimatch.com/en/");
            Thread.Sleep(10000);
            elements = driver.FindElements(By.XPath("//em[@onclick='checkSport(event, this);']/parent::a")).ToList();
            List<String> tagNames = new List<string>();
            for (int i = 0; i < elements.Count(); i++)
            {
                tagNames.Add(elements[i].Text);
            }
            elements = driver.FindElements(By.XPath("//em[@onclick='checkSport(event, this);']")).ToList();
            for (int i = 0; i < elements.Count(); i++)
            {
                if (tagNames[i] != "Football" && tagNames[i] != "Table tennis") continue;
                Thread.Sleep(rand.Next(500, 1000));
                elements[i].Click();
            }
            elements = driver.FindElements(By.XPath("//button[@onclick='openSelectedSports();']")).ToList();
            elements[0].Click();
            System.Threading.Thread.Sleep(15000);

            elements = driver.FindElements(By.XPath("//button[@onclick='switchProps();']")).ToList();
            elements[0].Click();

            Thread.Sleep(15000);
            html = (string)js.ExecuteScript("return document.documentElement.outerHTML");
            Parimatch.Start(html);*/
            StartLocal();
            Console.WriteLine("Pinacle end: " + DateTime.Now.ToLongTimeString());
            driver.Quit();

        }
        static double ConvertToDecimial(double coef)
        {
            if (coef < 0) coef = -100.0 / coef;
            else coef = coef / 100.0;
            return Math.Round(coef + 1, 3);

        }
        public static void Parse(string matchup, string straight)
        {
            Console.WriteLine("VNIMANIEEEEEEEEEEEEEEEE");
            List<Match> listOfMatches = new List<Match>();
            JObject json1 = JObject.Parse(matchup);
            var matchById = new Dictionary<string, Match>();
            foreach (var mtch in json1["value"])
            {
                if (mtch["parent"].ToString() == "" && mtch["isLive"].ToString() != "true")
                {
                    List<Bet> listOfBets = new List<Bet>();
                    string name = "", id;
                    id = mtch["id"].ToString();
                    var participants = mtch["participants"];
                    foreach (var participant in participants)
                    {
                        name += participant["name"] + " v ";
                    }
                    name = name.Substring(0, name.Length - 3);
                    //Console.WriteLine(id + " " + name);
                    Match match = new Match(name, listOfBets);
                    string leagueID = mtch["league"]["id"].ToString();
                    string leagueName = mtch["league"]["name"].ToString().ToLower().Replace(" ", "-").Replace("---", "-");
                    string sportName = mtch["league"]["sport"]["name"].ToString().ToLower().Replace(" ", "");
                    string tmpName = name.ToLower().Replace(" ", "-").Replace("---", "-");
                    if (sportName.Contains("tabletennis")) sportName = "table-tennis";
                    string url = "www.pinnacle.com/en/" + sportName + "/" + leagueName + "/" + tmpName + "/" + id + "/";
                    match.url = url;
                    string tmp = JsonConvert.SerializeObject(mtch["startTime"]);
                    DateTime date = new DateTime();
                    date = date.AddYears(2019);
                    //Console.WriteLine(date.ToString());
                    date = date.AddMonths(int.Parse(tmp[6].ToString() + tmp[7].ToString()) - 1);
                    date = date.AddDays(int.Parse(tmp[9].ToString() + tmp[10].ToString()) - 1);
                    date = date.AddHours(int.Parse(tmp[12].ToString() + tmp[13].ToString()));
                    date = date.AddMinutes(int.Parse(tmp[15].ToString() + tmp[16].ToString()));
                    match.dateTime = date;
                    match.date = "" + tmp[9].ToString() + tmp[10].ToString() + "/" + tmp[6].ToString() + tmp[7].ToString();
                    //match.date = tmp;
                    matchById.Add(id, match);
                    listOfMatches.Add(match);
                }
            }


            JObject json = JObject.Parse(straight);
            foreach (var bet in json["value"])
            {
                if (!matchById.ContainsKey(bet["matchupId"].ToString())) continue;
                List<Bet> listOfBets = matchById[bet["matchupId"].ToString()].listOfBets;
                if (bet["type"].ToString() == "total")
                {
                    string period = "";
                    if (bet["period"].ToString() != "0")
                    {
                        period = "Period" + bet["period"].ToString() + " ";
                    }

                    listOfBets.Add(new Bet(period + "Total Over " + bet["prices"][0]["points"].ToString().Trim().Replace(',', '.'), ConvertToDecimial(double.Parse(bet["prices"][0]["price"].ToString()))));
                    listOfBets.Add(new Bet(period + "Total Under " + bet["prices"][1]["points"].ToString().Trim().Replace(',', '.'), ConvertToDecimial(double.Parse(bet["prices"][1]["price"].ToString()))));
                }
                else if (bet["type"].ToString() == "team_total")
                {
                    string period = "";
                    if (bet["period"].ToString() != "0")
                    {
                        period = "Period" + bet["period"].ToString() + " ";
                    }

                    if (bet["side"].ToString() == "home")
                    {
                        listOfBets.Add(new Bet(period + "Total1 Over " + bet["prices"][0]["points"].ToString().Trim().Replace(',', '.'), ConvertToDecimial(double.Parse(bet["prices"][0]["price"].ToString()))));
                        listOfBets.Add(new Bet(period + "Total1 Under " + bet["prices"][1]["points"].ToString().Trim().Replace(',', '.'), ConvertToDecimial(double.Parse(bet["prices"][1]["price"].ToString()))));
                    }
                    else
                    {
                        listOfBets.Add(new Bet(period + "Total2 Over " + bet["prices"][0]["points"].ToString().Trim().Replace(',', '.'), ConvertToDecimial(double.Parse(bet["prices"][0]["price"].ToString()))));
                        listOfBets.Add(new Bet(period + "Total2 Under " + bet["prices"][1]["points"].ToString().Trim().Replace(',', '.'), ConvertToDecimial(double.Parse(bet["prices"][1]["price"].ToString()))));
                    }
                }
                else if (bet["type"].ToString() == "spread")
                {
                    string period = "";
                    if (bet["period"].ToString() != "0")
                    {
                        period = "Period" + bet["period"].ToString() + " ";
                    }

                    string tmp0 = bet["prices"][0]["points"].ToString().Trim().Replace(',', '.');
                    string tmp1 = bet["prices"][1]["points"].ToString().Trim().Replace(',', '.');
                    string betName0 = "H1 ";
                    string betName1 = "H2 ";
                    if (tmp0.Contains("-") || tmp0 == "0")
                    {
                        betName0 += tmp0;
                    }
                    else
                    {
                        betName0 += "+" + tmp0;
                    }
                    if (tmp1.Contains("-") || tmp1 == "0")
                    {
                        betName1 += tmp1;
                    }
                    else
                    {
                        betName1 += "+" + tmp1;
                    }
                    listOfBets.Add(new Bet(period + betName0, ConvertToDecimial(double.Parse(bet["prices"][0]["price"].ToString()))));
                    listOfBets.Add(new Bet(period + betName1, ConvertToDecimial(double.Parse(bet["prices"][1]["price"].ToString()))));
                }
                else if (bet["type"].ToString() == "moneyline")
                {
                    string period = "";
                    if (bet["period"].ToString() != "0")
                    {
                        period = "Period" + bet["period"].ToString() + " ";
                    }

                    Bet b1 = null;
                    Bet b2 = null;
                    bool flag = true;
                    foreach(var i in bet["prices"])
                    {
                        if (i["designation"] == null)
                        {
                            flag = false;
                            break;
                        }
                        if(i["designation"].ToString() == "home")
                        {
                            b1 = new Bet(period + "1", ConvertToDecimial(double.Parse(i["price"].ToString())));
                        }
                        else if (i["designation"].ToString() == "away")
                        {
                            b2 = new Bet(period + "2", ConvertToDecimial(double.Parse(i["price"].ToString())));
                        }
                        else
                        {
                            flag = false;
                        }
                    }
                    if(flag)
                    {
                        listOfBets.Add(b1);
                        listOfBets.Add(b2);
                    }
                }
            }
            foreach (Match match in listOfMatches)
            {
                if (match.listOfBets.Count() == 0 || match.matchName.Length > 50) continue;
                matches.Add(match);
            }
            //return listOfMatches;
        }
    }
}
