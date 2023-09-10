using ComicDownWpf.decoder;
using ComicDownWpf.viewmodel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ComicDownWpf.controls
{
    /// <summary>
    /// FluentImageViewer.xaml 的交互逻辑
    /// </summary>
    /// 
    public partial class FluentImageViewer : UserControl
    {
        private int panelIndex = 0;
        private int curPageIndex = 0;
        private int m_startPage = 0;
        private int groupSize = 20;
        private string m_cachePath;
        private string m_chapterUrl;
        
        private List<string> m_urlList;

        private int m_pageNum;
        public string ComicName { get; set; }
        public string CodeName { get; set; }

       
        public delegate void ChangePageDelegate(PageEventArgs args);
        public delegate void ChangeCharpterDelegate(PageEventTypeEnum args);
        public event ChangeCharpterDelegate changeCharpter;
        public event ChangePageDelegate changePageEvent;


        public FluentImageViewer()
        {
            InitializeComponent();  
            m_urlList = new List<string>();
            //DataContext = this;
        }

        private void GetAImg(string url)
        {         
            Dispatcher.Invoke(new Action(() =>
            {
                m_urlList.Add(url);
                if (m_urlList.Count < groupSize)
                {
                    AppendItem(url);
                }
            }));
        }

        private void LazyLoadPicture()
        {
            int step = groupSize;
            int startIndex = panelIndex;
            int endIndex = startIndex + step < m_urlList.Count ? startIndex + step : m_urlList.Count;

            for (int i = startIndex; i < endIndex; i++)
            {
                AppendItem(m_urlList[i]);
            }    
        }

        public void AppendItem(string url)
        {
            var filePath = string.Format(@"{0}\{1}.jpg", m_cachePath, panelIndex); 
            ImagePanel panel = new ImagePanel() {Index = panelIndex++ , ImageUrl=url, CodeName = this.CodeName, CachePath = filePath};
            smoothWrapnnel.Children.Add(panel);
        }

        public void ChangeCharpter(string title, string codeName, string url)
        {
            Close();
            StartShow(title, url);
        }

        private bool IsValidPath(string text)
        {
            var arry = System.IO.Path.GetInvalidPathChars();
            int index = text.IndexOfAny(arry);
            if (index >= 0)
            {
                return false;
            }

            char[] data = {'？', '，', ',' ,'#','！','!', ' ', ':', '：', '[', ']'};
            if (text.IndexOfAny(data) >= 0)
            {
                return false;
            }
            return true;
        }

        private string GetComicPath(string url)
        {
            int charpterIndex;
            string charpter;

            Uri uri = new Uri(url);
            string domain = uri.Host;
            charpterIndex = url.IndexOf(domain) + domain.Length;
            var temp = url.Substring(charpterIndex);

            if (temp.Contains("."))
            {             
                charpter = temp.Replace(".", "_").Replace("/", "_");
            }
            else
            {
                charpter = temp.Replace("/", "_");
            }
         
            return charpter;
        }

        public void LoadLocal(string save_path, string title, string chapterUrl)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(save_path);
            FileInfo[] fileInfo = dirInfo.GetFiles();
            Array.Sort(fileInfo, new FileNameSort());

            foreach (var i in fileInfo)
            {
                m_urlList.Add(i.FullName);
            }

            m_chapterUrl = chapterUrl;
            panelIndex = 0;
            curPageIndex = 0;
            m_pageNum = fileInfo.Length;
            m_cachePath = save_path;
            changePageEvent(new PageEventArgs() { Index = curPageIndex, PageCount = m_pageNum });
            LazyLoadPicture();
        }

        public void StartShow(string title,  string url)
        {
            m_chapterUrl = url;
            panelIndex = 0;
            curPageIndex = 0;
            m_pageNum = -1;

            var book = ComicBookShelf.GetOneHistory(ComicName, CodeName);
            if (book?.CharpterUrl == url)
            {
                //panelIndex = book.LastReadPage;
                m_startPage = book.LastReadPage;
            }

            if (!IsValidPath(title))
            {
                string charpter = GetComicPath(url);      
                m_cachePath = CommonManager.g_temp_path  + @"temp\"  + CodeName + @"\" + charpter;
            }
            else
            {
                m_cachePath = CommonManager.g_temp_path + @"temp\"  + CodeName + @"\" + title;
            }        

            if (!Directory.Exists(m_cachePath))
            {
                Directory.CreateDirectory(m_cachePath);               
            }

            Task.Run(() =>
            {
                var start = DateTime.Now;
                var list = ParserManager.GetImageList(CodeName, url, GetAImg);
                m_pageNum = list.Count;
                Console.WriteLine("==========解析器:{0}, 查询耗时:{1},缓存路径:{2}", CodeName, DateTime.Now.Subtract(start).TotalMilliseconds, m_cachePath);
                changePageEvent(new PageEventArgs() { Index = curPageIndex, PageCount = m_pageNum});
            });
        }

        private void smoothWrapnnel_PageChanged(object sender, PageEventArgs e)
        {
            curPageIndex = e.Index;
            e.PageCount = m_pageNum == -1? e.Index : m_pageNum;
            changePageEvent?.Invoke(e);

            if (e.PageEventType == PageEventTypeEnum.FinialPage)
            {
                if (m_pageNum!= -1 && curPageIndex >= m_pageNum - 1)
                {                   
                    changeCharpter(PageEventTypeEnum.NextCharpter);
                }
                else
                {
                    if (curPageIndex > groupSize/2)
                    {
                        LazyLoadPicture();
                    }

                }
              
            }
        }

        public void Close()
        {
            ComicBookShelf.UpdateReadRecord(ComicName, CodeName, m_chapterUrl, curPageIndex);

            foreach (ImagePanel i in smoothWrapnnel.Children)
            {
                i.Close();
            }

            m_urlList.Clear();
            panelIndex = 0;
            curPageIndex = 0;
            smoothWrapnnel.Children.Clear();
            scrollViwer.ScrollToVerticalOffset(0);
        }

        private void smoothWrapnnel_PageChanged(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
