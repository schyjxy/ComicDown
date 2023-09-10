using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ComicPlugin;

namespace KuKuDongManParser
{
    public class KuKuDongManParser : CommonParser
    {
        public struct ImageListResult
        {
            public List<string> urlList;
            public int index;
        }

        private Dictionary<string, string> paramDict;

        public KuKuDongManParser()
        {
            m_host   = "http://m.ikukudm.com/";
            m_hotUrl = "http://m.ikukudm.com/";
            m_searchUrl = "http://so.ikukudm.com/m_search.asp?kw=";
            m_description = "KuKu动漫";
            encode = Encoding.GetEncoding("gbk");

            HotComicCss(".imgBox > ul > li", "a:nth-child(1) > img", "a:nth-child(1)", "a:nth-child(2)");
            ComicInfoCss(".txtItme:nth-child(1)", ".classopen > li >a", ".txtItme:nth-child(2)", ".txtItme:nth-child(3)", ".txtDesc.autoHeight", "");
            SerachComicCss(".imgBox > ul > li", "a > img", "a", "a:nth-child(2)");     
            SetImageServer();
        }
         
        protected ImageListResult DoHttpGet(string url, int pageIndex)//这个已经被修改了，需要重新找到漫画地址
        {
            ImageListResult result = new ImageListResult();
            result.index = pageIndex;

            try
            {
                var htmlString = httpUtils.Get(url, Encoding.GetEncoding("gbk"));
                var document = GetDocumentByHtmlString(htmlString);
                var script = document.QuerySelector("body > div > script").TextContent;//获取脚本
                
                foreach(var i in paramDict)
                {
                    script = script.Replace(i.Key, i.Value);
                }

                var str = Regex.Match(script, @"<img\s*src\s*=\s*'(?<url>[""\s\w.:/ +]*)'").Groups["url"].Value;
                var imgUrl = str.Replace("+", "").Replace(@"""", "");
                result.urlList = new List<string>();
                result.urlList.Add(imgUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0}", ex.StackTrace);
            }

            return result;
        }

        private void SetImageServer()
        {
            paramDict = new Dictionary<string, string>();
            paramDict.Add("m201304d", "http://bili2.goto123.xyz/");
            paramDict.Add("m201001d", "http://bili2.goto123.xyz/");
            paramDict.Add("m200911d", "http://bili2.goto123.xyz/");
            paramDict.Add("k0910k", "http://bili2.goto123.xyz/");
            paramDict.Add("m2022", "http://bili2.goto123.xyz/");

            paramDict.Add("server0", "http://tu22.goto123.xyz/");        
            paramDict.Add("m2007", "http://tu22.goto123.xyz/");
            paramDict.Add("server", "http://tu22.goto123.xyz/");
        }

        public override List<string> GetComicImage(string url, GetOneImgHandler handler)
        {
            var pageCount = 0;
            var htmlString = httpUtils.Get(url, Encoding.GetEncoding("gbk"));
            var document = GetDocumentByHtmlString(htmlString);         

            var baseUrl = url.Substring(0, url.LastIndexOf("/"));
            var pageString = document.QuerySelector("body > div > div.bottom > ul > li:nth-child(2)").TextContent;
            pageString = pageString.Substring(pageString.IndexOf("/") + 1);
            pageCount = Convert.ToInt32(pageString);
            string[] imageArry = new string[pageCount];
            Task []taskArry = new Task[pageCount];

            for(int i = 0;i < taskArry.Length;i++)
            {
                int page = i + 1;
                string href = string.Format("{0}/{1}.htm", baseUrl, page);

                taskArry[i] = new Task(new Action(() =>
                {                                      
                    if(isStop)
                    {
                        return;
                    }

                    var result = DoHttpGet(href, page);

                    if(result.urlList.Count > 0)
                    {
                        if (handler != null)
                        {
                            handler(result.urlList[0]);
                        }

                        imageArry[page - 1] = result.urlList[0];
                    }

                }));

                taskArry[i].Start();
            }

            Task.WaitAll(taskArry);
            var list = imageArry.ToList<string>();
            return list;
        }

    }
}
