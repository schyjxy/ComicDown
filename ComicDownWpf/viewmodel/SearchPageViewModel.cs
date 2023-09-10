using AngleSharp.Common;
using ComicDownWpf.decoder;
using ComicDownWpf.pages;
using ComicPlugin;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.DependencyInjection;
using NiL.JS.Expressions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Web;
using DMSkin.Core.Common;
using System.Windows.Media.Imaging;
using ComicDownWpf.controls;
using System.Windows;
using System.Threading;
using System.Windows.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using AngleSharp.Text;

namespace ComicDownWpf.viewmodel
{
    class ParseInfo
    {
        public bool IsChecked { get; set; }
        public string CodecName { get; set; }
        public string Description { get; set; }
        public IComicDecoder ComicDecoder { get; set; }
    }

    internal class SearchPageViewModel : ObservableObject
    {
        public ICommand SearchCommand { get; set; }
        public ICommand LoadParseCommand { get; set; }

        private string m_keyword;
        private string m_result;
        private int m_comboxIndex;

        public string KeyWord
        {
            get => m_keyword;
            set { SetProperty(ref m_keyword, value); }
        }

        ObservableCollection<ComicInfoItem> m_comicList;
        ObservableCollection<ParseInfo> m_parserList;

        public event NavigateEventDelegate NavigateDetailEvent;

        public SearchPageViewModel()
        {
            SearchCommand = new RelayCommand(SearchComic);
            LoadParseCommand = new RelayCommand(GetParser);
            ComicList = new ObservableCollection<ComicInfoItem>();
            ParseList = new ObservableCollection<ParseInfo>();
            KeyWord = "夫妻";
        }

        public ObservableCollection<ComicInfoItem> ComicList
        {
            get => m_comicList;
            set { SetProperty(ref m_comicList, value); }
        }

        public ObservableCollection<ParseInfo> ParseList
        {
            get => m_parserList;
            set { SetProperty(ref m_parserList, value); }
        }

        public string Result
        {
            get => m_result;
            set { SetProperty(ref m_result, value); }
        }

        public int ComboBoxIndex
        {
            get => m_comboxIndex;
            set { SetProperty(ref m_comboxIndex, value); }
        }

        private void GetParser()
        {
            //MangaBzParser 搜索功能没做
            string parserString = "CopyMangaParser,XManHuaParser, MangaBzParser,ShenShiManHuaParser,JinMianTianTangParser,PaoPaoHanManParser, KuKuDongManParser, JiuJiuManHuaParser, JiuLingManHua,ManhuaDBParser";
            var list = ParserManager.GetParserList();
            ParseList.Clear();
            ComboBoxIndex = 0;

            foreach (var item in list)
            {
                ParseInfo info = new ParseInfo { CodecName = item.CodeName, ComicDecoder = item, Description = item.Description };

                if (parserString.Contains(info.CodecName))
                {
                    info.IsChecked= true;
                }
                ParseList.Add(info);
            }
        }

        public void DoSearch()
        {
            ComicList?.Clear();
            Result = "";

            if (ParseList.Count == 0)
            {
                GetParser();
            }

            Task.Run(new Action(() =>
            {
                for (int i = 0; i < ParseList.Count; i++)
                {
                    if (ParseList[i].IsChecked)
                    {
                        string key = HttpUtility.UrlEncode(KeyWord);
                        if (ParseList[i].CodecName == "KuKuDongManParser")
                        {
                            key = HttpUtility.UrlEncode(Encoding.GetEncoding("gbk").GetBytes(KeyWord));
                        }

                        SearchResult result = ParseList[i].ComicDecoder.SearchComic(key);
                        if (result.IsSuccess)
                        {
                            foreach (var r in result.DetailInfoList)
                            {
                                Application.Current.Dispatcher.Invoke((Action)(() =>
                                {
                                    var item = new ComicInfoItem();
                                    item.ComicName = r.ComicName;
                                    item.Href = r.Href;
                                    item.ClickCommand = new RelayCommand<CoverItem>(ItemClick);
                                    item.CodeName = r.CodeName;
                                    item.ImageUrl = r.ImageUrl;
                                    ComicList.Add(item);
                                }));

                            }
                        }

                    }
                }

                Result = "搜索 '" + KeyWord + "', 找到 " + ComicList.Count + " 本漫画";
            }));

           
        }

        private void SearchComic()
        {       
            DoSearch();
        }

        private void ItemClick(CoverItem item)
        {
            if (NavigateDetailEvent != null)
            {
                NavigateDetailEvent(item.MyComicName, item.Href, item.CodeName, item.MyImage, item.ImageUrl);
            }

        }

    }
}
