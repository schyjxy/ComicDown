using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using System.Reflection;
using System.IO;
using AngleSharp.Dom;
using System.Xml.Linq;
using System.Xml;
using System.Threading;

namespace ComicPlugin
{
    //尽量所有任务在父类这做
    public class CommonParser : IComicDecoder
    {
        protected string m_hotUrl;
        protected string m_searchUrl;
        protected string m_description;
        protected string m_codeName;
        protected string m_host;
        protected string m_cateUrl;

        protected HttpUtils httpUtils;
        protected ComicParam hotComicParam;
        protected CheckComicInfo checkComicInfo;
        protected ComicParam catelogParam;
        protected ComicParam searchComicParam;
        protected ParserParam parserParam;

        protected static bool isStop = false;
        protected Encoding encode = null;
        protected
        private string m_errorMsg = null;

        public CommonParser()
        {
            httpUtils = new HttpUtils();
            hotComicParam = new ComicParam();
            checkComicInfo = new CheckComicInfo();
            searchComicParam = new ComicParam();
            catelogParam = new ComicParam();
            parserParam = new ParserParam();
            m_codeName = this.GetType().Name;
        }

        public string SearchUrl { get { return m_searchUrl; } }

        public string Description { get { return m_description; } }

        public string CodeName
        {
            get { return m_codeName; }
            set { m_codeName = value; }
        }

        public static void SetStop(bool val)
        {
            isStop = val;
        }

        protected void SetErrorString(string errMessage)
        {
            m_errorMsg = errMessage;
        }
        public string GetError()
        {
            return m_errorMsg;
        }

        protected void HotComicCss(string firstCss, string imgurlCss, string hrefCss, string nameCss)
        {
            hotComicParam.FirstCss = firstCss;
            hotComicParam.ImageUrlCss = imgurlCss;
            hotComicParam.HrefCss = hrefCss;
            hotComicParam.ComicNameCss = nameCss;
        }

        protected void ComicInfoCss(string authorCss, string charpterCss, string tagCss, string statusCss, string descCss, string extraTagCss)
        {
            checkComicInfo.AuthorCss = authorCss;
            checkComicInfo.CharpterCss = charpterCss;
            checkComicInfo.TagCss = tagCss;
            checkComicInfo.StatusCss = statusCss;
            checkComicInfo.DescriptionCss = descCss;
            checkComicInfo.ExtraTagClassName = extraTagCss;
        }

        protected void SerachComicCss(string firstCss, string imgurlCss, string hrefCss, string nameCss)
        {
            searchComicParam.FirstCss = firstCss;
            searchComicParam.ComicNameCss = nameCss;
            searchComicParam.HrefCss = hrefCss;
            searchComicParam.ImageUrlCss = imgurlCss;
        }

        public string HotComicUrl
        {
            get { return m_hotUrl; }
        }

        protected IHtmlDocument GetDocumentByHtmlString(string text)
        {
            var context = BrowsingContext.New(Configuration.Default);
            var parser = context.GetService<IHtmlParser>();
            var document = parser.ParseDocument(text);
            return document;
        }

        public virtual SearchResult GetComicMenu()//获取漫画分类目录，这个带页面跳转
        {
            SearchResult result = new SearchResult();

            return result;
        }

