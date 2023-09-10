using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComicPlugin;

namespace ChongChongManHuaParser
{
    public class ChongChongManHuaParser:CommonParser
    {
        public ChongChongManHuaParser()
        {
            m_host = "https://www.m-xfc.com/";
            m_hotUrl = "https://www.m-xfc.com/category/list/5";
            m_searchUrl = "https://www.m-xfc.com/index.php/search?key=";
            m_description = "虫虫漫画";

            HotComicCss(".common-comic-item", "a >img", "a", "p >a");
        }

        public override List<string> GetComicImage(string url, GetOneImgHandler handler)
        {
            return base.GetComicImage(url, handler);
        }
    }
}
