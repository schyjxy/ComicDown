using ComicDownWpf.decoder;
using ComicDownWpf.pages;
using System.Threading.Tasks;
using System;
using ComicDownWpf.controls;
using ComicPlugin;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using NiL.JS.Expressions;

namespace ComicDownWpf.viewmodel
{
    class ParserInfo : ObservableObject
    {
        private bool m_isChecked;
        public string Description { get; set; }
        public IComicDecoder Decoder { get; set; }
        public ICommand ClickItemCmd { get; set; }

        public bool IsChecked      
        { 
            get => m_isChecked;
            set { SetProperty(ref m_isChecked, value); }
        }
    }

    class MainWindowViewModel : ViewBase
    {
        private PageBase m_currentPage = CommonManager.mainPage;
        private ComicItemPageViewModel comicItemPageView;
        private HistoryPageViewModel historyPageViewModel;
        private SearchPageViewModel searchPageViewModel;
        private ComicInfoPageViewModel comicPageViewModel;//漫画详情页
        private DownLoadPageViewModel downLoadPageViewModel;
        private DownChapterPageViewModel downChapterPageViewModel;
        private ObservableCollection<ParserInfo> m_parserList = null;

        public enum Menu
        { 
            BookShelf,
            Search,
            History,
            DownLoad,
            ComicDetailPage,
            DownLoadChapter
        }

        public MainWindowViewModel()
        {
            CommonManager.Init();          
            m_parserList = new ObservableCollection<ParserInfo>();

            comicItemPageView = new ComicItemPageViewModel();
            comicItemPageView.NavigateDetailEvent += ComicItemPageView_NavigateDetailEvent;
            CommonManager.mainPage.DataContext = comicItemPageView;

            historyPageViewModel = new HistoryPageViewModel();
            historyPageViewModel.NavigateDetailEvent += ComicItemPageView_NavigateDetailEvent;
            CommonManager.historyPage.DataContext = historyPageViewModel;

            searchPageViewModel = new SearchPageViewModel();
            searchPageViewModel.NavigateDetailEvent += ComicItemPageView_NavigateDetailEvent;
            CommonManager.searchPage.DataContext = searchPageViewModel;

            comicPageViewModel = new ComicInfoPageViewModel();
            comicPageViewModel.OnSearch += ComicPageViewModel_OnSearch;
            comicPageViewModel.OnDownLoad += ComicPageViewModel_OnDownLoad;
            CommonManager.comicInfoPage.DataContext = comicPageViewModel;

            downLoadPageViewModel = new DownLoadPageViewModel();
            downLoadPageViewModel.CoverClickEvent += DownLoadPageViewModel_CoverClickEvent;
            CommonManager.downLoadPage.DataContext = downLoadPageViewModel;
            CommonManager.comicCollectPage.NavigateDetailEvent += ComicItemPageView_NavigateDetailEvent;

            downChapterPageViewModel = new DownChapterPageViewModel();
            downChapterPageViewModel.NavigateDetailEvent += ComicItemPageView_NavigateDetailEvent;
            CommonManager.downCharpterPage.DataContext = downChapterPageViewModel;

            LoadParser();
        }

        private void DownLoadPageViewModel_CoverClickEvent(DownTaskTree tree)
        {
            CurrentPage = CommonManager.downCharpterPage;
            downChapterPageViewModel.LoadDownListory(tree);
        }

        private void ComicPageViewModel_OnDownLoad(ObservableCollection<CharpterItem> list)
        {
            downLoadPageViewModel.AddDownLoadTask(list);
        }

        //跳到漫画搜索
        private void ComicPageViewModel_OnSearch(string text)
        {
            Console.WriteLine(text);
            CurrentPage = CommonManager.searchPage;
            searchPageViewModel.KeyWord = text;
            searchPageViewModel.DoSearch();
        }

        // 触发点击事件
        private void ClickItem(ImageRadioButton info)
        {
            LoadComicByDecoder(info.Decoder);
        }

        public ObservableCollection<ParserInfo> ParserList
        {
            get { return m_parserList; }
            set 
            { 
                m_parserList = value; 
                OnPropertyChanged("ParserList");
            }
        }

        private void LoadComicByDecoder(IComicDecoder decoder)
        {
            CurrentPage = CommonManager.mainPage;
            comicItemPageView.LoadComic(decoder);
        }

        private void ComicItemPageView_NavigateDetailEvent(string comicName, string href, string codeName, System.Windows.Media.ImageSource image, string imgurl)
        {
            comicPageViewModel.ImageUrl = imgurl;
            comicPageViewModel.CodeName = codeName;
            comicPageViewModel.Href = href;
            comicPageViewModel.ComicName = comicName;
            comicPageViewModel.ComicImage = image;

            CurrentPage = CommonManager.comicInfoPage;
            comicPageViewModel.LoadComicInfo();
        }

        private void LoadParser()
        {
            var list = ParserManager.GetParserList();
            m_parserList = new ObservableCollection<ParserInfo>();
            ParserList.Clear();
            IComicDecoder decoder = null;

            foreach (var i in list)
            {
                ParserInfo info = new ParserInfo() { Description = i.Description, Decoder = i, ClickItemCmd = new RelayCommand<ImageRadioButton>(ClickItem) };               

                if (i.CodeName == "CopyMangaParser")
                {
                    decoder = i;
                    info.IsChecked= true;
                }

                ParserList.Add(info);
            }

            LoadComicByDecoder(decoder);
        }

        public PageBase CurrentPage
        {
            get { return m_currentPage; }
            set
            {
                m_currentPage = value;
                OnPropertyChanged("CurrentPage");
                
            }
        }

        private Menu m_clickMenu;

        public Menu ClickMenu
        {
            get { return m_clickMenu; }
            set
            {
                m_clickMenu = value;

                switch (m_clickMenu)
                {
                    case Menu.Search:
                        CurrentPage = CommonManager.searchPage;
                        break;
                    case Menu.History:
                        CurrentPage = CommonManager.historyPage;
                        break;
                    case Menu.DownLoad:
                        CurrentPage = CommonManager.downLoadPage;
                        break;
                    case Menu.BookShelf:
                        CurrentPage = CommonManager.comicCollectPage;
                        break;
                    case Menu.ComicDetailPage:
                        CurrentPage = CommonManager.comicInfoPage;
                        break;
                }

                OnPropertyChanged("ClickMenu");

            }
        }
    }
}
