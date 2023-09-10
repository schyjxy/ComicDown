using ComicPlugin;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ComicDownTools
{
    class HttpUtil
    {
        static string m_cookie;
      

        public static byte[] HexByte(string data)
        {
            int pos = 0;
            byte[] buffer = new byte[data.Length/2];

            for(int i = 0;i < data.Length;i+=2)
            {
                buffer[pos++] = Convert.ToByte(data.Substring(i,2), 16);    
            }

            return buffer;
        }


        public static string AesDecrypt(string data, string key, string iv)
        {
            Byte[] keyArray = Encoding.UTF8.GetBytes(key);
            byte[] rawData = HexByte(data);
            var base64Str = Convert.ToBase64String(rawData);
            Byte[] toEncryptArray = Convert.FromBase64String(base64Str);

            var rijndael = new System.Security.Cryptography.RijndaelManaged();
            rijndael.BlockSize = 128;
            rijndael.Key = keyArray;
            rijndael.Mode = System.Security.Cryptography.CipherMode.CBC;
            rijndael.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
            rijndael.IV = Encoding.UTF8.GetBytes(iv);

            System.Security.Cryptography.ICryptoTransform cTransform = rijndael.CreateDecryptor();
            Byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            return Encoding.UTF8.GetString(resultArray);
        }

        public static async Task<string> Get(string url, Encoding encoding = null)//Http Get方法
        {
            try
            {
                string retString = "";
                HttpWebRequest request = null;

                request = (HttpWebRequest)WebRequest.Create(url);
                request.KeepAlive = true;

                request.Method = "GET";
                request.Accept = "*/*";
                //request.Headers.Add("Cookie", "image_time_cookie=265750|638087173199522808|2,265713|638087192506991761|10; mangabzimgpage=265750|2:1,265713|2:1");
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.198 Safari/537.36";
                request.AllowAutoRedirect = true;

                request.ProtocolVersion = HttpVersion.Version10;
                request.CookieContainer = new CookieContainer();
                request.Host = new Uri(url).Host;
                request.AllowAutoRedirect = true;
                request.Timeout = 5000;
                request.KeepAlive = true;

                if (m_cookie != "")
                {
                    request.Headers.Add("Cookie", m_cookie);
                }

                HttpWebResponse response = (HttpWebResponse)(await request.GetResponseAsync());

                if (response.Headers.Count > 0)
                {
                    string[] data = response.Headers.GetValues("Set-Cookie");
                    m_cookie = "";
                    if(data != null)
                    {
                        foreach (var i in data)
                        {
                            m_cookie += i;
                        }
                    }
                    
                }

                Console.WriteLine("获取当前cookie:{0}", m_cookie);
                Stream myResponseStream = response.GetResponseStream();
                DateTime dateTime= DateTime.Now;
                StreamReader reader = new StreamReader(myResponseStream, encoding);              
                retString = reader.ReadToEnd();
                Console.WriteLine("耗时:{0}", DateTime.Now.Subtract(dateTime).TotalMilliseconds);
                return retString;
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0}", "HttpGet:" + ex.Message);
            }
            return null;
        }
    }
}
