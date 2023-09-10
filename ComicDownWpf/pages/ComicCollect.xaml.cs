using ComicDownWpf.viewmodel;
using System;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using Panuon.UI.Silver;
using System.Windows.Documents;
using ComicDownWpf.Windows;
using ComicDownWpf.controls;

namespace ComicDownWpf.pages
{
    /// <summary>
    /// ComicCollect.xaml 的交互逻辑
    /// </summary>
    public partial class ComicCollect : PageBase
    {
        private int pageSize = 20;
        private List<ComicBook> comicBookList;

        public ComicCollect()
        {
            InitializeComponent();
            this.Loaded += ComicCollect_Loaded;
            comicBookList = new List<ComicBook>();
        }

        public event NavigateEventDelegate NavigateDetailEvent;

        private void ComicCollect_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                LoadCollect();
                pageNation.CurrentIndex = 0;
            }
            catch(Exception ex)
            {

            }
            
        }

        public void LoadItems()
        {
            wraperPanel.Children.Clear();
            int startIndex = (pageNation.CurrentIndex - 1) * pageSize;
            int endIndex = startIndex + pageSize > comicBookList.Count ? comicBookList.Count - 1: startIndex + pageSize;
            loading.IsRunning = true;

            for (int i = startIndex; i < endIndex;i++)
            {
                var comicCover = new controls.ComicCover();
                comicCover.MouseLeftButtonDown += ComicCover_MouseLeftButtonDown;
                comicCover.ComicName = comicBookList[i].ComicName;
                comicCover.Href = comicBookList[i].ComicHref;
                comicCover.CodeName = comicBookList[i].CodeName;
                comicCover.CoverImg = new BitmapImage(new Uri(comicBookList[i].ImgUrl));
                comicCover.ImageUrl = comicBookList[i].ImgUrl;
                wraperPanel.Children.Add(comicCover);
            }

            loading.IsRunning = false;
        }

        public void LoadCollect() 
        {
            comicBookList.Clear();
            comicBookList = ComicBookShelf.GetAllBook();
            comicBookList.Reverse();
            int left = comicBookList.Count % pageSize;
            pageNation.TotalIndex = left == 0 ? comicBookList.Count/pageSize: comicBookList.Count / pageSize + 1;
        }

        private void ComicCover_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var comicCover = sender as controls.ComicCover;

            if (NavigateDetailEvent != null)
            {
                NavigateDetailEvent(comicCover.ComicName, comicCover.Href, comicCover.CodeName, comicCover.CoverImg, comicCover.ImageUrl);
            }
        }

        private void cancelClick_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var list = new List<controls.ComicCover>();
            
            foreach(controls.ComicCover i in wraperPanel.Children)
            {
                if(i.IsChecked)
                {
                    list.Add(i);
                }
            }

            foreach(var i in list)
            {
                i.FreeMemory();
                wraperPanel.Children.Remove(i);
                ComicBookShelf.RemoveComic(i.ComicName, i.Href);
            }
            
        }

        private void syncClick_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void pageNation_CurrentIndexChanged(object sender, Panuon.UI.Silver.Core.CurrentIndexChangedEventArgs e)
        {
            LoadItems();
        }
    }
}
