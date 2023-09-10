using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Text;

namespace ComicDownWpf.decoder
{
    class ManageBz : ParserBase
    {
        public ManageBz()
        {
            host = "http://www.mangabz.com/";
            cateUrl = "http://mangabz.com/manga-list-0-0-0/";
            hotUrl = "http://www.mangabz.com";
            desc = "MangaBz";
        }

        public override async Task<List<ComicDetailInfo>> GetHotComic(string url)
        {
            //CommonParser common = new CommonParser(url, host);
            //HotComicParam param = new HotComicParam
            //{
            //    HrefCss = "a:nth-child(2)",
            //    ImageUrlCss = "img",
            //    FirstCss = ".index-manga-item",
            //    ComicNameCss = "a:nth-child(2)",
            //};

            //var list = await common.GetHotComic(param);
            //return list;

            var comicList = new List<ComicDetailInfo>();
            var document = await GetDocument(url);
            var elementList = document.QuerySelectorAll(".index-manga-item");

            foreach (var element in elementList)
            {
                var cominInfo = new ComicDetailInfo()
                {
                    ComicName = element.GetElementsByClassName("index-manga-item-title")[0].FirstChild.TextContent,
                    ImgaeUrl = element.GetElementsByTagName("img")[0].GetAttribute("src"),
                    Href = host + element.GetElementsByTagName("a")[0].GetAttribute("href"),
                    CodeName = this.CodeName
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
            CommonParser commonParser = new CommonParser(url, host, CodeName);
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

        private List<string> MakeUrlList(string message)
        {
            var imageCount = 0;
            List<string> urlList = new List<string>();
            Regex countRegex = new Regex(@"MANGABZ_IMAGE_COUNT\s*=(?<count>\d+)");
            Regex pathRegex = new Regex(@"MANGABZ_CURL\s*=\s*""(?<path>[\w\/\-]*)\s*""");
            string text = countRegex.Match(message).Groups["count"].Value;
            imageCount = Convert.ToInt32(text);
            string path = pathRegex.Match(message).Groups["path"].Value;

            for (int i = 0; i < imageCount; i++)
            {
                urlList.Add(string.Format("{0}{1}#ipg{2}", host, path, i + 1));
            }
            return urlList;
        }

        private async Task<string> EnsureGetHtmlString(string url)
        {
            var count = 0;
            var htmlText = await GetHtmlStringByCef(url);
            var document = await ParseHtml(htmlText);

            if (document.GetElementById("cp_image") != null)
            {
                return htmlText;
            }
            else
            {
                bool isSuccess = false;

                do
                {
                    htmlText = await GetHtmlStringAgain();
                    document = await ParseHtml(htmlText);
                    if (document.GetElementById("cp_image") != null)
                    {
                        isSuccess = true;
                        break;
                    }

                    count++;
                    System.Threading.Thread.Sleep(1000);
                } while (!isSuccess);
            }

            if (count > 0)
            {
                Console.WriteLine("执行:{0}次后走出去了", count);
            }
            return htmlText;
        }

        private async Task<string> FindImageUrl(string htmlText)
        {
            bool isExit = false;
            string imageUrl = "";
            var document = await ParseHtml(htmlText);

            try
            {
                do
                {
                    if (document.GetElementById("cp_image") == null)
                    {
                        htmlText = await GetHtmlStringAgain();
                        document = await ParseHtml(htmlText);
                        System.Threading.Thread.Sleep(100);
                        continue;
                    }

                    imageUrl = document.GetElementById("cp_image").GetAttribute("src");
                    isExit = true;

                } while (!isExit);
            }
            catch
            {
                ;
            }

            return imageUrl;
        }

        public override async Task<List<string>> GetComicImage(string url, GetOneImgHandler handler)
        {
            var imgUrl = "";
            var imageList = new List<string>();
            var htmlText = await EnsureGetHtmlString(url);
            var document = await ParseHtml(htmlText);
            var urlList = MakeUrlList(htmlText);

            DateTime time = DateTime.Now;

            foreach (var i in urlList)
            {
                htmlText = await GetHtmlStringAgain();
                imgUrl = await FindImageUrl(htmlText);

                if(handler != null)
                {
                    handler(imgUrl);
                }
                imageList.Add(imgUrl);
                ExexuteJs("ShowNext()");
            }

            Console.WriteLine("获取所有图片耗时 {0} ms", DateTime.Now.Subtract(time).TotalMilliseconds);
            return imageList;
        }

        public override Task<SearchResult> SearchComic(string keyWord)
        {
            string url = string.Format("http://www.mangabz.com/search?title={0}", System.Web.HttpUtility.UrlEncode(keyWord));
            CommonParser parser = new CommonParser(url, host, CodeName);
            ComicParam param = new ComicParam();
            param.FirstCss = ".mh-list > li";
            param.HrefCss = "div > a";
            param.ImageUrlCss = "div > a > img";
            param.ComicNameCss = ".title";
            param.FindResultCss = ".result-title";
            param.PageCss = ".page-pagination > ul >li > a";
            return parser.SearchComic(param);         
        }

        public override async Task<List<ComicDetailInfo>> GoToPage(string url)
        {
            CommonParser parser = new CommonParser(url, host, CodeName);
            ComicParam param = new ComicParam();
            param.FirstCss = ".mh-list > li";
            param.HrefCss = "div > a";
            param.ImageUrlCss = "div > a > img";
            param.ComicNameCss = ".title";
            param.FindResultCss = ".result-title";
            param.PageCss = ".page-pagination > ul >li > a";
            var list = await parser.GoToPage(param);
            return list;
        }

        public override async Task<SearchResult> GetComicMenu()
        {
            CommonParser parser = new CommonParser(cateUrl, host, CodeName);
            ComicParam param = new ComicParam();
            param.FirstCss = ".mh-list > li";
            param.HrefCss = "div > a";
            param.ImageUrlCss = "div > a > img";
            param.ComicNameCss = ".title";
            param.FindResultCss = ".result-title";
            param.PageCss = ".page-pagination > ul >li > a";
            param.MenuCss = ".class-line > a";
            var list = await parser.GetComicMenu(param);
            return list;
        }

    }
}
