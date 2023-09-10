using ComicDownWpf.controls;
using ComicPlugin;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ComicDownWpf.viewmodel
{
    public delegate void NavigateEventDelegate(string comicName, string href, string codeName, ImageSource image, string imgurl);
    

    internal class ComicItemPageViewModel:ObservableObject
    {
        private Task loadTask;
        private ObservableCollection<ComicInfoItem> m_comicList;
        public event NavigateEventDelegate NavigateDetailEvent;

        public ObservableCollection<ComicInfoItem> ComicList
        {
            get => m_comicList;
            set { SetProperty(ref m_comicList, value); }
        }

        public ComicItemPageViewModel() 
        {
            ComicList = new ObservableCollection<ComicInfoItem>();
        }

        public void LoadComic(IComicDecoder decoder)
        {
            if (loadTask != null)
            {
                if (loadTask.Status == TaskStatus.Running)
                {
                    return;
                }

                loadTask.Dispose();
            }

            loadTask = new Task(new Action(() => {
                var list = decoder.GetHotComic();

                App.Current.Dispatcher.Invoke(() =>
                {
                    ComicList.Clear();

                    foreach (var i in list)
                    {
                        var item = new ComicInfoItem();
                        item.ComicName = i.ComicName;
                        item.Href = i.Href;
                        item.ClickCommand = new RelayCommand<CoverItem>(ItemClick);
                        item.CodeName = i.CodeName;
                        item.ImageUrl = i.ImageUrl;
                        ComicList.Add(item);
                    }
                });

              
            }));
            loadTask.Start();
        }

        private void ItemClick(CoverItem item)
        {
            if(NavigateDetailEvent != null)
            {
                NavigateDetailEvent(item.MyComicName, item.Href, item.CodeName, item.MyImage, item.ImageUrl);
            }
        }


    }
}
