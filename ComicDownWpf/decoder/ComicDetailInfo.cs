using System.Collections.Generic;

namespace ComicDownWpf.decoder
{
    public class SearchResult
    {
        public enum SearchType
        {
            Menu,
            KeyWord
        }

        public SearchType Type { get; set; }
        public int Count { get; set; }
        public List<ComicDetailInfo> DetailInfoList { get; set; }
        public Dictionary<string, string> MenuDict { get; set; }
        public Dictionary<string, string> PageDict { get; set; }
        public string CodeName { get; set; }
    }

    public class ComicDetailInfo
    {
        Dictionary<string, string> m_charpter;
        public ComicDetailInfo()
        {
            m_charpter = new Dictionary<string, string>();
            ShowAll = false;
        }

        public string Href { get; set; }
        public string ComicName { get; set; }
        public string ImgaeUrl { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string status { get; set; }
        public string Tag { get; set; }
        public bool ShowAll { get; set; }
        public Dictionary<string, string> Charpter { get { return m_charpter; } }
        public List<string> AllImage { get; set; }
        public string CodeName { get; set; }
    }
}
