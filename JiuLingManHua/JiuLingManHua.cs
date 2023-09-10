using ComicPlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiuLingManHua
{
    public class JiuLingManHua : CommonParser
    {
        public JiuLingManHua()
        {
            m_host = "http://m.90mh.org/";
            m_hotUrl = "http://m.90mh.org/update/";
            m_searchUrl = "http://m.90mh.org/search/?keywords=";
            m_description = "90漫画";

            HotComicCss("#update_list > div > div", "div.itemImg > a > mip-img", "div.itemImg > a", "div.itemTxt > a");
            ComicInfoCss
            (
                "body > div.comic-view.clearfix > div.view-sub.autoHeight > div > dl:nth-child(6) > dd > a",            
                "div.comic-chapters >div > ul > li >a",
                "body > div.comic-view.clearfix > div.view-sub.autoHeight > div > dl:nth-child(5) > dd > a:nth-child(1)",
                "body > div.comic-view.clearfix > div.view-sub.autoHeight > div > dl:nth-child(4) > dd",
                "body > div.comic-view.clearfix > p",
                null
            );

            SerachComicCss("#update_list > div", "div > div.itemImg >a > mip-img", "div > div.itemTxt > a", "div > div.itemTxt > a");
        }

        public override List<string> GetComicImage(string url, GetOneImgHandler handler)
        {
            List<string> urlList = new List<string>();
            var document = GetDocument(url);
            if (document == null)
            {
                return urlList;
            }
            httpUtils.SetRefer(url);
            var nextPage = document.QuerySelector("#chapter-image > a ").Attributes["href"].Value;
            var imgUrl = document.QuerySelector("#chapter-image > a >img ").Attributes["src"].Value;
            handler?.Invoke(imgUrl);
            urlList.Add(imgUrl);
            int num = Convert.ToInt32(document.QuerySelector("#k_total").TextContent);  

            List<string> hrefList = new List<string>();
            int index = url.LastIndexOf(".");
            string baseUrl = url.Substring(0, index);
            string suffix = url.Substring(index, url.Length - index);

            for(int i = 1;i < num;i++)
            {
                string hrefUrl = string.Format("{0}-{1}{2}", baseUrl, i + 1, suffix);
                hrefList.Add(hrefUrl);
            }

            Task[] tasks= new Task[hrefList.Count];
            string[] imageArry = new string[hrefList.Count];

            for(int i = 0; i < tasks.Length;i++)
            {
                int pos = i;
                       
                tasks[pos] = Task.Factory.StartNew(new Action(()=>
                {
                    if (isStop)
                    {
                        return;
                    }

                    string href = hrefList[pos];
                    document = GetDocument(href);
                    if(document == null)
                    {
                        System.Threading.Thread.Sleep(20);
                        document = GetDocument(href);
                    }

                    if (document == null)
                    {
                        return;
                    }

                    imgUrl = document.QuerySelector("#chapter-image > a >img ").Attributes["src"].Value;
                    if(imgUrl == null)
                    {
                        Console.WriteLine("我靠");
                    }
                    imageArry[pos] = imgUrl;
                }));
            }

            for(int i = 0;i < tasks.Length;i++)
            {
                tasks[i].Wait();
                handler?.Invoke(imageArry[i]);
            }

            urlList.AddRange(imageArry);
            return urlList;
        }


    }
}
