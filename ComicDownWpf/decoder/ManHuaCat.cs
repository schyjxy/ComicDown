using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace ComicDownWpf.decoder
{
    class ManHuaCat: ParserBase
    {
        public ManHuaCat()
        {
            host = "https://www.manhuacat.com";
            hotUrl = "https://www.manhuacat.com";
            desc = "漫画猫";
        }

        public override async Task<List<ComicDetailInfo>> GetHotComic(string url)
        {
            var comicList = new List<ComicDetailInfo>();
            var document = await GetDocument(url);
            var elementList = document.GetElementsByClassName("comicbook-index mb-2 mb-sm-0");

            foreach (var element in elementList)
            {
                var cominInfo = new ComicDetailInfo()
                {
                    ComicName = element.GetElementsByTagName("img")[0].GetAttribute("alt"),
                    ImgaeUrl = element.GetElementsByTagName("img")[0].GetAttribute("src"),
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

        protected string DecodeBase64(string encode, string src)
        {
            string decode = "";
            byte[] bytes = Convert.FromBase64String(src);
            decode = Encoding.GetEncoding(encode).GetString(bytes);
            return decode;
        }

        public override async Task<ComicDetailInfo> GetComicInfo(string url)
        {
            CommonParser commonParser = new CommonParser(url, host, CodeName);
            CheckComicInfo checkComicInfo = new CheckComicInfo()
            {
                AuthorCss = ".pub-duration ",
                CharpterCss = "#comic-book-list > div > ol > li > a",
                TagCss = ".list-inline-item > a > span",
                StatusCss = ".list-inline-item",
                DescriptionCss = ".comic_story"
            };
            var info = await commonParser.GetComicInfo(checkComicInfo);
            return info;
        }

        private string GetImageData(string data)
        {
            var imgData = data.Substring(data.IndexOf("img_data"));
            imgData = imgData.Substring(imgData.IndexOf(@"""") + 1);
            imgData = imgData.Substring(0, imgData.IndexOf(@""""));
            var ret = Utils.LZString.DecompressFromBase64(imgData);        
            return ret;
        }

        public override async Task<List<string>> GetComicImage(string url, GetOneImgHandler handler)
        {
            var list = new List<string>();
            var htmlSting = await HttpGet(url);
            var document = GetDocumentByHtmlString(htmlSting);
            var imgArryStr = GetImageData(htmlSting);
            var domain = document.GetElementsByClassName("d-none vg-r-data")[0].GetAttribute("data-chapter-domain");

            foreach(var i in imgArryStr.Split(','))
            {
                if(i != "")
                {
                    string imgUrl = domain + "/uploads/" + i;
                    Console.WriteLine("url:{0}", imgUrl);

                    if (handler != null)
                    {
                        handler(imgUrl);
                    }
                    list.Add(imgUrl);
                    System.Threading.Thread.Sleep(30);
                }
            }
            return list;
        }
    }
}
