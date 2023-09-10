using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Text.RegularExpressions;

namespace ComicDownWpf.decoder
{
    enum GetElementType
    {
        getElementById,
        getElementByClassName,
        getElementsByTagName,
    }

    enum ResultType
    {
        Attribute,
        TextContent
    }

    class ComicParam
    {
        public string FirstCss { get; set; }
        public string ComicNameCss { get; set; }
        public string ImageUrlCss { get; set; }
        public string HrefCss { get; set; }
        public string FindResultCss { get; set; }
        public string MenuCss { get; set; }
        public string PageCss { get; set; }

        public List<string> TagList { get; set;}
    }

    class GetElementPolicy
    {
        public GetElementType Type { get; set; }
        public string TagClassName { get; set; }
        public string IdName { get; set; }
        public string TagName { get; set; }
        public int Index;

    }


    class CheckComicInfo
    {
        public string AuthorCss { get; set; }
        public string TagCss { get; set; }
        public string DescriptionCss { get; set; }
        public string StatusCss { get; set; }
        public string CharpterCss { get; set; }

        public string ExtraTagClassName { get; set; }
    }

    class CommonParser
    {
        string host = "";
        string currentUrl;
        bool isUseCef = false;
        IHtmlDocument document;
        ParserBase parser = new ParserBase();
        AutoResetEvent autoResetEvent;

        public enum ParseType
        {
            HotComic,
            ComicInfo,
            ComicImageList
        }

        public string CodeName { get; set; }

        private IHtmlDocument GetDocument(string text)
        {
            var context = BrowsingContext.New(Configuration.Default);
            var parser = context.GetService<IHtmlParser>();
            document = parser.ParseDocument(text);
            return document;
        }

        private async Task<bool> Init(string url)
        {
            var htmlString = "";
            currentUrl = url;

            if (!isUseCef)
            {
                htmlString = await parser.HttpGet(url);
            }
            else
            {
                htmlString = await parser.GetHtmlStringByCef(url);
            }
            GetDocument(htmlString); 
            return true;
        }

        public CommonParser(string url, string host, string codeName, bool isUseCef = false)
        {         
            this.host = host;
            this.isUseCef = isUseCef;
            this.CodeName = codeName;
            parser.SetHepler(ParserManager.GetHelper());
            autoResetEvent = new AutoResetEvent(false);

            Task task = new Task(async () =>
            {
                await Init(url);
                autoResetEvent.Set();
            });
            task.Start();
            task.Wait();
            autoResetEvent.WaitOne();
        }

        public async Task<List<ComicDetailInfo>> GetHotComic(ComicParam param)
        {
            var comicList = new List<ComicDetailInfo>();
            var nodeList = document.QuerySelectorAll(param.FirstCss);

            foreach (var node in nodeList)
            {
                if (node.QuerySelector(param.ImageUrlCss) == null)
                {
                    continue;
                }

                var cominInfo = new ComicDetailInfo();
                cominInfo.ComicName = node.QuerySelector(param.ComicNameCss).TextContent;
                cominInfo.CodeName = CodeName;
                string[] srcTag = { "src", "data-src", "img-src"};

                foreach (var tag in srcTag)
                {
                    var src = node.QuerySelector(param.ImageUrlCss).GetAttribute(tag);
                    if (src != null)
                    {
                        cominInfo.ImgaeUrl = src;
                        break;
                    }
                }
               
                cominInfo.Href = host + node.QuerySelector(param.HrefCss).GetAttribute("href");

                if (!comicList.Exists(t => t.ComicName == cominInfo.ComicName))
                {
                    comicList.Add(cominInfo);
                }
            }
            return comicList;
        }

        private string DealSpace(string input)
        {
            return "";
        }

        private async Task<bool> CheckCss(string css)
        {
            var str = await parser.GetHtmlStringAgain();
            while (true)
            {               
                GetDocument(str);

                if(document.QuerySelector(css) == null)
                {
                    //Console.WriteLine("继续等待");
                    Thread.Sleep(500);
                    str = await parser.GetHtmlStringAgain();
                }
                else
                {
                    break;
                }
                
            }
            
            return true;
        }

