using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using AngleSharp.Dom;

namespace ComicDownWpf.decoder
{
    class KuKuDongMan : ParserBase
    {
        public KuKuDongMan()
        {
            host = "https://manhua.ikukudm.com";
            hotUrl = "https://manhua.ikukudm.com/";
            desc = "KuKu动漫";
        }

        public struct ImageListResult
        {
            public List<string> urlList;
            public int index;
        }

        public override async Task<List<ComicDetailInfo>> GetHotComic(string url)
        {
            CommonParser common = new CommonParser(url, host, CodeName);
            ComicParam param = new ComicParam
            {
                HrefCss = "a:nth-child(2)",
                ImageUrlCss = "img",
                FirstCss = "#comicmain > dd",
                ComicNameCss = "a:nth-child(2)",
            };

            var list = await common.GetHotComic(param);
            return list;
        }

        public override async Task<ComicDetailInfo> GetComicInfo(string url)
        {
            var comicInfo = new ComicDetailInfo();
            var htmlString = await HttpGet(url);
            var document = await ParseHtml(htmlString);

            if (document == null || htmlString == null)
            {
                return comicInfo;
            }

            var startIndex = htmlString.IndexOf("漫画作者");
            htmlString = htmlString.Substring(startIndex);
            string text = htmlString.Substring(0, htmlString.IndexOf("</td>")).Trim();
            string[] arry = text.Split('|');

            comicInfo.Author = "作者: " + arry[0].Split('：')[1];
            comicInfo.status = "状态：" + arry[1].Split('：')[1];
            comicInfo.Tag = "题材：无";
            comicInfo.Description = "简介：" + document.GetElementById("ComicInfo").TextContent;

            var list = document.GetElementById("comiclistn").GetElementsByTagName("dd");

            foreach (var i in list)
            {
                string name = i.GetElementsByTagName("A")[0].TextContent.Trim();
                string href = host +  i.GetElementsByTagName("A")[0].GetAttribute("href");

                if (!comicInfo.Charpter.ContainsKey(name))
                {
                    comicInfo.Charpter.Add(name, href);
                }
            }

            comicInfo.CodeName = CodeName;
            return comicInfo;
        }

        protected  async Task<ImageListResult> DoHttpGet(string url, int pageIndex)//这个已经被修改了，需要重新找到漫画地址
        {
            ImageListResult result = new ImageListResult();
            result.index = pageIndex;
            
            try
            {
                var htmlString = await HttpGet(url);
                var document = ParseHtmlSync(htmlString);
                var script = document.QuerySelector("body > table:nth-child(2) > tbody > tr > td > script:nth-child(4)").TextContent;//获取脚本
                script = script.Replace("m201304d", "http://pc.ihhmh.com/");
                script = script.Replace("m201001d", "http://pc.ihhmh.com/");
                script = script.Replace("m200911d", "http://pc.ihhmh.com/");
                script = script.Replace("k0910k", "http://pc.ihhmh.com/");

                var str = Regex.Match(script, @"<img\s*src\s*=\s*'(?<url>[""\s\w.:/ +]*)'").Groups["url"].Value;
                var imgUrl = str.Replace("+","").Replace(@"""","");
                Console.WriteLine("图片地址：{0}", imgUrl);
                result.urlList = new List<string>();
                result.urlList.Add(imgUrl);
            }
            catch(Exception ex)
            {
                Console.WriteLine("{0}", ex.StackTrace);
            }

            return result;
        }

        public override async Task<List<string>> GetComicImage(string url, GetOneImgHandler handler)
        {
            var pageCount = 0;
            var htmlString = await HttpGet(url);
            if(htmlString == null)
            {
                Console.WriteLine("获取网页错误");
            }
            var document = await ParseHtml(htmlString);
            var baseUrl = url.Substring(0, url.LastIndexOf("/"));
            var pageString = Regex.Match(htmlString, @"共(?<count>\d+)页").Groups["count"].Value;
            pageCount = Convert.ToInt32(pageString);
            var dict = new Dictionary<int, string>();
            var list = new List<string>();

            for (int page = 1; page < pageCount + 1;)
            {
                url = string.Format("{0}/{1}.htm", baseUrl, page);
                var result = await DoHttpGet(url, page);

                foreach(var i in result.urlList)
                {
                    if (handler != null)
                    {
                        handler(i);
                    }

                }
                list.AddRange(result.urlList);
                page = page + result.urlList.Count;
                System.Threading.Thread.Sleep(60);

            }        
            return list;
        }
    }
}
