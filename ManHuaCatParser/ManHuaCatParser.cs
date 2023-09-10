using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComicPlugin;

namespace ManHuaCatParser
{
    public class ManHuaCatParser : CommonParser
    {
        public ManHuaCatParser()
        {
            m_host = "https://www.maofly.com/";
            m_hotUrl = "https://www.maofly.com/";
            m_searchUrl = "https://www.maofly.com/search.html?q=";
            m_description = "漫画猫";

            checkComicInfo.AuthorCss = ".pub-duration ";
            checkComicInfo.CharpterCss = "#comic-book-list > div > ol > li > a";
            checkComicInfo.TagCss = ".list-inline-item > a > span";
            checkComicInfo.StatusCss = ".list-inline-item";
            checkComicInfo.DescriptionCss = ".comic_story";

            searchComicParam.FirstCss = "body > div > div.row.m-0 > div > div > div.row.m-0 > div";
            searchComicParam.ComicNameCss = "div > h2";
            searchComicParam.HrefCss = "div > a";
            searchComicParam.ImageUrlCss = "div > a > img";
        }

        public override List<ComicDetailInfo> GetHotComic()
        {
            var comicList = new List<ComicDetailInfo>();
            var document = GetDocument(m_hotUrl);
            if(document == null)
            {
                return comicList;
            }
            var elementList = document.GetElementsByClassName("comicbook-index mb-2 mb-sm-0");

            foreach (var element in elementList)
            {
                var cominInfo = new ComicDetailInfo()
                {
                    ComicName = element.GetElementsByTagName("img")[0].GetAttribute("alt"),
                    ImageUrl = element.GetElementsByTagName("img")[0].GetAttribute("src"),
                    Href = element.Children[0].GetAttribute("href"),
                    CodeName = this.CodeName
                };

                if (!comicList.Exists(t => t.ComicName == cominInfo.ComicName))
                {
                    comicList.Add(cominInfo);
                }
            }

            return comicList;
        }

        private string GetImageData(string data)
        {
            var imgData = data.Substring(data.IndexOf("img_data"));
            imgData = imgData.Substring(imgData.IndexOf(@"""") + 1);
            imgData = imgData.Substring(0, imgData.IndexOf(@""""));
            var ret = ComicPlugin.Utils.LZString.DecompressFromBase64(imgData);
            return ret;
        }

        public override List<string> GetComicImage(string url, GetOneImgHandler handler)
        {
            var list = new List<string>();
            var htmlSting = httpUtils.Get(url);
            var document = GetDocumentByHtmlString(htmlSting);
            var imgArryStr = GetImageData(htmlSting);
            var domain = document.GetElementsByClassName("d-none vg-r-data")[0].GetAttribute("data-chapter-domain");
            httpUtils.SetRefer(url);

            foreach (var i in imgArryStr.Split(','))
            {
                if (i != "")
                {
                    string imgUrl = domain + "/uploads/" + i;

                    if (handler != null)
                    {
                        handler(imgUrl);
                    }
                    list.Add(imgUrl);
                }
            }
            return list;
        }
    }

}
