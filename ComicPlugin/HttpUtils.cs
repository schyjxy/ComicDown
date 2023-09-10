using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using AngleSharp.Io;
using Microsoft.JScript;
using Microsoft.JScript.Vsa;

namespace ComicPlugin
{
    public class HttpUtils
    {
        Dictionary<string, string> m_headres;
        private string m_href;
        private string m_userAgent;
        private string m_cookies;
        private int m_timeOut = 20000;
        private CookieContainer m_cookieContainer;
        private Encoding m_encode;

        public HttpUtils()
        {
            m_headres = new Dictionary<string, string>();
            m_href = "";
            m_cookies = "";
            m_encode = Encoding.UTF8;
            m_cookieContainer = new CookieContainer();
            m_userAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.198 Safari/537.36";
            ServicePointManager.DefaultConnectionLimit = 10 * 1000;
        }

        public void SetUserAgent(string agent)
        {
            m_userAgent = agent;
        }

        public void SetRefer(string url)
        {
            m_href = url;
        }

        public void SetEncoding(Encoding newEncode)
        {
            m_encode = newEncode;
        }

        public void SetCookies(string key, string value)
        {
            
        }

        public void AddHeaders(string key, string val)
        {
            m_headres.Add(key, val);
        }

        public ArrayObject ExecuteJs(string jsText)
        {
            VsaEngine Engine = VsaEngine.CreateEngine();
            ArrayObject result = null;
            try
            {
                result = Microsoft.JScript.Eval.JScriptEvaluate(jsText, Engine) as ArrayObject;
            }
            catch (Exception ex)
            {
                return null;
            }
            return result;
        }

        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true; //总是接受
        }

        public string CombineUrl(string baseUrl, Dictionary<string, string> paramDict)
        {
            int pos = 0;
            string url = baseUrl + "?";

            foreach (var i in paramDict)
            {
                url += i.Key + "=" + System.Web.HttpUtility.UrlEncode(i.Value);
                if (pos < paramDict.Count - 1)
                {
                    url += "&";
                }
                
                pos++;
            }

            return url;
        }

        public HttpWebRequest MakeHttpRequest(string url)
        {
            try
            {
                HttpWebRequest request = null;
                ServicePointManager.Expect100Continue = false;

                if (url.StartsWith("https"))
                {
                    request = WebRequest.Create(url) as HttpWebRequest;
                    ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                    request.ProtocolVersion = HttpVersion.Version11;
                    // 这里设置了协议类型。
                    ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;// SecurityProtocolType.Tls1.2;                   
                    ServicePointManager.CheckCertificateRevocationList = true;
                }
                else
                {
                    request = (HttpWebRequest)WebRequest.Create(url);
                }

                request.Method = "GET";

                if (m_href != "")
                {
                    request.Referer = m_href;
                }

                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9";
                request.Headers.Add("Cache-Control:no-cache");
                request.UserAgent = m_userAgent;
                request.KeepAlive = false;

                request.ProtocolVersion = HttpVersion.Version11;
                request.Host = new Uri(url).Host;
                request.AllowAutoRedirect = true;
                request.Timeout = m_timeOut;
                request.ReadWriteTimeout = m_timeOut;
                return request as HttpWebRequest;

            }
            catch (Exception ex)
            {
                Console.WriteLine("ComicParser 创建异常 :" + ex.Message);
            }

            return null;
        }

        public static string DecodeBase64(string result)
        {
            string decode = "";
            byte[] bytes = System.Convert.FromBase64String(result);
            try
            {
                decode = Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                decode = result;
            }
            return decode;
        }

