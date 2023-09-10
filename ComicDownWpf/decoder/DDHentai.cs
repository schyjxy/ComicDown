using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ComicDownWpf.decoder
{
    class DDHentai:ParserBase
    {
        public DDHentai()
        {
            host = "https://zh.ddhentai.com/";
            hotUrl = "https://zh.ddhentai.com/";
            desc = "喵绅士";
        }

        public override async Task<List<ComicDetailInfo>> GetHotComic(string url)
        {
            var comicList = new List<ComicDetailInfo>();
            var document = await GetDocument(url);
            var elementList = document.GetElementsByClassName("gallery");

            foreach (var element in elementList)
            {
                var cominInfo = new ComicDetailInfo();
                cominInfo.ComicName = element.GetElementsByClassName("caption")[0].TextContent.Replace(":", "-");
                cominInfo.ImgaeUrl = element.GetElementsByTagName("img")[0].GetAttribute("data-src");
                cominInfo.Href = host + element.GetElementsByTagName("a")[0].GetAttribute("href");
                cominInfo.CodeName = CodeName;

                if (!comicList.Exists(t => t.ComicName == cominInfo.ComicName))
                {
                    comicList.Add(cominInfo);
                }
            }

            return comicList;
        }

        public override async Task<ComicDetailInfo> GetComicInfo(string url)
        {
            var baseTag = "tag-container field-name";
            var comicInfo = new ComicDetailInfo();
            var document = await GetDocument(url);

            if (document == null)
            {
                return comicInfo;
            }

            if (document.GetElementsByClassName(baseTag)[0] != null)
            {
                comicInfo.Author = document.GetElementsByClassName(baseTag)[1].TextContent.Trim();
            }

            comicInfo.Description = "简介：无";
            comicInfo.status = "状态：无";
            comicInfo.Tag = "题材：" + document.GetElementsByClassName(baseTag)[0].TextContent;
            var list = document.GetElementsByClassName("thumb-container");
            string href = host + list[0].QuerySelector("a").GetAttribute("href");
            comicInfo.CodeName = CodeName;
            comicInfo.Charpter.Add(string.Format("全一话({0})页", list.Length), href);
            return comicInfo;
        }

        private string GetImgurl(string url)//这个已经被修改了，需要重新找到漫画地址
        {
            var imgUrl = "";
            try
            {
                var htmlString = HttpGetSync(url);
                var document = ParseHtmlSync(htmlString);
                imgUrl = document.QuerySelector("#image-container > a > img").GetAttribute("src");
                
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0}", ex.StackTrace);
            }

            return imgUrl;
        }

        public override async Task<List<string>> GetComicImage(string url, GetOneImgHandler handler)
        {
            var htmlString = await HttpGet(url);
            var document = await ParseHtml(htmlString);
            var list = new List<string>();          
            var pageString = document.QuerySelector(".num-pages").TextContent;
            var pageCount = Convert.ToInt32(pageString);

            var imgUrl = GetImgurl(url);
            Console.WriteLine("源:{0}", imgUrl);
            var baseUrl = imgUrl.Substring(0, url.LastIndexOf("/"));
            var index = imgUrl.LastIndexOf(".");
            var suffix = imgUrl.Substring(index, imgUrl.Length - index);

            for (var pageIndex = 1; pageIndex < pageCount;pageIndex++)
            {
                var nextUrl = string.Format("{0}{1}{2}", baseUrl, pageIndex, suffix);
                Console.WriteLine("源:{0}", nextUrl);
                if (handler != null)
                {
                    handler(nextUrl);
                }
                list.Add(nextUrl);
            }
  
            return list;
        }
    }
}
