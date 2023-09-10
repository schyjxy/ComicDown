using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using AngleSharp.Scripting;

namespace ComicDownTools
{
    class RetInfo
    { 
        public string ImageCss { get; set; }
    }
    class CssFinder
    {
        protected IHtmlDocument GetDocumentByHtmlString(string text)
        {
            var context = BrowsingContext.New(Configuration.Default);
            var parser = context.GetService<IHtmlParser>();
            var document = parser.ParseDocument(text);
            return document;
        }

        //protected IHtmlDocument GetJs()
        //{
        //    IConfiguration config = Configuration.Default.With();
        //}

        public async Task<RetInfo> SearchHot(string url)
        {
            RetInfo info = new RetInfo();
            string html = await HttpUtil.Get(url);
            var document = GetDocumentByHtmlString(html);

            if (document == null)
            {
                return info;
            }

            string[] tag = { "img"};

            var nodeCollection = document.QuerySelectorAll("img");

            foreach (IElement e in nodeCollection)
            {
                var parent = e.Parent as IElement;
                
                if (parent != null && parent.NodeName == "A")
                {
                    if (parent.Attributes["title"] != null)
                    {

                    }

                    Console.WriteLine("{0}, {1}", parent.Attributes["title"].Value, e.Attributes["src"].Value);
                }
            }

            return info;
        }
    }
}