        public async Task<ComicDetailInfo> GetComicInfo(CheckComicInfo checkComicInfo)
        {

            var comicInfo = new ComicDetailInfo();
            if(isUseCef && document.QuerySelector(checkComicInfo.TagCss) == null)
            {
                await CheckCss(checkComicInfo.StatusCss);
            }
                      
            if(checkComicInfo.AuthorCss != null)
            {
                try
                {
                    var node = document.QuerySelector(checkComicInfo.AuthorCss);
                    if (node != null)
                    {
                        var temp = node.TextContent.Trim();
                        temp = Regex.Replace(temp, @"[\r\n]*", "");
                        comicInfo.Author = "作者: " + temp;
                    }
                    else
                    {
                        comicInfo.Author = "作者: 无";
                    }
                }
                catch(Exception ex)
                {
                    comicInfo.Author = "作者: 无";
                }
                  
                
            }

            if(checkComicInfo.DescriptionCss != null)
            {
                comicInfo.Description = "简介: " + document.QuerySelector(checkComicInfo.DescriptionCss).TextContent.Trim();
            }

            if(checkComicInfo.StatusCss != null)
            {
                var node = document.QuerySelector(checkComicInfo.StatusCss);
                if(node != null)
                {
                    var temp = node.TextContent.Trim();
                    temp = Regex.Replace(temp, @"[\r\n]*", "");
                    comicInfo.status = "状态：" + temp;
                }
                else
                {
                    comicInfo.status = "状态：无";
                }
               
            }
        
            if (checkComicInfo.TagCss != null)
            {
                var temp = document.QuerySelector(checkComicInfo.TagCss);

                if(temp != null)
                {
                    var tag = temp.TextContent.Trim();
                    tag = Regex.Replace(tag, @"[\r\n]*", "");
                    comicInfo.Tag = "题材：" + tag;
                }
                else
                {
                    comicInfo.Tag = "题材：无";
                }                                    
            }

            var list = document.QuerySelectorAll(checkComicInfo.CharpterCss);

            foreach(var i in list)
            {
                string href;
                string hrefTemp;
                string name = Regex.Replace(i.TextContent, @"[\r\n]*", "");
                hrefTemp = i.GetAttribute("href");

                if (hrefTemp.Contains("http") || hrefTemp.Contains("https"))
                {
                    href = hrefTemp;
                }
                else
                {
                    href = host + hrefTemp;
                }
                

                if (!comicInfo.Charpter.ContainsKey(name))
                {
                    comicInfo.Charpter.Add(name, href);
                }
            }

            if(checkComicInfo.ExtraTagClassName != null)
            {
                string key = "全一话";
                string value = host +  document.GetElementsByClassName(checkComicInfo.ExtraTagClassName)[0].GetAttribute("href");
                comicInfo.Charpter.Add(key, value);
            }

            comicInfo.CodeName = CodeName;
            return comicInfo;
        }

        private List<ComicDetailInfo> GetComicDetailList(ComicParam param)
        {
            var comicList = new List<ComicDetailInfo>();
            var nodeList = document.QuerySelectorAll(param.FirstCss);
            string[] srcTag = { "src", "data-src", "img-src" };

            foreach (var node in nodeList)
            {
                if (node.QuerySelector(param.ImageUrlCss) == null)
                {
                    continue;
                }

                var cominInfo = new ComicDetailInfo();
                cominInfo.CodeName = CodeName;
                cominInfo.ComicName = node.QuerySelector(param.ComicNameCss).TextContent.Trim();
               
                foreach (var tag in srcTag)
                {
                    var src = node.QuerySelector(param.ImageUrlCss).GetAttribute(tag);
                    if (src != null)
                    {
                        cominInfo.ImgaeUrl = src;
                        break;
                    }
                }

                cominInfo.Href = host + node.QuerySelector(param.HrefCss).GetAttribute("href");

                if (!comicList.Exists(t => t.ComicName == cominInfo.ComicName))
                {
                    comicList.Add(cominInfo);
                }

            }

            return comicList;

        }

        public async Task<List<ComicDetailInfo>> GoToPage(ComicParam param)
        {
            return GetComicDetailList(param);
        }

        public async Task <SearchResult> SearchComic(ComicParam param)
        {
            var result = new SearchResult();
            result.DetailInfoList = GetComicDetailList(param);
            var temp = document.QuerySelector(param.FindResultCss).TextContent;
            var countStr = Regex.Match(temp, @"(?<data>\d+)").Groups["data"].Value;

            if(countStr != "")
            {
                result.Count = Convert.ToInt32(countStr);
            }

            result.PageDict = new Dictionary<string, string>();
            foreach( var i in document.QuerySelectorAll(param.PageCss))
            {
                string key   =  Regex.Match(i.TextContent.Trim(), @"(?<data>\d+)").Groups["data"].Value;
                if(key == "")
                {
                    continue;
                }
                string value = host + i.GetAttribute("href");

                if(!result.PageDict.ContainsKey(key))
                {
                    result.PageDict.Add(key, value);
                }
            }
            result.Type = SearchResult.SearchType.KeyWord;
            result.CodeName = CodeName;
            return result;
        }

        public async Task<SearchResult> GetComicMenu(ComicParam param)
        {
            var result = new SearchResult();
            result.DetailInfoList = GetComicDetailList(param);
            result.PageDict = new Dictionary<string, string>();
            result.MenuDict = new Dictionary<string, string>();

            foreach (var i in document.QuerySelectorAll(param.PageCss))
            {
                string key = Regex.Match(i.TextContent.Trim(), @"(?<data>\d+)").Groups["data"].Value;
                if (key == "")
                {
                    continue;
                }
                string value = host + i.GetAttribute("href");

                if (!result.PageDict.ContainsKey(key))
                {
                    result.PageDict.Add(key, value);
                }
            }

            foreach (var i in document.QuerySelectorAll(param.MenuCss))
            {
                string key = i.TextContent.Trim();
                if (key == "")
                {
                    continue;
                }
                string value = host + i.GetAttribute("href");

                if (!result.MenuDict.ContainsKey(key))
                {
                    result.MenuDict.Add(key, value);
                }
            }
            result.Count = result.PageDict.Count;
            result.Type = SearchResult.SearchType.Menu;
            result.CodeName = CodeName;
            return result;
        }
    }
}
