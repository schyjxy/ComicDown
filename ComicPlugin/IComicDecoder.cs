using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicPlugin
{
    public delegate void GetOneImgHandler(string imgUrl);

    public class SearchResult
    {
        public bool IsSuccess { get; set; }

        public enum SearchType
        {
            Menu,
            KeyWord
        }

        public SearchResult()
        {
            IsSuccess = true;
            DetailInfoList = new List<ComicDetailInfo>();
            MenuDict = new Dictionary<string, string>();
            PageDict = new Dictionary<string, string>();

        }

        public SearchType Type { get; set; }
        public int Count { get; set; }
        public List<ComicDetailInfo> DetailInfoList { get; set; }
        public Dictionary<string, string> MenuDict { get; set; } //
        public Dictionary<string, string> PageDict { get; set; }//翻页要用的键值对
    }

    public class GetImage
    {
        public GetOneImgHandler imgHandler;
    }

    public class ParserParam
    {
        public string ComicInfoUrl { get; set; }
    }

    public class ComicDetailInfo
    {
        SortedDictionary<string, string> m_charpter;
        public ComicDetailInfo()
        {
            m_charpter = new SortedDictionary<string, string>();
            ShowAll = false;
            AllImage = new List<string>();
        }

        public string Href { get; set; }
        public string ComicName { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string status { get; set; }
        public string Tag { get; set; }
        public bool ShowAll { get; set; }
        public SortedDictionary<string, string> Charpter { get { return m_charpter; } }
        public List<string> AllImage { get; set; }
        public string CodeName { get; set; }
       
    }

    public class ComicParam
    {
        public string FirstCss { get; set; }
        public string ComicNameCss { get; set; }//漫画名CSS
        public string ImageUrlCss { get; set; }//图片链接CSS
        public string HrefCss { get; set; }//漫画页面CSS
        public string FindResultCss { get; set; }//
        public string MenuCss { get; set; }
        public string PageCss { get; set; }
        public List<string> TagList { get; set; }
    }

    public class CheckComicInfo
    {
        /// <summary>
        /// 漫画作者CSS 
        /// </summary>
        public string AuthorCss { get; set; }
        /// <summary>
        /// 漫画标签CSS
        /// </summary>
        public string TagCss { get; set; }
        /// <summary>
        /// 漫画简介CSS
        /// </summary>
        public string DescriptionCss { get; set; }
        /// <summary>
        /// 漫画状态CSS
        /// </summary>
        public string StatusCss { get; set; }
        /// <summary>
        /// 漫画章节CSS
        /// </summary>
        public string CharpterCss { get; set; }
        public string ExtraTagClassName { get; set; }
    }

    public interface IComicDecoder
    {
        string HotComicUrl { get;}
        string SearchUrl { get; }
        string Description { get; }
        string CodeName { get; set; }

        List<ComicDetailInfo> GetHotComic();//获取热门漫画
        SearchResult GetComicMenu();
        ComicDetailInfo GetComicInfo(string url);
        List<ComicDetailInfo> GoToPage(string url);
        List<string> GetComicImage(string url, GetOneImgHandler handler);
        SearchResult SearchComic(string keyWord);
        bool DownLoadImage(string url, string path);
        void Dispose();//资源释放
    }
}
