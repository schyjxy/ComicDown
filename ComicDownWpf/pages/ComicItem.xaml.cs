using System;
using System.IO;
using System.Net;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace ComicDownWpf.pages
{
    /// <summary>
    /// ComicItem.xaml 的交互逻辑
    /// </summary>
    /// 
    public partial class ComicItem : PageBase
    {
        Timer timer;

        public ComicItem()
        {
            InitializeComponent();
            InitTimer();
        }

        private void InitTimer()
        {
            timer = new Timer();
            timer.Interval = 1000;
            timer.Elapsed += Timer_Elapsed;
            timer.Enabled = false;
            timer.Stop();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            timer.Stop();
            Dispatcher.Invoke(new Action(() =>
            {
                scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            }));
        }

        public Stream DownLoad(string url)//下载模块，添加gzip支持
        {
            Stream imgStream = null;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Accept = "image/webp,image/*,*/*;q=0.8";
            request.Method = "GET";
            request.KeepAlive = true;
            request.Host = new Uri(url).Host;
            //request.Headers.Add("Accept-Encoding:gzip, deflate");
            request.Referer = url;
            request.Headers.Add("Cache-Control:max-age=0");
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/50.0.2661.102 Safari/537.36";
            request.Timeout = 10000;

            try
            {
                HttpWebResponse resp = (HttpWebResponse)request.GetResponse();
                imgStream = resp.GetResponseStream();
                return imgStream;
            }
            catch (Exception ex)
            {
                Console.WriteLine("-----下载超时------, {0}", ex.Message);
                return imgStream;
            }
        }

        public override async void LoadItem()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                wraperPanel.Children.Clear();

                foreach (var i in CommonManager.ComicInfoList)
                {
                    controls.ComicCover comicCover = new controls.ComicCover();
                    comicCover.MouseLeftButtonDown += ComicCover_MouseLeftButtonDown;
                    Uri uri = new Uri(i.ImageUrl);

                    try
                    {
                        comicCover.CoverImg = new BitmapImage(uri);
                        comicCover.ComicName = i.ComicName;
                        comicCover.CodeName = i.CodeName;
                        comicCover.Href = i.Href;
                        wraperPanel.Children.Add(comicCover);
                        //Console.WriteLine("comicName:{0}, href:{1}", i.ComicName, i.Href);

                        if(i.ComicName == null || i.Href == null)
                        {
                            Console.WriteLine("出现");
                        }
                    }

                    catch(Exception ex)
                    {
                        Console.WriteLine("这里报错:" + ex.StackTrace);
                    }
                }

            }));        

        }

        private void ComicCover_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            controls.ComicCover comicCover = sender as controls.ComicCover;
            ComicPage comicInfo = new ComicPage();
            comicInfo.CodeName = comicCover.CodeName;
            comicInfo.ComicName = comicCover.ComicName;
            comicInfo.ComicImage = comicCover.CoverImg;
            comicInfo.Href = comicCover.Href;
            MainFrame.Navigate(comicInfo);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void scrollViewer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            if (!timer.Enabled)
            {
                timer.Start();
            }
        }
    }
}
