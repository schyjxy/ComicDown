using ComicDownWpf.decoder;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NiL.JS.Expressions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.Text.RegularExpressions;
using ComicDownWpf.Windows;

namespace ComicDownWpf.viewmodel
{
    public class DownTaskRecord : ObservableObject
    {
        private string m_comicName;
        private int m_currentProgress;
        private int m_totalNum;
        private bool m_running;
        private bool m_downCompleted;
        private bool m_isChecked;
        private Task m_downLoadTask = null;

        public delegate void DeleteTaskDelegate(DownTaskRecord record);
        public event DeleteTaskDelegate DeleteComicEvent;

        public DownTaskRecord()
        {
            ReDownLoadCommand = new RelayCommand(ReDownLoad);
            OpenDirectoryCommand = new RelayCommand(OpenDirectory);
            DeleteTaskCommand = new RelayCommand(DeleteComic);
            ItemClickCommand = new RelayCommand(ItemClick);
            OpenComicCommand = new RelayCommand(OpenComic);
            PageNum = 100;
        }

        public string ComicName
        {
            get { return m_comicName; }
            set
            {
                SetProperty(ref m_comicName, value);
            }
        }

        public string ComicInfoUrl { get; set; }

        public string ChapterName
        {
            get; set;
        }

        public string ImageUrl
        {
            get; set;
        }

        public int PageNum
        {
            get { return m_totalNum; }
            set
            {
                SetProperty(ref m_totalNum, value);
            }
        }

        public string CodeName { get; set; }
        public string Href { get; set; }

        public bool IsChecked
        {
            get => m_isChecked;
            set { SetProperty(ref m_isChecked, value); }
        }

        public bool DownCompleted
        {
            get => m_downCompleted;
            set { SetProperty(ref m_downCompleted, value); }
        }

        public int CurrentProgress
        {
            get { return m_currentProgress; }
            set
            {
                SetProperty(ref m_currentProgress, value);
            }
        }

        public string SavePath { get; set; }
        public ICommand OpenComicCommand { get; set; }
        public ICommand ReDownLoadCommand { get; set; }
        public ICommand DeleteTaskCommand { get; set; }
        public ICommand OpenFileCommand { get; set; }
        public ICommand OpenDirectoryCommand { get; set; }
        public ICommand ItemClickCommand { get; set; }

        private void ReDownLoad()
        {
            CurrentProgress = 0;
            Start();
        }

        DateTime time = DateTime.Now;

        private void ItemClick()
        {
            DateTime now = DateTime.Now;
            var cost = now.Subtract(time).TotalMilliseconds;

            if (cost < 500)
            {
                IsChecked = !IsChecked;
            }
            time = now;
        }

        public void Stop()
        {
            m_running = false;
            if (m_downLoadTask?.Status == TaskStatus.Running)
            {
                m_downLoadTask.Wait();
            }

            if (Directory.Exists(SavePath))
            {
                Directory.Delete(SavePath, true);
            }
            ComicBookShelf.RemoveDownLoadRecord(ComicName, Href);
        }

        public void DeleteComic()
        {           
            DeleteComicEvent?.Invoke(this);
        }

        private void OpenComic()
        {
            BookWindow bookWindow = new BookWindow();
            bookWindow.ComicName = ComicName + ChapterName;
            bookWindow.CodeName = CodeName;
            //bookWindow.CharpterInfoList = GetHrefList();
            bookWindow.Url = Href;
            bookWindow.LoadLocal(SavePath, bookWindow.ComicName, Href);
            bookWindow.Show();     
        }

        private void OpenDirectory()
        {
            System.Diagnostics.Process.Start("explorer.exe", SavePath);
        }

        private bool IsValidPath(string text)
        {
            var arry = System.IO.Path.GetInvalidPathChars();
            int index = text.IndexOfAny(arry);
            if (index >= 0)
            {
                return false;
            }

            char[] data = { '？', '，', ',', '#', '！', '!', ' ', ':', '：' };
            if (text.IndexOfAny(data) >= 0)
            {
                return false;
            }
            return true;
        }

        private string DeleteIlegalFileName(string name)
        {
            string ret = "";
            if (!IsValidPath(name))
            {
                MatchCollection collection = Regex.Matches(name, @"\w+");
                foreach (var i in collection)
                {
                    ret = ret + i.ToString() + "_";
                }
            }
            else
            {
                ret = name;
            }

            string regSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            Regex rg = new Regex(string.Format("[{0}]", Regex.Escape(regSearch)));
            ret = rg.Replace(ret, "");
            return ret;
        }

