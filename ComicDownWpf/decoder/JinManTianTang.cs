using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicDownWpf.decoder
{
    class JinManTianTang:ParserBase
    {
        public JinManTianTang()
        {
            host = "https://18comic2.art";
            hotUrl = "https://18comic2.art";
            desc = "禁漫天堂";
        }

        public override async Task<List<ComicDetailInfo>> GetHotComic(string url)
        {
            var comicList = new List<ComicDetailInfo>();
            var htmlString = await GetHtmlStringByCef(url);
            var document = ParseHtmlSync(htmlString);
            var elementList = document.GetElementsByClassName("col-xs-6 col-sm-4 col-md-3 col-lg-2 list-col");

            foreach (var element in elementList)
            {
                var cominInfo = new ComicDetailInfo()
                {
                    ComicName = element.GetElementsByTagName("img")[0].GetAttribute("title"),
                    ImgaeUrl = element.GetElementsByTagName("img")[0].GetAttribute("data-original"),
                    Href = host + element.GetElementsByTagName("a")[0].GetAttribute("href"),
                    CodeName = this.CodeName
                };

                Console.WriteLine("{0}", cominInfo.ImgaeUrl);

                string outText = element.GetElementsByTagName("img")[0].OuterHtml;

                if (!comicList.Exists(t => t.ComicName == cominInfo.ComicName))
                {
                    comicList.Add(cominInfo);
                }
            }

            return comicList;
        }

        public override async Task<ComicDetailInfo> GetComicInfo(string url)
        {
            CommonParser commonParser = new CommonParser(url, host, CodeName, true);
            CheckComicInfo checkComicInfo = new CheckComicInfo()
            {
                AuthorCss = "#intro-block > div:nth-child(5) > span > a",
                CharpterCss = "div.episode >ul >a",
                TagCss = "#intro-block > div:nth-child(4) > span > a:nth-child(1)",
                StatusCss = "#intro-block > div:nth-child(4) > span > a:nth-child(2)",
                DescriptionCss = "#intro-block > div.p-t-5.p-b-5",
                ExtraTagClassName = "col btn btn-primary dropdown-toggle reading"
            };
            var info = await commonParser.GetComicInfo(checkComicInfo);
            return info;
        }

        public override async Task<List<string>> GetComicImage(string url, GetOneImgHandler handler)
        {
            List<string> imageList = new List<string>();
            var htmlString = await HttpGet(url);
            var document = ParseHtmlSync(htmlString);
            var list = document.GetElementsByClassName("row thumb-overlay-albums")[0].GetElementsByTagName("img");

            foreach(var ele in list)
            {
                var imgUrl = ele.GetAttribute("data-original");
                if(imgUrl == null)
                {
                    Console.WriteLine(ele.OuterHtml);
                    continue;
                }
                Console.WriteLine("地址 {0}", imgUrl);

                if(handler != null)
                {
                    handler(imgUrl);
                }
                imageList.Add(imgUrl);

                System.Threading.Thread.Sleep(60);
            }
            return imageList;
        }
    }
}
