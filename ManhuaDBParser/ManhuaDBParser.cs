using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComicPlugin;
using System.IO;
using Newtonsoft.Json;


namespace ManhuaDBParser
{

    public class ManhuaDBParser:CommonParser
    {
        public ManhuaDBParser()
        {
            m_hotUrl = "https://www.manhuadb.com/update.html";
            m_host = "https://www.manhuadb.com/";
            m_searchUrl = "https://www.manhuadb.com/search?q=s%27s";
            m_description = "漫画DB";

            var baseCss = "body > div.container-fluid.comic-detail.px-0.px-sm-3.pt-sm-3 > div.row.m-0 > div > div.comic-main-section.bg-white.p-3 > div.row.m-0 >div >div";

            hotComicParam.FirstCss = baseCss;
            hotComicParam.ImageUrlCss = baseCss + "> a >img";
            hotComicParam.HrefCss = baseCss + "> a";
            hotComicParam.ComicNameCss = baseCss + "> a";

            checkComicInfo.AuthorCss = "body > div.container-fluid.comic-detail.px-0.px-lg-3.px-xl-0 > div.row.m-0.mt-lg-3 > div.col-lg-9.px-0 > div.comic-main-section.bg-white.p-0.p-sm-3.d-sm-flex > div > ul.creators > li > a";
            checkComicInfo.CharpterCss = @"#comic-book-list >div >ol >li >a";
            checkComicInfo.TagCss = "#navbarSupportedContent > ul > li.nav-item.dropdown > ul > li:nth-child(1) > a";
            checkComicInfo.DescriptionCss = "body > div.container-fluid.comic-detail.px-0.px-lg-3.px-xl-0 > div.row.m-0.mt-lg-3 > div.col-lg-9.px-0 > div.comic-main-section.bg-white.p-0.p-sm-3.d-sm-flex > div > p";

            m_searchUrl = "https://www.manhuadb.com/search?q=";
            searchComicParam.FirstCss = "body > div.container-fluid.comic-detail.px-0.px-sm-3.pt-sm-3 > div.row.m-0 > div > div.comic-main-section.bg-white.p-3 > div.row.m-0 > div";
            searchComicParam.ComicNameCss = "div > div > h2";
            searchComicParam.ImageUrlCss = "div > div > a > img";
            searchComicParam.HrefCss = "div > div > a";
        }

        public override List<string> GetComicImage(string url, GetOneImgHandler handler)
        {
            var errorCount = 0;
            var imgList = new List<string>();
            var htmlString = httpUtils.Get(url);

            while (htmlString == null)
            {
                System.Threading.Thread.Sleep(20);
                htmlString = httpUtils.Get(url);
                errorCount++;
                if (errorCount == 5)
                {
                    return imgList;
                }
            }

            var document = GetDocumentByHtmlString(htmlString);
            var temp = htmlString.Substring(htmlString.IndexOf("img_data"));
            temp = temp.Substring(temp.IndexOf("'") + 1);
            temp = temp.Substring(0, temp.IndexOf("'"));
            var imgData = ComicPlugin.Utils.Base64.DecodeBase64(Encoding.UTF8, temp);
            var jsonArry = JsonConvert.DeserializeAnonymousType(imgData, new[] { new { img = "", p = 0 } }.ToList());
            var css = "body > div.container-fluid.comic-detail.p-0 > div.d-none.vg-r-data";
            var node =  document.QuerySelector(css);
            var imgPre = node.GetAttribute("data-img_pre");
            var host = node.GetAttribute("data-host");

            foreach (var i in jsonArry)
            {
                var trueUrl = host + imgPre + i.img;
                imgList.Add(trueUrl);

                if(handler != null)
                {
                    handler(trueUrl);
                }
            }

            return imgList;
        }
    }
}
