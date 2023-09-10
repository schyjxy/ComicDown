using AngleSharp;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using ComicDownWpf.Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Net.Http;

namespace ComicDownWpf.decoder
{
    class ImageParam
    { 
        public string Url { get; set; }
    }

    public class ParserBase
    {
        HtmlParser htmlParser;
        protected string host = "";
        protected string hotUrl = "";
        protected string cateUrl = "";
        protected string searchUrl = "";
        protected string desc = "";
        public delegate void GetOneImgHandler(string imgUrl);
        public CefHelper BrowerHelper { get; set; }

        public void SetHepler(CefHelper helper)
        {
            this.BrowerHelper = helper;        
        }

        public ParserBase()
        {
            htmlParser = new HtmlParser();
        }

        public string Description { get { return desc; } }

        public string CateUrl
        {
            get { return cateUrl; }
        }

        public string HotComicUrl { get { return hotUrl; } }

        public string SearchUrl { get { return searchUrl; } }

        public string CodeName { get;set; }

        public string HttpGetSync(string Url)//Http Get方法
        {
            string retString = "";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.Method = "GET";
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            request.Headers.Add("Accept-Encoding:gzip, deflate");//启用压缩编码
            request.Headers.Add("Cache-Control:max-age=0");
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36";
            request.KeepAlive = false;
            request.Host = new Uri(Url).Host;
            request.AllowAutoRedirect = true;
            request.Timeout = 10000;
            request.ProtocolVersion = HttpVersion.Version10;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            retString = DecompressEncode.DealAll(request.Host, myResponseStream, response.ContentEncoding);
            return retString;
        }

        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true; //总是接受
        }

        public async Task<string> HttpsGet(string Url)//Https Get方法
        {
            HttpClient httpClient = new HttpClient();
            var handler = new HttpClientHandler();
            handler.AllowAutoRedirect = true;
            //handler.UseCookies = true;
            //handler.CookieContainer = cookies;
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            var result = httpClient.GetAsync("https://www.zhihu.com/").Result;
            return "";
        }


        public async Task<string> HttpGet(string Url)//Http Get方法
        {
            try
            {
                string retString = "";
                HttpWebRequest request = null;

                if(Url.StartsWith("https"))
                {
                    request = WebRequest.Create(Url) as HttpWebRequest;
                    ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                    request.ProtocolVersion = HttpVersion.Version11;
                    // 这里设置了协议类型。
                    ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;// SecurityProtocolType.Tls1.2;
                    request.KeepAlive = false;
                    ServicePointManager.CheckCertificateRevocationList = true;
                    ServicePointManager.DefaultConnectionLimit = 100;
                    ServicePointManager.Expect100Continue = false;
                }
                else
                {
                    request = (HttpWebRequest)WebRequest.Create(Url);
                }

                request.Method = "GET";
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9";
                request.Headers.Add("Accept-Encoding:gzip, deflate");//启用压缩编码
                request.Headers.Add("Cache-Control:no-cache");
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.198 Safari/537.36";
                request.AllowAutoRedirect = true;

                request.Headers.Add(@"authority", "18comic.vip");
                request.ProtocolVersion = HttpVersion.Version10;
                request.Host = new Uri(Url).Host;
                request.AllowAutoRedirect = true;
                request.Timeout = 10000;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream myResponseStream = response.GetResponseStream();
                retString = DecompressEncode.DealAll(request.Host, myResponseStream, response.ContentEncoding);
                return retString;
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0}", "HttpGet:" + ex.Message);
            }
            return null;
        }

        public async Task<string> GetHtmlStringByCef(string url)
        {
            return await BrowerHelper.GetHtmlString(url);
        }

        public async Task<string> GetHtmlStringAgain()
        {
            return await BrowerHelper.GetHtmlStringAgain();
        }

        public void ExexuteJs(string js)
        {
            BrowerHelper.ExecuteJs(js);
        }

        protected async Task<IHtmlDocument> GetDocument(string url)
        {
            DateTime time = DateTime.Now;
            try
            {
                string text = await HttpGet(url);             
                return GetDocumentByHtmlString(text);
            }
            catch (Exception ex)
            {
                return null;
            }

            //Console.WriteLine(text);

        }

        protected IHtmlDocument GetDocumentByHtmlString(string text)
        {
            var context = BrowsingContext.New(Configuration.Default);
            var parser = context.GetService<IHtmlParser>();
            var document = parser.ParseDocument(text);
            return document;
        }

        protected  IHtmlDocument ParseHtmlSync(string htmlString)
        {
            List<ComicDetailInfo> comicList = new List<ComicDetailInfo>();
            var context = BrowsingContext.New(Configuration.Default);
            var parser = context.GetService<IHtmlParser>();
            var document = parser.ParseDocument(htmlString);
            return document;
        }

        protected async Task<IHtmlDocument> ParseHtml(string htmlString)
        {
            List<ComicDetailInfo> comicList = new List<ComicDetailInfo>();
            var context = BrowsingContext.New(Configuration.Default);
            var parser = context.GetService<IHtmlParser>();
            var document = parser.ParseDocument(htmlString);
            return document;
        }

        public virtual async Task<ComicDetailInfo> GetComicInfo(string url)
        {
            ComicDetailInfo comicDetailInfo = new ComicDetailInfo();
            return comicDetailInfo;
        }

        public virtual async Task<List<string>> GetComicImage(string url, GetOneImgHandler handler)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<SearchResult> GetComicMenu()
        {
            throw new NotImplementedException();
        }

        public virtual async Task<bool> SaveImage(string url)
        {
            return true;
        }

        public virtual async Task<List<ComicDetailInfo>> GetHotComic(string url)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<SearchResult> SearchComic(string keyWord)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<List<ComicDetailInfo>> GoToPage(string url)
        {
            throw new NotImplementedException();
        }
    }
}
