using ComicDownWpf.controls;
using ComicDownWpf.decoder;
using ComicDownWpf.Windows;
using ComicPlugin;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NiL.JS.Expressions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static ComicDownWpf.Windows.BookWindow;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ComicDownWpf.viewmodel
{
    public class CharpterItem : ObservableObject
    {
        public string CharpterName { get; set; }
        public string Href { get; set; }
        public string ComicName { get; set; }
        public string CodeName { get; set; }
        public string ImageUrl { get; set; }
        public string ComicInfoUrl { get; set; }
        
        private bool m_isLastRead;
        private bool m_isChecked;

        public bool IsChecked
        {
            get { return m_isChecked; }
            set { SetProperty(ref m_isChecked, value); }
        }

        public bool IsLastRead 
        {
            get => m_isLastRead;
            set { SetProperty(ref m_isLastRead, value); }
        }
        public ICommand ClickCommand { get; set; }
    }

    //漫画详情页
    internal class ComicInfoPageViewModel : ObservableObject
    {        
        private string m_href;
        private string m_comicName;
        private string m_imageUrl;
        private string m_author;
        private string m_status;
        private string m_desc;
        private string m_tag;
        private string m_codeName;
        private string m_collectText;
        private bool m_Docollect = false;
        private ImageSource m_coverImage;
        private Task loadTask;
        private BookWindow bookWindow = null;
        private ObservableCollection<CharpterItem> m_charpterList;

        public delegate void SearchDelegate(string text);
        public delegate void DownLoadDelegate(ObservableCollection<CharpterItem> list);

        public ICommand AddCollectCommand { get; set; }
        public ICommand ResumeReadCommand { get; set; }
        public ICommand SearchCommand { get; set; }
        public ICommand OpenHrefCommand { get; set; }
        public ICommand DownLoadCommand { get; set; }
        public event SearchDelegate OnSearch;
        public event DownLoadDelegate OnDownLoad;


        public ComicInfoPageViewModel()
        {
            m_charpterList = new ObservableCollection<CharpterItem>();
            ResumeReadCommand = new RelayCommand(ResumeRead);
            AddCollectCommand= new RelayCommand(AddCollect);
            SearchCommand = new RelayCommand<string>(SearchComic);
            OpenHrefCommand = new RelayCommand(OpenBrower);
            DownLoadCommand = new RelayCommand(DownLoad);
            CollectText = "收藏";
        }

        public string Href
        {
            get => m_href;
            set { SetProperty(ref m_href, value); }
        }

        public string ComicName
        {
            get => m_comicName;
            set { SetProperty(ref m_comicName, value); }
        }

        public string CodeName
        {
            get => m_codeName;
            set { SetProperty(ref m_codeName, value); }
        }

        public ImageSource ComicImage
        {
            get => m_coverImage;
            set { SetProperty(ref m_coverImage, value); }
        }

        public string ImageUrl
        {
            get => m_imageUrl;
            set { SetProperty(ref m_imageUrl, value); }
        }

        public string Author
        {
            get => m_author;
            set { SetProperty(ref m_author, value); }
        }

        public string CollectText
        {
            get => m_collectText;
            set { SetProperty(ref m_collectText, value); }
        }

        public string Description
        {
            get => m_desc;
            set { SetProperty(ref m_desc, value); }
        }

        public string Status
        {
            get => m_status;
            set { SetProperty(ref m_status, value); }
        }

        public string Tag
        {
            get => m_tag;
            set { SetProperty(ref m_tag, value); }
        }

        public ObservableCollection<CharpterItem> CharpterList
        {
            get => m_charpterList;
            set { SetProperty(ref m_charpterList, value); }
        }

        private void OpenBrower()
        {
            Process.Start(Href);
        }

        private void DownLoad()
        {
            CheckWindow checkWindow = new CheckWindow();
            checkWindow.CharpterList = CharpterList;
            checkWindow.Closing += CheckWindow_Closing;
            checkWindow.ShowDialog();
        }

        private void CheckWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CheckWindow checkWindow = sender as CheckWindow;
            if(checkWindow.ShouldDownLoad)
            {            
                OnDownLoad?.Invoke(checkWindow.SelectedChapters);
            }
        }

        private void SearchComic(string keyWord)
        {
            Console.WriteLine(keyWord);
            OnSearch?.Invoke(keyWord);
        }

        private void ResumeRead()
        {
            foreach(var i in CharpterList)
            {
                if(i.IsLastRead)
                {
                    CreateReadWindow(i.Href);
                    break;
                }
            }
        }

        private void CreateReadWindow(string chapterHref)
        {
            if (bookWindow != null)
            {
                //bookWindow.Close();
                //bookWindow = null;
            }

            bookWindow = new BookWindow();
            bookWindow.Url = chapterHref;
            bookWindow.CharpterInfoList = GetHrefList();
            bookWindow.CodeName = CodeName;
            bookWindow.ComicName = ComicName;
            bookWindow.Show();
            ComicBookShelf.AddOneReadRecord(ComicName, chapterHref, Href, ImageUrl, CodeName);
            bookWindow.Activate();
        }

        private string GetReadHistory()
        {
            var book = ComicBookShelf.GetOneHistory(ComicName, CodeName);
            if(book == null)
            {
                return null;
            }
            return book.CharpterUrl;
        }

        private void AddCollect() 
        {
            m_Docollect = !m_Docollect;

            if(m_Docollect)
            {
                if (!ComicBookShelf.CheckIfHasComic(ComicName, CodeName))
                {
                    var imgUrl = CommonManager.CacheImage(ImageUrl, CodeName);
                    ComicBookShelf.AddOneComic(new ComicBook { ComicName = this.ComicName, ComicHref = this.Href, ImgUrl = imgUrl, CodeName = this.CodeName });
                }

                CollectText = "已收藏";
            }
            else
            {
                ComicBookShelf.RemoveComic(ComicName, Href);
                CollectText = "收藏";
            }     
        }

        private void Reset()
        {
            string text = "加载中...";
            Author = text;
            Status = text;
            Description = text;
            Tag = text;

            if (ComicBookShelf.CheckIfHasComic(ComicName, CodeName))
            {
                CollectText = "已收藏";
            }
            else
            {
                CollectText = "收藏";
            }

            App.Current.Dispatcher.Invoke(new Action(() =>
            {
                CharpterList.Clear();
            }));
            
        }

        private void LoadCharpter()
        {
            Reset();
            var info = ParserManager.GetComicInfo(CodeName, Href);
            Author = info.Author;
            Status = info.status;
            Description = info.Description;
            Tag = info.Tag;

            List<string> list = new List<string>();

            foreach (var i in info.Charpter)
            {
                list.Add(i.Key);
            }

            list.Sort(delegate (string a, string b)
            {
                var num1 = Regex.Match(a, @"(?<data>\d+)").Groups["data"].Value;
                var num2 = Regex.Match(b, @"(?<data>\d+)").Groups["data"].Value;

                if (num1.Length > 0 && num2.Length > 0)
                {
                    int input_a = Convert.ToInt32(num1);
                    int input_b = Convert.ToInt32(num2);

                    if (input_a >= input_b)
                    {
                        return 1;
                    }

                    return -1;
                }

                return a.Length >= b.Length ? 1 : -1;
            });

            string url = GetReadHistory();

            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                CharpterList.Clear();

                foreach (var i in list)
                {
                    var item = new CharpterItem() { CharpterName = i, ImageUrl = this.ImageUrl, ComicInfoUrl = this.Href, Href = info.Charpter[i], CodeName = this.CodeName, ComicName = this.ComicName };
                    if(item.Href == url)
                    {
                        item.IsLastRead= true;
                    }

                    item.ClickCommand = new RelayCommand<controls.Charpter>(ItemClick);
                    CharpterList.Add(item);
                }
            }));
 
        }

        private List<BookWindow.CharpterInfo> GetHrefList()
        {
            var list = new List<BookWindow.CharpterInfo>();
            foreach (var item in CharpterList)
            {
                CharpterInfo info = new CharpterInfo();
                info.m_comicUrl = item.Href;
                info.m_charpterName = item.CharpterName;
                list.Add(info);
            }
            return list;
        }

        private void ItemClick(controls.Charpter charpter)
        {
            CreateReadWindow(charpter.Href);
        }

        public void LoadComicInfo()
        {
            loadTask = new Task(LoadCharpter);
            loadTask.Start();
        }
    }

}
