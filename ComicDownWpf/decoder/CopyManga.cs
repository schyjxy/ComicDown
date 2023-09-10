using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicDownWpf.decoder
{
    class CopyManga:ParserBase
    {
        public CopyManga()
        {
            host = "https://www.copymanga.com";
            hotUrl = "https://www.copymanga.com";
            desc = "拷贝漫画";
        }

        public override async Task<List<ComicDetailInfo>> GetHotComic(string url)
        {
            var comicList = new List<ComicDetailInfo>();       
            var document = await GetDocument(url);
            var elementList = document.GetElementsByClassName("col-auto");

            foreach (var element in elementList)
            {
                var cominInfo = new ComicDetailInfo()
                {
                    ComicName = element.GetElementsByTagName("p")[0].TextContent,
                    ImgaeUrl = element.GetElementsByTagName("img")[0].GetAttribute("data-src"),
                    Href = host + element.Children[0].GetAttribute("href"),
                    CodeName = this.CodeName
                };

                if (!comicList.Exists(t => t.ComicName == cominInfo.ComicName))
                {
                    comicList.Add(cominInfo);
                }
            }

            return comicList;
        }
        //public override async Task<ComicDetailInfo> GetComicInfo(string url)
        //{
        //    var comicInfo = new ComicDetailInfo();
        //    var htmlString = await GetHtmlStringByCef(url);
        //    var document = await ParseHtml(htmlString);

        //    if (document == null)
        //    {
        //        return comicInfo;
        //    }

        //    if (document.GetElementsByClassName("comicParticulars-right-txt")[1] != null)
        //    {
        //        comicInfo.Author = "作者：" + document.GetElementsByClassName("comicParticulars-right-txt")[1].Children[0].TextContent.Trim();
        //    }

        //    if (document.GetElementsByClassName("intro").Length > 0)
        //    {
        //        comicInfo.Description = "简介：" + document.GetElementsByClassName("intro")[0].TextContent;
        //    }

        //    comicInfo.status = "状态：" + document.GetElementsByClassName("comicParticulars-right-txt")[4].TextContent.Trim();

        //    if (document.GetElementsByClassName("comicParticulars-left-theme-all comicParticulars-tag").Length > 0)
        //    {
        //        comicInfo.Tag = "题材：" + document.GetElementsByClassName("comicParticulars-left-theme-all comicParticulars-tag")[0].TextContent.Trim().Replace("\n", "").Replace("#", "");
        //    }
        //    else
        //    {
        //        comicInfo.Tag = "题材：无";
        //    }

        //    var list = document.GetElementsByClassName("tab-pane fade show active")[0].GetElementsByTagName("ul");

        //    foreach (var i in list)
        //    {
        //        foreach (var j in i.GetElementsByTagName("a"))
        //        {
        //            if (j.GetAttribute("target") == "_blank")
        //            {
        //                string name = j.TextContent.Trim().Replace(" ", "");
        //                string href = host + j.GetAttribute("href");

        //                if (!comicInfo.Charpter.ContainsKey(name))
        //                {
        //                    comicInfo.Charpter.Add(name, href);
        //                }
        //            }

        //        }
        //    }

        //    return comicInfo;
        //}


        public override async Task<ComicDetailInfo> GetComicInfo(string url)
        {
            CommonParser commonParser = new CommonParser(url, host, CodeName, true);
            CheckComicInfo checkComicInfo = new CheckComicInfo()
            {
                AuthorCss = "body > main > div.container.comicParticulars-title > div > div.col-9.comicParticulars-title-right > ul > li:nth-child(3) > span.comicParticulars-right-txt",
                CharpterCss = "#default全部 > ul >a",
                TagCss = "body > main > div.container.comicParticulars-title > div > div.col-9.comicParticulars-title-right > ul > li:nth-child(7) > span.comicParticulars-left-theme-all.comicParticulars-tag",
                StatusCss = "body > main > div.container.comicParticulars-title > div > div.col-9.comicParticulars-title-right > ul > li:nth-child(6) > span.comicParticulars-right-txt",
                DescriptionCss = "head > title"
            };
            var info = await commonParser.GetComicInfo(checkComicInfo);
            return info;
        }

        public override Task<List<string>> GetComicImage(string url, GetOneImgHandler handler)
        {
            return base.GetComicImage(url, handler);
        }
    }
}