        private void DownLoad()
        {
            var urlList = ParserManager.GetImageList(CodeName, Href, null);
            PageNum = urlList.Count;
            DownCompleted = false;
            SavePath = CommonManager.g_download_path + DeleteIlegalFileName(ComicName) + "\\" + DeleteIlegalFileName(ChapterName);
            ComicBookShelf.AddDownLoadRecord(ComicName, ChapterName, ComicInfoUrl, ImageUrl, Href, CodeName, "0", PageNum.ToString(), SavePath);

            if (!Directory.Exists(SavePath))
            {
                Directory.CreateDirectory(SavePath);
            }

            int count = 0;
            int failCount = 0;

            for (int i = CurrentProgress; i < urlList.Count; i++)
            {
                if (!m_running)
                {
                    return;
                }
                var fullPath = string.Format(@"{0}\{1}.jpg", SavePath, count++);

                while (!ParserManager.DownLoadImage(CodeName, fullPath, urlList[i]))
                {
                    if (!m_running)
                    {
                        return;
                    }

                    failCount++;
                    System.Threading.Thread.Sleep(500);

                    if (failCount > 5)
                    {
                        Console.WriteLine("下载失败:{0}", ComicName);
                        break;
                    }
                }

                ComicBookShelf.UpdateDownLoadRecord(ComicName, CodeName, Href, CurrentProgress.ToString());
                CurrentProgress++;
                failCount = 0;
            }

            if (CurrentProgress >= PageNum - 1)
            {
                ComicBookShelf.UpdateDownLoadRecord(ComicName, CodeName, Href, CurrentProgress.ToString());
                DownCompleted = true;
            }
        }

        public void Start()
        {         
            if(m_downLoadTask == null || m_downLoadTask.Status == TaskStatus.RanToCompletion)
            {
                m_running = true;
                m_downLoadTask = new Task(DownLoad);
                m_downLoadTask.Start();
            }
           
        }
    }

    internal class DownChapterPageViewModel : ObservableObject
    {
        private ObservableCollection<DownTaskRecord> m_downList;
        public ICommand FullCheckCommand { get; set; }
        public ICommand FullUnCheckCommand { get; set; }
        public ICommand DeletePatchCommand { get; set; }
        public ICommand CoverClickCommand { get; set; }
        public event NavigateEventDelegate NavigateDetailEvent;

        public DownChapterPageViewModel()
        {
            m_downList = new ObservableCollection<DownTaskRecord>();
            FullCheckCommand = new RelayCommand(FullCheck);
            FullUnCheckCommand = new RelayCommand(FullUnCheck);
            DeletePatchCommand = new RelayCommand(DeleteBatch);
            CoverClickCommand = new RelayCommand(CoverClick);
        }

        private ImageSource m_imageSource;
        private string m_comicName;

        public ImageSource CoverImage
        {
            get => m_imageSource;
            set { SetProperty(ref m_imageSource, value); }
        }

        public string ImageUrl { get; set; }
        public string CodeName { get; set; }
        public string Href { get; set; }
        public string ComicInfoUrl { get; set; }

        public string ComicName 
        { 
            get => m_comicName;
            set { SetProperty(ref m_comicName, value); }
        }

        public ObservableCollection<DownTaskRecord> DownList
        {
            get => m_downList;
            set { SetProperty(ref m_downList, value);}
        }

        private void FullCheck()
        {
            if (DownList != null)
            {
                foreach (var i in DownList)
                    i.IsChecked = true;
            }
        }

        private void CoverClick()
        {
            NavigateDetailEvent?.Invoke(ComicName, ComicInfoUrl, CodeName, CoverImage, ImageUrl);
        }

        private void DeleteBatch()
        {          
            Task.Run(new Action(() =>
            {
                var removeList = new List<DownTaskRecord>();

                for (int i = 0; i < DownList.Count; i++)
                {
                    if (DownList[i].IsChecked)
                    {
                        removeList.Add(DownList[i]);
                    }
                }

                foreach (var i in removeList)
                {
                    i.DeleteComic();
                }
            }));


        }

        private void FullUnCheck()
        {
            if (DownList != null)
            {
                foreach (var i in DownList)
                    i.IsChecked = false;
            }
        }

        public void LoadDownListory(DownTaskTree tree)
        {
             ComicName = tree.ComicName;
             ImageUrl = tree.ImageUrl;
             Href = tree.Href;
             CoverImage = new BitmapImage(new Uri(tree.ImageUrl));
             DownList = tree.NodeList;
             ComicInfoUrl = tree.ComicInfoUrl;
             CodeName = tree.CodeName;
        }
    }
}
