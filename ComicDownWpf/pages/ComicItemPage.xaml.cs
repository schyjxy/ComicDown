using ComicDownWpf.controls;
using ComicPlugin;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
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
    public partial class ComicItemPage : PageBase
    {
        Timer timer;

        public ComicItemPage()
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

        private void ComicCover_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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
