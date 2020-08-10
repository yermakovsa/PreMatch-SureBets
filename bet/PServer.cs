using bet.Data;
using bet.Functions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Models;
using Titanium.Web.Proxy.Network;


namespace bet
{
    public class PServer
    {
        static string matchups = null, markets = null;
        private static ProxyServer proxyServer;
        private List<Match> xbet;
        public static Dictionary<string, int> map = new Dictionary<string, int>();
        public static int cnt = 0;
        public static void Clear()
        {
            matchups = null;
            markets = null;
        }
        public static void Check()
        {
            if (matchups != null && markets != null) Console.WriteLine(matchups.Length + " | " + markets.Length);
            else Console.WriteLine("IDI NAHUI");
        }
        public PServer()
        {
            proxyServer = new ProxyServer();
            xbet = new List<Match>();
        }

        public void Start()
        {
            proxyServer.BeforeResponse += OnResponse;
            proxyServer.ServerCertificateValidationCallback += OnCertificateValidation;
            proxyServer.ClientCertificateSelectionCallback += OnCertificateSelection;
            proxyServer.CertificateManager.CertificateEngine = CertificateEngine.BouncyCastleFast;
            proxyServer.EnableConnectionPool = false;
            proxyServer.ThreadPoolWorkerThread = 512;
            //proxyServer.ThreadPoolWorkerThread = Environment.ProcessorCount * 2000;
            var explicitEndPoint = new ExplicitProxyEndPoint(IPAddress.Loopback, 8000, true);
            //proxyServer.EnableConnectionPool = true;
            explicitEndPoint.BeforeTunnelConnectRequest += OnBeforeTunnelConnectRequest;
            proxyServer.AddEndPoint(explicitEndPoint);
            proxyServer.Start();
        }

        public void Stop()
        {
            proxyServer.BeforeResponse -= OnResponse;
            proxyServer.ServerCertificateValidationCallback -= OnCertificateValidation;
            proxyServer.ClientCertificateSelectionCallback -= OnCertificateSelection;
            proxyServer.Stop();
        }
        public static void SetExternalProxy(string IPadress, int port)
        {
            //proxyServer.setExternalProxy(IPaddress, Convert.ToInt32(portnumber));
            proxyServer.UpStreamHttpProxy = new ExternalProxy() { HostName = IPadress, Port = port };
            proxyServer.UpStreamHttpsProxy = new ExternalProxy() { HostName = IPadress, Port = port };
        }
        private async Task OnBeforeTunnelConnectRequest(object sender, TunnelConnectSessionEventArgs e)
        {
            if(!e.HttpClient.Request.Url.Contains("guest.api.arcadia.pinnacle.com")) e.DecryptSsl = false;
        }
        public async Task OnResponse(object sender, SessionEventArgs e)
        {
            if (e.HttpClient.Response.StatusCode == 200 && (e.HttpClient.Request.Method == "GET" || e.HttpClient.Request.Method == "POST") )
            {
                //Console.WriteLine(await e.GetResponseBodyAsString());
                if (e.HttpClient.Request.Url.Contains("matchups") || e.HttpClient.Request.Url.Contains("markets/straight"))
                {

                    if (e.HttpClient.Request.Url.Contains("matchups")) matchups = await e.GetResponseBodyAsString();
                    else markets = await e.GetResponseBodyAsString();

                    if (matchups != null && markets != null)
                    {
                        Console.WriteLine("GO " + DateTime.Now.ToLongTimeString());
                        cnt++;
                        Console.WriteLine("VNIMANIE: " + cnt);
                        Pinnacle.Parse("{value:" + matchups + "}", "{value:" + markets + "}");

                        matchups = null;
                        markets = null;
                    }
                        //
                        /*Match match = Xbet.Parse(await e.GetResponseBodyAsString());
                        if (!map.ContainsKey(match.matchName))
                        {
                            map[match.matchName] = 1;
                            xbet.Add(match);
                            Console.WriteLine("END: " + DateTime.Now.ToLongTimeString());
                        }*/
                        //xbet.Add(Xbet.Parse(await e.GetResponseBodyAsString()));
                }
            }
        }
        public Task OnCertificateValidation(object sender, CertificateValidationEventArgs e)
        {
            if (e.SslPolicyErrors == System.Net.Security.SslPolicyErrors.None)
                e.IsValid = true;

            return Task.CompletedTask;
        }

        public Task OnCertificateSelection(object sender, CertificateSelectionEventArgs e)
        {
            return Task.CompletedTask;
        }

        public Bookmaker GetXbet()
        {
            Bookmaker bookmaker = new Bookmaker("1xbet", xbet);
            return bookmaker;
        }
    }
}
