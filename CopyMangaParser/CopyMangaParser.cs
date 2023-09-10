using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using ComicPlugin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CopyMangaParser
{
    class CharpterResponse
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public string results { get; set; }
    }

    public class CopyMangaParser:CommonParser
    {
        private const string m_key = "xxxmanga.woo.key";
        private const string m_countApi = "api/kb/web/searchs/comics";

        public CopyMangaParser()
        {
            m_host = "https://www.copymanga.tv/";
            m_hotUrl = "https://www.copymanga.tv/";
            m_description = "拷贝漫画";
            m_searchUrl = "https://www.copymanga.tv/search?q=";

            HotComicCss("div.col-auto", "a >div>img", "a", "a > p");

            ComicInfoCss
             (
                "body > main > div.container.comicParticulars-title > div > div.col-9.comicParticulars-title-right > ul > li:nth-child(3) > span.comicParticulars-right-txt > a",
                "#default全部 > ul >a",
                "body > main > div.container.comicParticulars-title > div > div.col-9.comicParticulars-title-right > ul > li:nth-child(7) > span.comicParticulars-left-theme-all.comicParticulars-tag > a:nth-child(1)",
                "body > main > div.container.comicParticulars-title > div > div.col-9.comicParticulars-title-right > ul > li:nth-child(6) > span.comicParticulars-right-txt",
                "body > main > div.container.comicParticulars-synopsis > div:nth-child(2) > p",
                ""
            );
        }

        private byte[] HexToByteArry(string data)
        {
            int pos = 0;
            byte[] buffer = new byte[data.Length / 2];

            for (int i = 0; i < data.Length; i += 2)
            {
                buffer[pos++] = Convert.ToByte(data.Substring(i, 2), 16);
            }

            return buffer;
        }

        private string AesDecrypt(string data, string key, string iv)
        {
            Byte[] keyArray = Encoding.UTF8.GetBytes(key);
            byte[] rawData = HexToByteArry(data);
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

        protected override void ParseCharpter(IHtmlDocument document, ComicDetailInfo comicInfo)
        {
            var url = parserParam.ComicInfoUrl;
            var path = url.Substring(url.LastIndexOf("/") + 1);
            var checkUrl = string.Format("{0}comicdetail/{1}/chapters", m_host, path);
            var json = httpUtils.Get(checkUrl);
            var charpterInfo = JsonConvert.DeserializeObject<CharpterResponse>(json);
            var aesData = charpterInfo.results.Substring(0x10);
            var data = AesDecrypt(aesData, m_key, charpterInfo.results.Substring(0, 0x10));
            var obj = JsonConvert.DeserializeObject(data) as JObject;
            JArray arry = obj["groups"]["default"]["chapters"] as JArray;
            
            for(int i = 0;i < arry.Count; i++)
            {
                var key = arry[i]["name"].ToString();
                var value = string.Format("{0}/chapter/{1}", url, arry[i]["id"].ToString());
                if(!comicInfo.Charpter.ContainsKey(key))
                {
                    comicInfo.Charpter.Add(key, value);
                }
                
            }
        }

        public override SearchResult SearchComic(string keyWord)
        {
            SearchResult searchResult = new SearchResult();
            searchResult.DetailInfoList = new List<ComicDetailInfo>();
            var searchUrl = string.Format("{0}{1}?offset=0&platform=2&limit=12&q=${2}&q_type=${3}", 
                               m_host, m_countApi, keyWord, "");
            var json = httpUtils.Get(searchUrl);
            JObject obj = JsonConvert.DeserializeObject(json) as JObject;
            JArray arry = obj["results"]["list"] as JArray;
            
            for(int i = 0;i < arry.Count;i++)
            {
                ComicDetailInfo info = new ComicDetailInfo();
                info.CodeName = CodeName;
                info.ComicName = arry[i]["name"].ToString();
                info.Href = m_host + "comic/" + arry[i]["path_word"].ToString();
                info.ImageUrl = arry[i]["cover"].ToString(); ;               
                searchResult.DetailInfoList.Add(info);
            }

            searchResult.Count = searchResult.DetailInfoList.Count;
            return searchResult;
        }

        public override List<string> GetComicImage(string url, GetOneImgHandler handler)
        {
            var document = GetDocument(url);
            List<string> list = new List<string>();

            if(document == null)
            {
                return list;
            }

            var imgData = document.QuerySelector("div.imageData").GetAttribute("contentkey");
            var iv = imgData.Substring(0, 0x10);
            var data = imgData.Substring(0x10);
            var jsonData = AesDecrypt(data, m_key, iv);
            JArray arry = JsonConvert.DeserializeObject(jsonData) as JArray;
            for(int i = 0;i < arry.Count;i++)
            {
                var imageUrl = arry[i]["url"].ToString();
                handler?.Invoke(imageUrl);
                list.Add(imageUrl);
            }

            return list;
        }
    }
}
