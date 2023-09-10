using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ComicDownWpf.Utils;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace ComicDownWpf.viewmodel
{
    class ComicBookList
    {
        public List<ComicBook> List { get; set; }
    }

    class ComicBook : IComparable<ComicBook>
    {
        public string ComicName { get; set; }
        public string ComicHref { get; set; }
        public string ImgUrl { get; set; }
        public string CodeName { get; set; }
        public string CharpterUrl { get; set; }
        public UInt64 LastReadTime { get; set; }//最后阅读时间
        public int LastReadPage { get; set; }//最后阅读页码

        public int CompareTo(ComicBook other)
        {
           if(this.LastReadTime > other.LastReadTime)
           {
                return -1;
           }
           return 1;
        }
    }

    class ComicBookShelf
    {
        public static SqliteHelper helper;
        public static string collectTable = "MyColletion";
        public static string historyTable = "MyHistory";
        public static string downLoadTable = "MyDownLoad";
        public static string comicName = "comicName";
        public static string charpterName = "charpterName";
        public static string urlName = "url";
        public static string imgUrlName = "imgUrl";
        public static string codeName = "codeName";
        public static string charpterUrlName = "charpterUrl";//漫画信息详情url
        public static string savePathName = "path";
        public static string curDownPageName = "curDownPage";
        public static string totalPageName = "totalPageNum";
        public static string lastReadTime = "lastReadTime";
        public static string lastReadPage = "lastReadPage";

        static void CreateTable(string name)
        {
            List<ColumnInfo> list = new List<ColumnInfo>() { };
            list.Add(new ColumnInfo { ColumnName = comicName, Type = "vchar(1000)" });
            list.Add(new ColumnInfo { ColumnName = urlName, Type = "vchar(1000)" });
            list.Add(new ColumnInfo { ColumnName = imgUrlName, Type = "vchar(1000)" });
            list.Add(new ColumnInfo { ColumnName = codeName, Type = "vchar(1000)" });
            helper.CreateTable(name, list);
        }

        static void CreateHistoryTable(string name)
        {
            List<ColumnInfo> list = new List<ColumnInfo>() { };
            list.Add(new ColumnInfo { ColumnName = comicName, Type = "vchar(1000)" });
            list.Add(new ColumnInfo { ColumnName = urlName, Type = "vchar(1000)" });
            list.Add(new ColumnInfo { ColumnName = imgUrlName, Type = "vchar(1000)" });
            list.Add(new ColumnInfo { ColumnName = codeName, Type = "vchar(1000)" });
            list.Add(new ColumnInfo { ColumnName = charpterUrlName, Type = "vchar(1000)" });
            list.Add(new ColumnInfo { ColumnName = lastReadTime, Type = "vchar(1000)" });
            list.Add(new ColumnInfo { ColumnName = lastReadPage, Type = "vchar(1000)" });
            helper.CreateTable(name, list);
        }

        static void CreateDownLoadTable(string name)
        {
            List<ColumnInfo> list = new List<ColumnInfo>() { };
            list.Add(new ColumnInfo { ColumnName = comicName, Type = "vchar(1000)" });
            list.Add(new ColumnInfo { ColumnName = charpterName, Type = "vchar(1000)" });
            list.Add(new ColumnInfo { ColumnName = charpterUrlName, Type = "vchar(1000)" });
            list.Add(new ColumnInfo { ColumnName = urlName, Type = "vchar(1000)" });
            list.Add(new ColumnInfo { ColumnName = imgUrlName, Type = "vchar(1000)" });        
            list.Add(new ColumnInfo { ColumnName = codeName, Type = "vchar(1000)" });
            list.Add(new ColumnInfo { ColumnName = curDownPageName, Type = "vchar(100)" });
            list.Add(new ColumnInfo { ColumnName = totalPageName, Type = "vchar(100)" });
            list.Add(new ColumnInfo { ColumnName = savePathName, Type = "vchar(1000)" });
            helper.CreateTable(name, list);
        }

        static ComicBookShelf()
        {
            helper = new SqliteHelper("comic.db");
            if(!helper.IsHasTable(collectTable))
            {
                CreateTable(collectTable);
            } 

            if(!helper.IsHasTable(historyTable))
            {
                CreateHistoryTable(historyTable);
            }

            if(!helper.IsHasTable(downLoadTable))
            {
                CreateDownLoadTable(downLoadTable);
            }
        }

        ~ComicBookShelf()
        {
            helper.Close();
        }

        public static List<ComicBook> GetAllHistory()
        {
            ComicBookList bookList = helper.GetHistory<ComicBookList>(historyTable);
            return bookList.List;
        }

        public static ObservableCollection<DownTaskRecord> GetAllDownRecord()
        {
            return helper.GetDownAllRecord();
        }

        public static ComicBook GetOneHistory(string comicName, string codeName)
        {
            if(!CheckIfHasHistory(comicName, codeName))
            {
                return null;
            }
            return helper.GetOneHistory(historyTable, comicName, codeName);
        }

        public static List<ComicBook> GetAllBook()
        {
            ComicBookList bookList = helper.GetAllBook<ComicBookList>(collectTable);
            return bookList.List;
        }

        public static void AddComicList(List<ComicBook> list)
        {
            foreach(var i in list)
            {
                AddOneComic(i);
            }
        }

        /// <summary>
        /// 更新阅读记录
        /// </summary>
        /// <param name="comicName">更新的漫画名</param>
        /// <param name="codeName">更新的Decoder 名</param>
        /// <param name="charpterUrl">章节url</param>
        /// <param name="lastReadPage">最后一次阅读的页数</param>

        public static void UpdateReadRecord(string comicName, string codeName, string charpterUrl, int lastReadPage)
        {
            var setDict = new Dictionary<string, string>();
            setDict.Add(ComicBookShelf.comicName, comicName);//设置列表
            setDict.Add(ComicBookShelf.charpterUrlName, charpterUrl);
            setDict.Add(ComicBookShelf.lastReadPage, lastReadPage.ToString());

            Int64 utcTime = 0;
            DateTime time = DateTime.Now;
            var startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            utcTime = (Int64)(time - startTime).TotalSeconds;
            setDict.Add(ComicBookShelf.lastReadTime, utcTime.ToString());

            var findDict = new Dictionary<string, string>();//查找列表
            findDict.Add(ComicBookShelf.comicName, comicName);
            findDict.Add(ComicBookShelf.codeName, codeName);
            helper.UpdateData(historyTable, setDict, findDict);
        }

        public static void AddOneReadRecord(string comicName, string chapterUrl, string href, string imgUrl, string codeName)
        {
            if(imgUrl.Length > 0)
            {            
                var dict = new Dictionary<string, string>();
                dict.Add(ComicBookShelf.comicName, comicName);
                dict.Add(ComicBookShelf.urlName, href);
                dict.Add(ComicBookShelf.imgUrlName, imgUrl);
                dict.Add(ComicBookShelf.codeName, codeName);
                dict.Add(ComicBookShelf.charpterUrlName, chapterUrl);
                dict.Add(ComicBookShelf.lastReadPage, "0");//默认第一页

                Int64 utcTime = 0;
                DateTime time = DateTime.Now;
                System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
                utcTime = (Int64)(time - startTime).TotalSeconds;
                dict.Add(ComicBookShelf.lastReadTime, utcTime.ToString());

                if (!CheckIfHasHistory(comicName, codeName))
                {
                    helper.InsertData(historyTable, dict);
                }
                else
                {
                   // UpdateReadRecord();
                }
                          
            }
           
        }

        public static void AddDownLoadRecord(string comicName, string chapterName, string charpterInfoUrl, string imgUrl, string href,  string codeName, string curDownPage, string totalNum, string savePath)
        {
            var dict = new Dictionary<string, string>();
            dict.Add(ComicBookShelf.comicName, comicName);
            dict.Add(ComicBookShelf.urlName, href);
            dict.Add(ComicBookShelf.charpterUrlName, charpterInfoUrl);
            dict.Add(ComicBookShelf.codeName, codeName);
            dict.Add(ComicBookShelf.charpterName, chapterName);
            dict.Add(ComicBookShelf.imgUrlName, imgUrl);
            dict.Add(ComicBookShelf.curDownPageName, curDownPage);
            dict.Add(ComicBookShelf.totalPageName, totalNum);
            dict.Add(ComicBookShelf.savePathName, savePath);

            if(!CheckIfHasDownRecord(comicName, codeName, chapterName))
            {
                helper.InsertData(downLoadTable, dict);
            }           
        }

        public static void UpdateDownLoadRecord(string comicName, string codeName, string href, string curDownPage)
        {
            var setDict = new Dictionary<string, string>();
            setDict.Add(ComicBookShelf.curDownPageName, curDownPage);//设置列表

            var findDict = new Dictionary<string, string>();//查找列表
            findDict.Add(ComicBookShelf.comicName, comicName);//设置列表
            findDict.Add(ComicBookShelf.urlName, href);
            findDict.Add(ComicBookShelf.codeName, codeName);

            helper.UpdateData(downLoadTable, setDict, findDict);
        }

        public static void AddOneComic(ComicBook book)
        {
            var dict = new Dictionary<string, string>();
            dict.Add(comicName, book.ComicName);
            dict.Add(urlName, book.ComicHref);
            dict.Add(imgUrlName, book.ImgUrl);
            dict.Add(codeName, book.CodeName);
            helper.InsertData(collectTable, dict);
        }

        public static void RemoveComic(string name, string href)
        {
            helper.DeleteOneRow(collectTable, comicName, name, urlName, href);
        }

        public static void RemoveHisoty(string name, string href)
        {
           helper.DeleteOneRow(historyTable, comicName, name, urlName, href);
        }

        public static void RemoveDownLoadRecord(string name, string href)
        {
            helper.DeleteOneRow(downLoadTable, comicName, name, urlName, href);
        }

        public static bool CheckIfHasHistory(string name, string codeName)
        {
            return helper.IfHasRecord(historyTable, comicName, name);
        }

        public static bool CheckIfHasComic(string name, string codeName)
        {
            return helper.IfHasRecord(collectTable, comicName, name);
        }

        public static bool CheckIfHasDownRecord(string name, string codeName, string chapterName)
        {
            var dict = new Dictionary<string, string>();
            dict.Add(ComicBookShelf.comicName, name);
            dict.Add(ComicBookShelf.codeName, codeName);
            dict.Add(ComicBookShelf.charpterName, chapterName);
            return helper.IfHasRecord(downLoadTable, dict);
        }
    }
}
