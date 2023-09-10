using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ComicPlugin;

namespace MangaBzParser
{
    //和X漫画解析一样
    public class MangaBzParser:CommonParser
    {
        public MangaBzParser()
        {
            m_host = "https://www.mangabz.com/";
            m_cateUrl = "https://mangabz.com/manga-list-0-0-0/";
            m_hotUrl = "https://www.mangabz.com";
            m_searchUrl = "https://www.mangabz.com/search?title=";
            m_description = "MangaBz";

            checkComicInfo.AuthorCss = "body > div.detail-info-1 > div > div > p.detail-info-tip > span:nth-child(1) > a ";
            checkComicInfo.CharpterCss = ".detail-list-form-con > a";
            checkComicInfo.TagCss = ".item";
            checkComicInfo.StatusCss = "body > div.detail-info-1 > div > div > p.detail-info-tip > span:nth-child(2) > span";
            checkComicInfo.DescriptionCss = ".detail-info-content";

            SerachComicCss(".mh-item", "a >img", "a", "div >h2 >a");
        }

        public override List<ComicDetailInfo> GetHotComic()
        {
            var comicList = new List<ComicDetailInfo>();
            var document = GetDocument(m_hotUrl);
            if(document == null)
            {
                Console.WriteLine("Mangabz 网络获取失败");
                return comicList;
            }
            var elementList = document.QuerySelectorAll(".index-manga-item");

            foreach (var element in elementList)
            {
                var cominInfo = new ComicDetailInfo()
                {
                    ComicName = element.GetElementsByClassName("index-manga-item-title")[0].FirstChild.TextContent,
                    ImageUrl = element.GetElementsByTagName("img")[0].GetAttribute("src"),
                    Href = m_host + element.GetElementsByTagName("a")[0].GetAttribute("href"),
                    CodeName = this.CodeName
                };

                if (!comicList.Exists(t => t.ComicName == cominInfo.ComicName))
                {
                    comicList.Add(cominInfo);
                }
            }

            return comicList;
        }

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
            Console.WriteLine("请求的url:{0}", full_url);

            for (int i = 0; i < len; i++)
            {
                if (isStop)
                {
                    return count;
                }

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

            var imgCount = Regex.Match(htmlString, @"MANGABZ_IMAGE_COUNT\s*=\s*(?<data>\d+)").Groups["data"].Value;
            var cid = Regex.Match(htmlString, @"MANGABZ_CID\s*=\s*(?<data>\d+)").Groups["data"].Value;
            var mid = Regex.Match(htmlString, @"MANGABZ_MID\s*=\s*(?<data>\d+)").Groups["data"].Value;
            var sign = Regex.Match(htmlString, @"MANGABZ_VIEWSIGN\s*=\s*""(?<data>\w+)").Groups["data"].Value;
            var data_time = Regex.Match(htmlString, @"MANGABZ_VIEWSIGN_DT\s*=\s*""(?<data>[\s\w:-]*)").Groups["data"].Value;

            if (document == null)
            {
                return list;
            }

            int totalNum = Convert.ToInt32(imgCount);

            for (int i = 1; i < totalNum + 2;)
            {
                string nextUrl = url.Substring(0, url.Length - 1) + "-p" + i.ToString() + "/";
                htmlString = httpUtils.Get(nextUrl);
                cid = Regex.Match(htmlString, @"MANGABZ_CID\s*=\s*(?<data>\d+)").Groups["data"].Value;
                mid = Regex.Match(htmlString, @"MANGABZ_MID\s*=\s*(?<data>\d+)").Groups["data"].Value;
                sign = Regex.Match(htmlString, @"MANGABZ_VIEWSIGN\s*=\s*""(?<data>\w+)").Groups["data"].Value;
                data_time = Regex.Match(htmlString, @"MANGABZ_VIEWSIGN_DT\s*=\s*""(?<data>[\s\w:-]*)").Groups["data"].Value;
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