        public static string EncodeBase64(string source)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(source);
            try
            {
                source = System.Convert.ToBase64String(bytes);
            }
            catch
            {

            }
            return source;
        }
       
        //非并发，keep-alive 为true
        public string Get(string url, Encoding encoding = null)//Http Get方法
        {
            try
            {
                string retString = "";
                HttpWebRequest request;            
                ServicePointManager.Expect100Continue = false;

                if (url.StartsWith("https"))
                {
                    request = WebRequest.Create(url) as HttpWebRequest;
                    ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                    request.ProtocolVersion = HttpVersion.Version11;
                    // 这里设置了协议类型。
                    ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;// SecurityProtocolType.Tls1.2;
                    ServicePointManager.CheckCertificateRevocationList = true;                  
                }
                else
                {
                    request = (HttpWebRequest)WebRequest.Create(url);
                }

                request.Method = "GET";
                if (m_href != "")
                {
                    request.Referer = m_href;
                }

                if(m_cookies != "")
                {                  
                    request.Headers.Add("Cookie", m_cookies);
                }

                request.Accept = "*/*";
                request.Headers.Add("Accept-Encoding:gzip, deflate");//启用压缩编码
                request.Headers.Add("Cache-Control:no-cache");
                request.UserAgent = m_userAgent;
                request.AllowAutoRedirect = true;
                request.ProtocolVersion = HttpVersion.Version10;
                request.Host = new Uri(url).Host;
                request.Timeout = m_timeOut;
                request.ReadWriteTimeout = m_timeOut;
                request.KeepAlive = true;//
                request.CookieContainer = m_cookieContainer;

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                foreach (Cookie cook in response.Cookies)
                {
                    //Console.WriteLine("{0} = {1}", cook.Name, cook.Value);
                    //Console.WriteLine("Domain: {0}", cook.Domain);
                    //Console.WriteLine("Path: {0}", cook.Path);
                    //Console.WriteLine("Port: {0}", cook.Port);
                    //Console.WriteLine("Secure: {0}", cook.Secure);
                    //Console.WriteLine("When issued: {0}", cook.TimeStamp);
                    //Console.WriteLine("Expires: {0} (expired? {1})",cook.Expires, cook.Expired);
                    //Console.WriteLine("Don't save: {0}", cook.Discard);
                    //Console.WriteLine("Comment: {0}", cook.Comment);
                    //Console.WriteLine("Uri for comments: {0}", cook.CommentUri);
                    //Console.WriteLine("Version: RFC {0}", cook.Version == 1 ? "2109" : "2965");
                }

                Stream myResponseStream = response.GetResponseStream();
                retString = DecompressEncode.DealAll(request.Host, myResponseStream, response.ContentEncoding, encoding);
                return retString;
            }
            catch (Exception ex)
            {
                Console.WriteLine("访问url:{0} 失败, {1}", url, "HttpGet:" + ex.Message);
            }
            return null;
        }

        public bool DownLoadImage(string url, string path)
        {
            bool ret = false;
            FileStream fileStream = null;
            DateTime time = DateTime.Now;

            try
            {
                HttpWebRequest request = MakeHttpRequest(url);
                //Console.WriteLine("==========Begin============");
                //Console.WriteLine("开始请求 url: {0} ", url);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                //Console.WriteLine("请求耗时:{1}, url: {0}", url, DateTime.Now.Subtract(time).TotalMilliseconds);
                DateTime time1 = DateTime.Now;

                if (response == null)
                {
                    return false;
                }

                Stream responseStream = response.GetResponseStream();
                fileStream = new FileStream(path, FileMode.Create);
               
                int len;
                byte[] buffer = new byte[10240];

                do
                {
                    len = responseStream.Read(buffer, 0, buffer.Length);
                    fileStream.Write(buffer, 0, len);
                } while (len > 0);

                //Console.WriteLine("GetResponse结束耗时:{1}, url:{0}", url, DateTime.Now.Subtract(time1).TotalMilliseconds);
                //Console.WriteLine("==========End============");
                //response?.Dispose();
                responseStream?.Close();
                responseStream.Close();                     
                response?.Close();
                request?.Abort();
                ret = true;
            }
            catch (Exception ex)
            {
                var cost = DateTime.Now.Subtract(time).TotalMilliseconds;
                Console.WriteLine("Comic Parser 下载图片失败, {0}, url:{1}, 耗时:{2} ms", ex.Message, url, cost);
                ret = false;
            }

            fileStream?.Close();
            if(!ret)
            {
                if (File.Exists(path))//LoadCache和LoadImage 冲突
                {
                    File.Delete(path);
                }
            }
        
           return ret;
        }
    }
}
