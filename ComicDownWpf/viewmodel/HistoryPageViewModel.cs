using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using ComicDownWpf.controls;
using ComicDownWpf.pages;
using System.Windows.Controls.Primitives;
using NiL.JS.Statements;
using NiL.JS.Expressions;

namespace ComicDownWpf.viewmodel
{
    class ComicInfoItem:ObservableObject
    {

        private bool m_selected = false;
        public string ComicName { get; set; }
        public string CodeName { get; set; }
        public ICommand ClickCommand { get;  set; }

        public bool IsChecked 
        {
            get => m_selected;
            set { SetProperty(ref m_selected, value); }
        }
        public string Href { get; set; }
        public string ImageUrl { get; set; }
        public string CharpterUrl { get; set; }
    }

    internal class HistoryPageViewModel : ObservableObject
    {
        private ObservableCollection<ComicInfoItem> m_histotyItem;
        public event NavigateEventDelegate NavigateDetailEvent;

        public ICommand LoadHistoryCommand { get; private set; }
        public ICommand TestCommand { get; private set; }
        public ICommand FullCheckCommand { get; private set; }
        public ICommand FullUnCheckCommand { get; private set; }
        public ICommand DeleteRecordCommand { get; private set; }
        public ICommand ChangePageIndexCommand { get; set; }

        private int m_pageIndex;
        private int m_pageSize = 30;
        private int m_pageCount;
        private List<ComicBook> m_bookList;

        public int PageIndex 
        {
            get => m_pageIndex;
            set { SetProperty(ref m_pageIndex, value); }
        }
  
        public int PageCount 
        {
            get => m_pageCount;
            set { SetProperty(ref m_pageCount, value); }
        }

        public HistoryPageViewModel()
        {
            TestCommand = new RelayCommand<CoverItem>(Click);
            LoadHistoryCommand = new RelayCommand(LoadHistory);         
            HistoryItem = new ObservableCollection<ComicInfoItem>();
            FullCheckCommand = new RelayCommand(FullCheck);
            FullUnCheckCommand = new RelayCommand(FullUnCheck);
            DeleteRecordCommand= new RelayCommand(DeleteRecord);
            ChangePageIndexCommand = new RelayCommand(ChangePageIndex);
        }

        private void FullCheck()
        {
            foreach(var i in HistoryItem)
            {
                i.IsChecked = true;
            }
        }

        private void FullUnCheck()
        {
            foreach (var i in HistoryItem)
            {
                i.IsChecked = false;
            }
        }

        private void ChangePageIndex()
        {
            LoadOnePage();
        }

        private void DeleteRecord()
        {
            List<ComicInfoItem> removeList = new List<ComicInfoItem>();
            for(int i = 0;i < HistoryItem.Count; i++)
            {
                if (HistoryItem[i].IsChecked)
                {
                    removeList.Add(HistoryItem[i]);
                }               
            }

            foreach (var i in removeList)
            {
                HistoryItem.Remove(i);
                m_bookList.Remove(m_bookList.Find(delegate (ComicBook book) { return book.ComicName == i.ComicName; }));
                ComicBookShelf.RemoveHisoty(i.ComicName, i.Href);
            }
        }

        private void Click(CoverItem item)
        {
            if (NavigateDetailEvent != null)
            {
                NavigateDetailEvent(item.MyComicName, item.Href, item.CodeName, item.MyImage, item.ImageUrl);
            }
        }

        private void LoadOnePage()
        {
            HistoryItem.Clear();
            int startIndex = (PageIndex-1)* m_pageSize;
            int endIndex = startIndex + m_pageSize;
            endIndex = endIndex < m_bookList.Count ? endIndex : m_bookList.Count;

            for(int i = startIndex; i < endIndex;i++)
            {
                HistoryItem.Add(new ComicInfoItem
                {
                    ComicName = m_bookList[i].ComicName,
                    ClickCommand = this.TestCommand,
                    CodeName = m_bookList[i].CodeName,
                    ImageUrl = m_bookList[i].ImgUrl,
                    Href = m_bookList[i].ComicHref,
                    CharpterUrl = m_bookList[i].CharpterUrl,
                    IsChecked = false
                });
            }
        }

        private void LoadHistory()
        {
            m_bookList = ComicBookShelf.GetAllHistory();
            m_bookList.Sort((x, y) => x.CompareTo(y));       
            PageIndex = 1;
            PageCount = m_bookList.Count % m_pageSize == 0 ? m_bookList.Count / m_pageSize:
                                                             m_bookList.Count / m_pageSize + 1;
            LoadOnePage();
        }

        public ObservableCollection<ComicInfoItem> HistoryItem
        {
            get => m_histotyItem;
            set { SetProperty(ref m_histotyItem, value); }
        }
    }
}