        public virtual List<ComicDetailInfo> GoToPage(string url)
        {
            Console.WriteLine("解析器：{0} 实现方法:{1}", CodeName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            throw new NotImplementedException();
        }

        protected IHtmlDocument GetDocument(string url)
        {
            var htmlString = httpUtils.Get(url, encode);

            if (htmlString == null || htmlString == "")
            {
                Console.WriteLine("GetDocumen 报错");
                return null;
            }

            var document = GetDocumentByHtmlString(htmlString);
            return document;
        }

        private string DealText(string text)
        {
            string ret = text;
            string[] arry = {"\n", "  ", "<em>", "</em>" };

            for(int i = 0;i < arry.Length;i++)
            {
                ret = ret.Replace(arry[i], "");
            }
            return ret;
        }

        protected virtual bool ParseComicTitle(IElement node, string css, ComicDetailInfo cominInfo)
        {
            string[] nameTag = { "alt", "title" };
            IElement nameNode = null;
            bool result = false;

            if (node == null)
            {
                return false;
            }

            if (css != null)
            {
                nameNode = css != null ? node.QuerySelector(css) : node;
                if (nameNode == null)
                {
                    return false;
                }
            }
            else
            {
                nameNode = node;
            }

            if (nameNode == null)
            {
                return false;
            }

            cominInfo.ComicName = nameNode.TextContent.Replace("\n", "");

            if (cominInfo.ComicName == null || cominInfo.ComicName == "")
            {
                for (int i = 0; i < nameTag.Length; i++)
                {
                    if (nameNode.Attributes[nameTag[i]] == null)
                    {
                        continue;
                    }

                    if (nameNode.Attributes[nameTag[i]].Value != null)
                    {
                        cominInfo.ComicName = nameNode.Attributes[nameTag[i]].Value;
                        result = true;
                        break;
                    }
                }
            }
            else
            {
                result = true;
            }

            if (result)
            {
                cominInfo.ComicName = DealText(cominInfo.ComicName);
            }

            return result;
        }

        virtual protected string CheckUrl(string href, string host)
        {
            string url = "";

            if (href.Contains("http"))
            {
                url = href;
            }
            else
            {
                if (host.EndsWith("/"))
                {
                    if (href.StartsWith("/"))
                    {
                        href = href.Substring(1);
                    }
                    url = host + href;
                }
                else
                {
                    if (href.StartsWith("/"))
                    {
                        url = host + href;
                    }
                    else
                    {
                        url = host + "/" + href;
                    }

                }
            }

            return url;
        }

        protected virtual bool ParseComicImgUrl(IElement node, string css,  ComicDetailInfo cominInfo)
        {
            IElement imgNode = null;
            bool hasFind = false;
            string[] srcTag = { "data-original", "data-src", "img-src", "src" };
            if(node == null)
            {
                return false;
            }

            if(css != null)
            {
                if (node.QuerySelector(css) == null)
                {
                    return false;
                }
                imgNode = node.QuerySelector(css); 
            }
            else
            {
                imgNode = node;
            }

            foreach (var tag in srcTag)
            {
                var src = imgNode.GetAttribute(tag);

                if (src != null)
                {
                    cominInfo.ImageUrl = CheckUrl(src, m_host);
                    hasFind = true;
                    break;
                }
            }
            return hasFind;
        }

        protected virtual bool ParseComicHref(IElement node, string css, ComicDetailInfo cominInfo)
        {
            if(node == null)
            {
                return false;
            }

            if (css != null)
            {
                if (node.QuerySelector(css) == null)
                {
                    return false;
                }

                var baseHref = css != null ? node.QuerySelector(css).GetAttribute("href") : node.Attributes["href"].Value;

                if (baseHref.Contains("http"))
                {
                    cominInfo.Href = baseHref;
                }
                else
                {
                    cominInfo.Href = CheckUrl(baseHref, m_host);
                }
            }
            else
            {
                if (node.Attributes["href"] != null)
                {
                    cominInfo.Href = node.Attributes["href"].Value;
                }
                else
                {
                    return false;
                }
            }
     
            return true;
        }

        public virtual List<ComicDetailInfo> GetHotComic()
        {
            var comicList = new List<ComicDetailInfo>();
            var document = GetDocument(m_hotUrl);

            if (document == null)
            {
                SetErrorString(string.Format("{0} GetDocument Error", CodeName));
                return comicList;
            }

            var nodeList = document.QuerySelectorAll(hotComicParam.FirstCss);   

            foreach (var node in nodeList)
            {                      
                var cominInfo = new ComicDetailInfo();
                cominInfo.CodeName = m_codeName;
                ParseComicImgUrl(node, hotComicParam.ImageUrlCss, cominInfo);
                if(cominInfo.ImageUrl == null)
                {
                    continue;
                }

                if(ParseComicTitle(node, hotComicParam.ComicNameCss, cominInfo) && ParseComicHref(node, hotComicParam.HrefCss, cominInfo))
                {
                    if (!comicList.Exists(t => t.ComicName == cominInfo.ComicName))
                    {
                        comicList.Add(cominInfo);
                    }
                } 
            }
            return comicList;
        }

        protected virtual void ParseCharpter(IHtmlDocument document, ComicDetailInfo comicInfo)
        {
            var list = document.QuerySelectorAll(checkComicInfo.CharpterCss);
            int count = 1;
            
            foreach (var i in list)
            {
                string href;
                string hrefTemp;
                string name = Regex.Replace(i.TextContent, @"[\r\n\s]*", "");
                hrefTemp = i.GetAttribute("href");

                if (hrefTemp.Contains("http") || hrefTemp.Contains("https"))
                {
                    href = hrefTemp;
                }
                else
                {
                    href = m_host + hrefTemp;
                }

                if(name == "")
                {
                    name = count.ToString().PadLeft(3, '0');
                }

                if (!comicInfo.Charpter.ContainsKey(name))
                {
                    comicInfo.Charpter.Add(name, href);
                }
            }
        }

        private string FilterString(string data)
        {
            string ret = "";
            MatchCollection collection = Regex.Matches(data, @"(?<data>\w+)");
            foreach (Match m in collection)
            {
                ret += m.Groups["data"].Value + " ";
            }

            return ret;
        }

        protected virtual void ParseAuthor(IHtmlDocument document, ComicDetailInfo comicInfo)
        {
            if (checkComicInfo.AuthorCss != null)
            {
                try
                {
                    var node = document.QuerySelector(checkComicInfo.AuthorCss);
                    if (node != null)
                    {
                        var temp = node.TextContent.Trim();
                        temp = Regex.Replace(temp, @"[\r\n]*", "").Replace("作者：", "");
                        comicInfo.Author = FilterString(temp);
                    }
                    else
                    {
                        comicInfo.Author = "无";
                    }
                }
                catch (Exception ex)
                {
                    comicInfo.Author = "无";
                }

            }
        }

        protected virtual void ParseStatus(IHtmlDocument document, ComicDetailInfo comicInfo)
        {
            if (checkComicInfo.StatusCss != null)
            {
                var node = document.QuerySelector(checkComicInfo.StatusCss);
                if (node != null)
                {
                    var temp = node.TextContent.Trim();
                    temp = Regex.Replace(temp, @"[\r\n]*", "").Replace("\n", "").Replace("  ", "");
                    comicInfo.status = "状态：" + temp;
                }
                else
                {
                    comicInfo.status = "状态：无";
                }
            }
        }

        protected virtual void ParseDescription(IHtmlDocument document, ComicDetailInfo comicInfo)
        {
            if (checkComicInfo.DescriptionCss != null)
            {
                var desc = "";
                var node = document.QuerySelector(checkComicInfo.DescriptionCss);
                if(node != null)
                {
                    desc = node.TextContent.Trim();
                    desc = desc.Replace("  ", "").Replace("\n", "");
                }
                else
                {
                    desc = "简介:无";
                }            
                comicInfo.Description = desc;
            }
        }

        protected virtual void ParseTag(IHtmlDocument document, ComicDetailInfo comicInfo)
        {
            if (checkComicInfo.TagCss != null)
            {
                var temp = document.QuerySelector(checkComicInfo.TagCss);

                if (temp != null)
                {
                    var tag = temp.TextContent.Trim();
                    tag = Regex.Replace(tag, @"[\r\n]*", "");
                    comicInfo.Tag = "题材：" + tag;
                }
                else
                {
                    comicInfo.Tag = "题材：无";
                }

                comicInfo.Tag.Replace("  ", "").Replace("\n", "");
            }
        }

        public virtual ComicDetailInfo GetComicInfo(string url)
        {
            int failCount = 0;
            var comicInfo = new ComicDetailInfo();
            comicInfo.CodeName = CodeName;
            IHtmlDocument document = GetDocument(url);
         
            while((document = GetDocument(url)) == null)
            {
                failCount++;
                if(failCount > 2)
                {
                    break;
                }

                Thread.Sleep(1000);
            }

            if (document == null)
            {
                SetErrorString(string.Format("{0} GetComicInfo: GetDocument Error", CodeName));
                return comicInfo;
            }

            parserParam.ComicInfoUrl = url;
            ParseAuthor(document, comicInfo);
            ParseStatus(document, comicInfo);
            ParseDescription(document, comicInfo);
            ParseTag(document, comicInfo);     
            ParseCharpter(document, comicInfo);

            if (checkComicInfo.ExtraTagClassName != null && checkComicInfo.ExtraTagClassName.Length > 0)
            {
                string key = "全一话";
                var nodeCollect = document.GetElementsByClassName(checkComicInfo.ExtraTagClassName);
                if(nodeCollect != null && nodeCollect.Length > 0 && nodeCollect[0].GetAttribute("href") != null)
                {
                    string value = m_host + nodeCollect[0].GetAttribute("href");
                    comicInfo.Charpter.Add(key, value);
                }          
            }
         
            return comicInfo;
        }

        public virtual List<string> GetComicImage(string url, GetOneImgHandler handler)
        {
            Console.WriteLine("解析器：{0} 未实现方法:{1}", CodeName, System.Reflection.MethodBase.GetCurrentMethod().Name);
            throw new NotImplementedException();
        }

        public virtual SearchResult SearchComic(string keyWord)
        {
            SearchResult result = new SearchResult();
            var searchUrl = m_searchUrl + keyWord;
            var htmlString = httpUtils.Get(searchUrl, encode);
            var document = GetDocumentByHtmlString(htmlString);

            if (document == null)
            {
                result.IsSuccess = false;
                return result;
            }

            var nodeList = document.QuerySelectorAll(searchComicParam.FirstCss);

            //填充查找结果
            foreach (var node in nodeList)
            {
                var info = new ComicDetailInfo();
                info.CodeName = CodeName;

                if(ParseComicTitle(node, searchComicParam.ComicNameCss, info) && ParseComicImgUrl(node, searchComicParam.ImageUrlCss, info)
                    && ParseComicHref(node, searchComicParam.HrefCss, info))
                {
                    result.DetailInfoList.Add(info);
                }             
            }

            result.Count = result.DetailInfoList.Count;
            return result;
        }

        public virtual void Dispose()
        {
            isStop = true;
        }

        public virtual bool DownLoadImage(string url, string path)
        {
            return httpUtils.DownLoadImage(url, path);
        }
    }
}
