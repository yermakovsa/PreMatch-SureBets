using bet.Data;
using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace bet.Functions
{
    class Parimatch
    {
        public static Bookmaker bookmaker;

        static double ConvertToDecimial(double coef)
        {
            if (coef < 0) coef = -100.0 / coef;
            else coef = coef / 100.0;
            return Math.Round(coef, 3);

        }
        static List<Bet> Parse(string s, string name)
        {

            List<Bet> bets = new List<Bet>();
            s = s.Replace("&nbsp;", "");
            s = s.Replace(" ", "");
            s = Regex.Replace(s, @"\t|\n|\r", "");
            int index;
            if (s.Contains("Add.totals:"))
            {
                index = s.IndexOf("Add.totals:") + 11;
                while (true)
                {
                    if (index < s.Length && s[index] == '(')
                    {
                        string totalN = "";
                        string koefB = "", koefS = "";
                        index++;
                        while (index < s.Length && s[index] != ')')
                        {
                            totalN += s[index];
                            index++;
                        }
                        index += 5;
                        while (index < s.Length && s[index] != ';')
                        {
                            koefB += s[index];
                            index++;
                        }
                        index += 6;
                        while (index < s.Length && s[index] != ';')
                        {
                            koefS += s[index];
                            index++;
                        }
                        if (koefS.Length < 1 || koefB.Length < 1 || totalN.Length < 1) break;

                        totalN = totalN.Replace(".0", "");
                        totalN = totalN.Trim();
                        Bet bet = new Bet("Total Over " + totalN, double.Parse(koefB.Replace('.', '.')));
                        bets.Add(bet);
                        Bet bet2 = new Bet("Total Under " + totalN, double.Parse(koefS.Replace('.', '.')));
                        bets.Add(bet2);
                    }
                    else break;
                    index++;
                }
            }
            string[] A = name.Split(new string[] { " v " }, StringSplitOptions.None);
            if (A.Length != 2) return bets;
            A[0] = A[0].Replace(" ", "");
            A[1] = A[1].Replace(" ", "");
            if(s.Contains("Add.handicaps"))
            {
                index = s.IndexOf(A[0] + ":", s.IndexOf("Add.handicaps")) + A[0].Length + 1;
                while (index < s.Length && s[index] == '(')
                {
                    string handicap = "", coef = "";
                    index++;
                    while (index < s.Length && s[index] != ')')
                    {
                        handicap += s[index];
                        index++;
                    }
                    index++;
                    while (index < s.Length && s[index] != ';')
                    {
                        coef += s[index];
                        index++;
                    }
                    index++;
                    if (handicap.Length == 0 || coef.Length == 0) break;
                    handicap = handicap.Replace(".0", "");
                    Bet bet = new Bet("H1 " + handicap.Replace("–", "-"), double.Parse(coef.Replace('.', '.')));
                    bets.Add(bet);
                }
                index = s.IndexOf(A[1] + ":", s.IndexOf("Add.handicaps")) + A[1].Length + 1;
                while (index < s.Length && s[index] == '(')
                {
                    string handicap = "", coef = "";
                    index++;
                    while (index < s.Length && s[index] != ')')
                    {
                        handicap += s[index];
                        index++;
                    }
                    index++;
                    while (index < s.Length && s[index] != ';')
                    {
                        coef += s[index];
                        index++;
                    }
                    index++;
                    if (handicap.Length == 0 || coef.Length == 0) break;
                    handicap = handicap.Replace(".0", "");
                    Bet bet = new Bet("H2 " + handicap.Replace("–", "-"), double.Parse(coef.Replace('.', '.')));
                    bets.Add(bet);
                }
            }
            if(s.Contains("Add.totals"))
            {

                index = s.IndexOf(A[0] + ":", s.IndexOf("Add.totals")) + A[0].Length + 1;
                while (true)
                {
                    if (index < s.Length && s[index] == '(')
                    {
                        string totalN = "";
                        string koefB = "", koefS = "";
                        index++;
                        while (index < s.Length && s[index] != ')')
                        {
                            totalN += s[index];
                            index++;
                        }
                        index += 5;
                        while (index < s.Length && s[index] != ';')
                        {
                            koefB += s[index];
                            index++;
                        }
                        index += 6;
                        while (index < s.Length && s[index] != ';')
                        {
                            koefS += s[index];
                            index++;
                        }
                        if (koefS.Length < 1 || koefB.Length < 1 || totalN.Length < 1) break;

                        totalN = totalN.Replace(".0", "");
                        totalN = totalN.Trim();
                        Bet bet = new Bet("Total1 Over " + totalN, double.Parse(koefB.Replace('.', '.')));
                        bets.Add(bet);
                        Bet bet2 = new Bet("Total1 Under " + totalN, double.Parse(koefS.Replace('.', '.')));
                        bets.Add(bet2);
                    }
                    else break;
                    index++;
                }
                index = s.IndexOf(A[1] + ":", s.IndexOf("Add.totals")) + A[1].Length + 1;
                while (true)
                {
                    if (index < s.Length && s[index] == '(')
                    {
                        string totalN = "";
                        string koefB = "", koefS = "";
                        index++;
                        while (index < s.Length && s[index] != ')')
                        {
                            totalN += s[index];
                            index++;
                        }
                        index += 5;
                        while (index < s.Length && s[index] != ';')
                        {
                            koefB += s[index];
                            index++;
                        }
                        index += 6;
                        while (index < s.Length && s[index] != ';')
                        {
                            koefS += s[index];
                            index++;
                        }
                        if (koefS.Length < 1 || koefB.Length < 1 || totalN.Length < 1) break;

                        totalN = totalN.Replace(".0", "");
                        totalN = totalN.Trim();
                        Bet bet = new Bet("Total2 Over " + totalN, double.Parse(koefB.Replace('.', '.')));
                        bets.Add(bet);
                        Bet bet2 = new Bet("Total2 Under " + totalN, double.Parse(koefS.Replace('.', '.')));
                        bets.Add(bet2);
                    }
                    else break;
                    index++;
                }
            }
            //for (int i = 0; i < bets.Count(); i++)
              //  bets[i].coef = ConvertToDecimial(bets[i].coef);
            return bets;
        }
        static string GetParimatch(string name, string date)
        {
            string[] names = name.Split(new string[] { " v " }, StringSplitOptions.None);
            return "//*[contains(text(),\"" + date + "\")]/following-sibling::td/a[text()[contains(.,\"" + names[0].Trim() + "\")] and text()[contains(.,\"" + names[names.Length - 1].Trim() + "\")]]/parent::td/parent::tr/parent::tbody/following-sibling::tbody[1]";
        }
        static string GetParimatchS(string name, string date)
        {
            string[] names = name.Split(new string[] { " v " }, StringSplitOptions.None);
            return "//*[contains(text(),\"" + date + "\")]/following-sibling::td/a[text()[contains(.,\"" + names[0].Trim() + "\")] and text()[contains(.,\"" + names[names.Length - 1].Trim() + "\")]]/parent::td/parent::tr/td/u/a";
        }
        public static void Start()
        {
            Console.WriteLine("Parimatch start: " + DateTime.Now.ToLongTimeString());

            string pathToFile = AppDomain.CurrentDomain.BaseDirectory + '\\';

            IWebDriver browser;
            ChromeOptions options = new ChromeOptions();
            options.PageLoadStrategy = PageLoadStrategy.Eager;
            options.AddArguments("headless");
            browser = new ChromeDriver(pathToFile, options, TimeSpan.FromSeconds(300));
            //browser.Manage().Window.Maximize();
            System.Threading.Thread.Sleep(2000);
            Random rand = new Random();
            List<IWebElement> elements = new List<IWebElement>();
            browser.Navigate().GoToUrl("https://parimatch.com/en/");
            elements = browser.FindElements(By.XPath("//em[@onclick='checkSport(event, this);']/parent::a")).ToList();
            List<String> tagNames = new List<string>();
            for (int i = 0; i < elements.Count(); i++)
            {
                tagNames.Add(elements[i].Text);
            }
            elements = browser.FindElements(By.XPath("//em[@onclick='checkSport(event, this);']")).ToList();
            for (int i = 0; i < elements.Count(); i++)
            {
                if (tagNames[i].Contains("Football/Stats") || tagNames[i].Contains("Football/Futures") ||
                    tagNames[i].Contains("Entertainment") || tagNames[i].Contains("Lottery") ||
                    tagNames[i].Contains("Politics") || tagNames[i].Contains("Specials")) continue;
                elements[i].Click();
                System.Threading.Thread.Sleep(rand.Next(500, 2000));
                break;
            }
            System.Threading.Thread.Sleep(10000);
            elements = browser.FindElements(By.XPath("//button[@onclick='openSelectedSports();']")).ToList();
            elements[0].Click();
            System.Threading.Thread.Sleep(20000);

            elements = browser.FindElements(By.XPath("//button[@onclick='switchProps();']")).ToList();
            elements[0].Click();

            Thread.Sleep(20000);
            IJavaScriptExecutor js = (IJavaScriptExecutor)browser;
            string html = (string)js.ExecuteScript("return document.documentElement.outerHTML");
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            HtmlNode[] nodes = htmlDoc.DocumentNode.SelectNodes("//td[@class='l']/a").ToArray();
            List<Data.Match> matches = new List<Data.Match>();

            //browser.Quit();

            Console.WriteLine(nodes.Length + " - nodes len");
            for (int i = 0; i < nodes.Length; i++)
            {
                Data.Match match = new Data.Match("1", null);
                string str = nodes[i].InnerHtml.Trim(), tmp = "";
                Boolean ch = false;
                for (int j = 0; j < str.Length; j++)
                {
                    if (str[j] == '<')
                    {
                        if (j + 3 < str.Length && str[j + 1] == 'b' && str[j + 2] == 'r' && str[j + 3] == '>')
                        {
                            ch = false;
                            j += 3;
                            if (j + 1 < str.Length) tmp += " v ";
                        }
                        else ch = true;
                    }
                    else if (str[j] == '>') ch = false;
                    else if (!ch) tmp += str[j];
                }
                match.matchName = tmp;
                matches.Add(match);
            }
            nodes = htmlDoc.DocumentNode.SelectNodes("//td[@class='l']/a/parent::td/preceding-sibling::td[1]").ToArray();
            for (int i = 0; i < nodes.Length; i++)
            {
                string datecurr = "";
                string dateHtml = nodes[i].InnerText;
                for (int j = 0; j < dateHtml.Length - 5; j++)
                {
                    datecurr += dateHtml[j];
                }
                matches[i].date = datecurr.Trim();
            }
            for (int i = 0; i < matches.Count(); i++)
            {

                List<Bet> bets = new List<Bet>();
                if (htmlDoc.DocumentNode.SelectNodes(GetParimatch(matches[i].matchName, matches[i].date)) == null)
                {
                    matches[i].listOfBets = bets;
                    continue;
                }
                else
                    nodes = htmlDoc.DocumentNode.SelectNodes(GetParimatch(matches[i].matchName, matches[i].date)).ToArray();
                if (nodes.Count() == 0)
                {
                    matches[i].listOfBets = bets;
                    continue;
                }
                //Console.WriteLine(matches[i].matchName + " " + matches[i].date);
                matches[i].listOfBets = Parse(nodes[0].InnerText, matches[i].matchName);
            }

            bookmaker = new Bookmaker("parimatch", matches);
            Console.WriteLine("Parimatch end: " + DateTime.Now.ToLongTimeString());

        }
        public static string GetName(string str)
        {
            Data.Match match = new Data.Match("1", null);
            string tmp = "";
            Boolean ch = false;
            for (int j = 0; j < str.Length; j++)
            {
                if (str[j] == '<')
                {
                    if (j + 3 < str.Length && str[j + 1] == 'b' && str[j + 2] == 'r' && str[j + 3] == '>')
                    {
                        ch = false;
                        j += 3;
                        if (j + 1 < str.Length) tmp += " v ";
                    }
                    else ch = true;
                }
                else if (str[j] == '>') ch = false;
                else if (!ch) tmp += str[j];
            }
            return tmp;
        }
        public static string GetDate(string dateHtml)
        {
            string datecurr = "";
            for (int j = 0; j < dateHtml.Length - 5; j++)
            {
                datecurr += dateHtml[j];
            }
            return datecurr.Trim();
        }
        static Dictionary<string, int> used = new Dictionary<string, int>();
        public static void Start(string html)
        {
            Console.WriteLine("Parimatch start: " + DateTime.Now.ToLongTimeString());

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            List<Data.Match> matches = new List<Data.Match>();

            HtmlNode[] tbody = htmlDoc.DocumentNode.SelectNodes("//tbody[@id]").ToArray(),
                       tbodyPrev = htmlDoc.DocumentNode.SelectNodes("//tbody[@id]/preceding-sibling::tbody[1]").ToArray();
            for(int i = 0; i < tbody.Length; i++)
            {
                try
                {
                    HtmlDocument curr = new HtmlDocument();
                    curr.LoadHtml(tbodyPrev[i].InnerHtml);
                    HtmlNode currNode = curr.DocumentNode.SelectSingleNode("//tr/td[@class='l']/a");
                    string name = GetName(currNode.InnerHtml.Trim());
                    currNode = curr.DocumentNode.SelectSingleNode("//tr/td[2]");
                    string date = GetDate(currNode.InnerText);
                    List<Bet> bets = Parse(tbody[i].InnerText, name);
                    Data.Match match = new Data.Match(name, bets);
                    match.date = date;
                    matches.Add(match);
                    used[name] = 1;
                }
                catch (Exception e){
                }
            }
            int cnt = 0;
            HtmlNode[] nodes = htmlDoc.DocumentNode.SelectNodes("//tr[@class='bk']").ToArray();
            for(int i = 0; i < nodes.Count(); i++)
            {
                try
                {
                    HtmlDocument curr = new HtmlDocument();
                    curr.LoadHtml(nodes[i].InnerHtml);
                    HtmlNode currNode = curr.DocumentNode.SelectSingleNode("//td[@class='l']");
                    
                    string name = GetName(currNode.InnerHtml.Trim());
                    if (used.ContainsKey(name)) continue;
                    currNode = curr.DocumentNode.SelectSingleNode("//td[2]");
                    string date = GetDate(currNode.InnerText);
                    List<Bet> bets = new List<Bet>();
                    HtmlNode[] tmp = curr.DocumentNode.SelectNodes("//td").ToArray();
                    double coef = double.Parse(tmp[tmp.Length - 2].InnerText),
                           coef2 = double.Parse(tmp[tmp.Length - 1].InnerText);
                    bets.Add(new Bet("1", coef));
                    bets.Add(new Bet("2", coef2));

                    Data.Match match = new Data.Match(name, bets);
                    match.date = date;
                    matches.Add(match);
                    cnt++;
                }
                catch(Exception e)
                {
                }
            }
            Console.WriteLine(matches.Count + " " + cnt);
            //browser.Quit();

  
            bookmaker = new Bookmaker("parimatch", matches);
            Console.WriteLine("Parimatch end: " + DateTime.Now.ToLongTimeString());
            Console.WriteLine("mathces count: " + matches.Count());
        }
        public static Bookmaker GetParimatch()
        {
            return bookmaker;
        }
    }
}
