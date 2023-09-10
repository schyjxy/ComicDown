using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComicPlugin;
using AngleSharp;
using System.Text.RegularExpressions;

namespace XManHuaParser
{
    public class XManHuaParser:CommonParser
    {
        public XManHuaParser()
        {
            m_host = "https://xmanhua.com/";
            m_hotUrl = "https://xmanhua.com/";
            m_description = "X漫画";
            m_searchUrl = "https://xmanhua.com/search?title=";
            HotComicCss(".index-manga-item", "a >img", "a", "p");

            ComicInfoCss
             (
                ".detail-info-tip >span >a",
                "#chapterlistload >a",
                ".detail-info-tip >span:nth-child(3) >span",
                ".detail-info-tip >span:nth-child(2) >span",
                ".detail-info-content",
                ""
            );

            SerachComicCss(".mh-item", "a > img", "a", "div > h2 >a");
        }

        //要设置cookies，不然一次只能拿到两个url
        private int AjaxRequest(ref List<string> list, GetOneImgHandler handler, string url, string cid, string mid, string pageIndex, string dateTime, string sign)
        {
            string key = "";
            int count = 0;
            var param = new Dictionary<string, string>();
            param.Add("cid", cid);
            param.Add("page", pageIndex);
            param.Add("key", key);
            param.Add("_cid", cid);
            param.Add("_mid", mid);
            param.Add("_dt", dateTime);
            param.Add("_sign", sign);

            httpUtils.SetRefer(url);
            var full_url = httpUtils.CombineUrl(url + "chapterimage.ashx", param);
            var js_msg = httpUtils.Get(full_url);
            var arry_obj = httpUtils.ExecuteJs(js_msg);
            int len = Convert.ToInt32(arry_obj.length);
            //Console.WriteLine("请求的url:{0}", full_url);

            for (int i = 0; i < len; i++)
            {
                if (isStop)
                {
                    return count;
                }

                //Console.WriteLine("url:{0}", arry_obj[i].ToString());            
                var imgUrl = arry_obj[i].ToString();

                if (list.Exists(o => o == imgUrl) == false)
                {
                    list.Add(imgUrl);
                    count++;
                    if (handler != null)
                    {
                        handler(imgUrl);
                    }
                }
            }

            return count;
        }

        public override List<string> GetComicImage(string url, GetOneImgHandler handler)
        {
            var list = new List<string>();         
            httpUtils.SetRefer(url);
            var htmlString = httpUtils.Get(url);
            var document = GetDocumentByHtmlString(htmlString);

            var imgCount = Regex.Match(htmlString, @"XMANHUA_IMAGE_COUNT\s*=\s*(?<data>\d+)").Groups["data"].Value;
            var cid = Regex.Match(htmlString, @"XMANHUA_CID\s*=\s*(?<data>\d+)").Groups["data"].Value;
            var mid = Regex.Match(htmlString, @"XMANHUA_MID\s*=\s*(?<data>\d+)").Groups["data"].Value;
            var sign = Regex.Match(htmlString, @"XMANHUA_VIEWSIGN\s*=\s*""(?<data>\w+)").Groups["data"].Value;
            var data_time = Regex.Match(htmlString, @"XMANHUA_VIEWSIGN_DT\s*=\s*""(?<data>[\s\w:-]*)").Groups["data"].Value;
    
            if (document == null)
            {
                return list;
            }

            int totalNum = Convert.ToInt32(imgCount);

            for (int i = 1; i < totalNum + 2;)
            {
                if (isStop)
                {
                    return list;
                }

                string nextUrl = url.Substring(0, url.Length - 1) + "-p" + i.ToString() + "/";
                htmlString = httpUtils.Get(nextUrl);
                cid = Regex.Match(htmlString, @"XMANHUA_CID\s*=\s*(?<data>\d+)").Groups["data"].Value;
                mid = Regex.Match(htmlString, @"XMANHUA_MID\s*=\s*(?<data>\d+)").Groups["data"].Value;
                sign = Regex.Match(htmlString, @"XMANHUA_VIEWSIGN\s*=\s*""(?<data>\w+)").Groups["data"].Value;
                data_time = Regex.Match(htmlString, @"XMANHUA_VIEWSIGN_DT\s*=\s*""(?<data>[\s\w:-]*)").Groups["data"].Value;         
                i = i + AjaxRequest(ref list, handler, nextUrl, cid, mid, i.ToString(), data_time, sign);

                if (list.Count == totalNum)
                {
                    break;
                }
            }
            
            return list;
           
        }
    }
}
