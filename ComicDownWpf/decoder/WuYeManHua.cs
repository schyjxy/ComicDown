using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicDownWpf.decoder
{
    class WuYeManHua:ParserBase
    {
        public WuYeManHua()
        {
            desc = "拷贝漫画";
        }
        public override async Task<List<ComicDetailInfo>> GetHotComic(string url)
        {
            CommonParser common = new CommonParser(url, host, CodeName, true);
            ComicParam param = new ComicParam
            {
                HrefCss = "a:nth-child(2)",
                ImageUrlCss = "img",
                FirstCss = ".index-manga-item",
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

            if (document == null)
            {
                return comicInfo;
            }

            var startIndex = htmlString.IndexOf("漫画作者");
            htmlString = htmlString.Substring(startIndex);
            string text = htmlString.Substring(0, htmlString.IndexOf("</td>")).Trim();
            string[] arry = text.Split('|');

            comicInfo.Author = "作者:" + arry[0].Split('：')[1];
            comicInfo.status = "状态：" + arry[1].Split('：')[1];
            comicInfo.Tag = "题材：无";
            comicInfo.Description = "简介：" + document.GetElementById("ComicInfo").TextContent;

            var list = document.GetElementById("comiclistn").GetElementsByTagName("dd");

            foreach (var i in list)
            {
                string name = i.GetElementsByTagName("A")[0].TextContent.Trim();
                string href = i.GetElementsByTagName("a")[1].GetAttribute("href");

                if (!comicInfo.Charpter.ContainsKey(name))
                {
                    comicInfo.Charpter.Add(name, href);
                }
            }

            return comicInfo;
        }
    }
}
