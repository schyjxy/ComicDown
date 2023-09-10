using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicDownWpf.decoder
{
    class ManHuaXin: ParserBase
    {
        public ManHuaXin()
        {
            host = "https://m.mhxin.com/";
            hotUrl = "";
            desc = "漫画芯";
        }

        public override async Task<List<ComicDetailInfo>> GetHotComic(string url)
        {
            var comicList = new List<ComicDetailInfo>();
            var document = await GetDocument(url);
            var elementList = document.GetElementsByClassName("clearfix itemBox");

            foreach (var element in elementList)
            {
                var cominInfo = new ComicDetailInfo()
                {
                    ComicName = element.GetElementsByClassName("title")[0].TextContent, 
                    ImgaeUrl = element.GetElementsByTagName("img")[0].GetAttribute("src"),
                    Href = element.GetElementsByClassName("title")[0].GetAttribute("href"),
                };

                if (!comicList.Exists(t => t.ComicName == cominInfo.ComicName))
                {
                    comicList.Add(cominInfo);
                }
            }

            return comicList;
        }

        public override async Task<ComicDetailInfo> GetComicInfo(string url)
        {
            CommonParser commonParser = new CommonParser(url, CodeName, host);
            CheckComicInfo checkComicInfo = new CheckComicInfo()
            {
                AuthorCss = "body > div.detail-info-1 > div > div > p.detail-info-tip > span:nth-child(1) > a ",
                CharpterCss = ".detail-list-form-con > a",
                TagCss = ".item",
                StatusCss = "body > div.detail-info-1 > div > div > p.detail-info-tip > span:nth-child(2) > span",
                DescriptionCss = ".detail-info-content"
            };
            var info = await commonParser.GetComicInfo(checkComicInfo);
            return info;
        }

        public override Task<SearchResult> GetComicMenu()
        {
            return base.GetComicMenu();
        }
    }
}
