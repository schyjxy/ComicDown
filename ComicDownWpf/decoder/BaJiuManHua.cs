using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ComicDownWpf.decoder
{
    class BaJiuManHua:ParserBase
    {
        public BaJiuManHua()
        {
            host = "http://m.89mh.com";
            hotUrl = "http://m.89mh.com";
            desc = "89漫画";
        }

        public override async Task<List<ComicDetailInfo>> GetHotComic(string url)
        {
            var comicList = new List<ComicDetailInfo>();
            var document = await GetDocument(url);
            var elementList = document.GetElementsByClassName("col_3_1");

            foreach (var i in elementList)
            {
                foreach(var element in i.GetElementsByTagName("li"))
                {
                    var cominInfo = new ComicDetailInfo()
                    {
                        ComicName = element.GetElementsByTagName("a")[1].TextContent,
                        ImgaeUrl = element.GetElementsByTagName("img")[0].GetAttribute("src"),
                        Href = host + element.GetElementsByTagName("a")[0].GetAttribute("href"),
                        CodeName = this.CodeName
                    };

                    if (!comicList.Exists(t => t.ComicName == cominInfo.ComicName))
                    {
                        comicList.Add(cominInfo);
                    }
                }
             
            }

            return comicList;
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

            comicInfo.Author = "作者: " + document.GetElementsByClassName("txtItme")[1].TextContent.Trim();
            comicInfo.status = "状态：" + document.GetElementsByClassName("txtItme")[0].TextContent.Trim();
            comicInfo.Tag = "题材：" + document.GetElementsByClassName("txtItme")[2].TextContent.Trim();
            comicInfo.Description = "简介：" + document.GetElementsByClassName("txtDesc autoHeight")[0].TextContent.Trim();

            var list = document.GetElementById("mh-chapter-list-ol-0").GetElementsByTagName("li");

            foreach (var i in list)
            {
                string name = i.GetElementsByTagName("span")[0].TextContent.Trim();
                string href = host + i.GetElementsByTagName("a")[0].GetAttribute("href");

                if (!comicInfo.Charpter.ContainsKey(name))
                {
                    comicInfo.Charpter.Add(name, href);
                }
            }

            comicInfo.CodeName = CodeName;
            return comicInfo;
        }

        public override async Task<List<string>> GetComicImage(string url, GetOneImgHandler handler)
        {
            List<string> imgList = new List<string>();
            var htmlString = await HttpGet(url);
            var document = await ParseHtml(htmlString);
            return imgList;
        }
    }
}
