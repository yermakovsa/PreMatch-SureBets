using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace bet.Functions
{
    class ProxyList
    {
        List<string> listOfProxies;

        public ProxyList()
        {
            listOfProxies = new List<string>();
        }
        void StartWebRequest(string url, string proxy)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.61 Safari/537.36";
            httpWebRequest.Proxy = new WebProxy(proxy);
            httpWebRequest.BeginGetResponse(new AsyncCallback(FinishWebRequest), httpWebRequest);
        }

        void FinishWebRequest(IAsyncResult result)
        {
            Console.WriteLine("END Request: " + DateTime.Now.ToLongTimeString());
            try
            {
                HttpWebResponse response = (result.AsyncState as HttpWebRequest).EndGetResponse(result) as HttpWebResponse;
                Console.WriteLine("Vnimanie!: " + response.ResponseUri.ToString());
                using (var streamReader = new StreamReader(response.GetResponseStream()))
                {
                    var responseText = streamReader.ReadToEnd();
                    if (responseText.Length > 100000)
                    {
                        HttpWebRequest httpWebRequest = (HttpWebRequest)result.AsyncState;
                        Console.WriteLine("GOOD: " + httpWebRequest.Address);
                        WebProxy proxy = (WebProxy)httpWebRequest.Proxy;
                        listOfProxies.Add(proxy.Address.ToString());
                    }
                    else Console.WriteLine("BAD");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("BAD");
            }
        }

        public async Task Start()
        {
            string pathToFile = AppDomain.CurrentDomain.BaseDirectory + '\\';
            IWebDriver browser;
            ChromeOptions options = new ChromeOptions();
            options.PageLoadStrategy = PageLoadStrategy.Eager;
            browser = new ChromeDriver(pathToFile, options, TimeSpan.FromSeconds(180));
            IJavaScriptExecutor js = (IJavaScriptExecutor)browser;
            browser.Manage().Window.Maximize();
            Thread.Sleep(2000);
            browser.Navigate().GoToUrl("https://hidemy.name/ru/proxy-list/?anon=234#list");
            System.Threading.Thread.Sleep(10000);
            List<IWebElement> elements;
            while (true)
            {
                elements = browser.FindElements(By.XPath("//tbody/tr/td/div[@class='bar']/p")).ToList();
                elements = browser.FindElements(By.XPath("//tbody/tr/td")).ToList();
                for (int i = 0; i < elements.Count(); i += 7)
                {
                    StartWebRequest("https://1xbet.com/", elements[i].Text + ":" + elements[i + 1].Text);
                }
                if (browser.FindElements(By.XPath("//li[@class='next_array']/a")) == null) break;
                elements = browser.FindElements(By.XPath("//li[@class='next_array']/a")).ToList();
                if (elements.Count() == 0) break;
                Console.WriteLine(elements[0].GetAttribute("href"));
                browser.Navigate().GoToUrl(elements[0].GetAttribute("href"));
                Thread.Sleep(25000);
            }
            Console.WriteLine("Sleep 120 sec...");
            Thread.Sleep(120000);
            browser.Quit();
            Console.WriteLine("listOfProxies count: " + listOfProxies.Count());
            for (int i = 0; i < listOfProxies.Count(); i++)
                Console.WriteLine(listOfProxies[i]);
        }

    }
}