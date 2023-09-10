using ComicDownWpf.viewmodel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ComicDownWpf.Windows
{
    /// <summary>
    /// BookWindow.xaml 的交互逻辑
    /// </summary>
    public partial class BookWindow : Window
    {
        Timer timer;
        Cursor handCursor;
        private bool m_loadLocal = false;
        private int m_curCharpter = -1;

        public struct CharpterInfo
        {
            public string m_charpterName;
            public string m_comicUrl;
        }

        public string ComicName { get; set; }

        public BookWindow()
        {
            InitializeComponent();
            //handCursor = new Cursor("hand.png");
            timer = new Timer();
            timer.Interval = 3000;
            timer.Elapsed += Timer_Elapsed;
            CharpterInfoList = new List<CharpterInfo>();
            this.DataContext= this;

        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                bottomGrid.Visibility = Visibility.Hidden;
            }));
        }

        public List<CharpterInfo> CharpterInfoList { get; set; }

        public void SetTitle(string title)
        {
            this.Title = title;
            titleText.Text = title;
        }

        public string CodeName { get; set; }
        public string Url { get; set; }
  
        private void ShowComic(string url)
        {
            fluentImageViewer.ComicName = this.ComicName;
            fluentImageViewer.CodeName= this.CodeName;
            fluentImageViewer.StartShow(Title, Url);          
            fluentImageViewer.changePageEvent += FluentImageViewer_changePageEvent;
            fluentImageViewer.changeCharpter += FluentImageViewer_changeCharpter;
        }

        private void FluentImageViewer_changeCharpter(controls.PageEventTypeEnum args)
        {
            if (args == controls.PageEventTypeEnum.NextCharpter)
            {
                if (CharpterInfoList.Count > 0 && m_curCharpter + 1 < CharpterInfoList.Count)
                {
                    LoadNewCharpter(++m_curCharpter);
                }
            }

            if (args == controls.PageEventTypeEnum.LastCharpter)
            {
                if (CharpterInfoList.Count > 0 && m_curCharpter - 1 >= 0)
                {
                    LoadNewCharpter(--m_curCharpter);
                }
            }
        }

        private void FluentImageViewer_changePageEvent(controls.PageEventArgs args)
        {
            Dispatcher.BeginInvoke(new Action(() => {
                pageText.Text = string.Format("({0}/{1})", args.Index + 1, args.PageCount);
            }));

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!m_loadLocal)
            {
                for (int i = 0; i < CharpterInfoList.Count; i++)
                {
                    if (CharpterInfoList[i].m_comicUrl == Url)
                    {
                        m_curCharpter = i;
                        SetTitle(ComicName + " " + CharpterInfoList[i].m_charpterName);
                        break;
                    }
                }

                ShowComic(Url);
            }          
        }

        //本地漫画加载
        public void LoadLocal(string path, string title, string chapterUrl)
        {
            m_loadLocal = true;
            SetTitle(title);
            fluentImageViewer.ComicName = this.ComicName;
            fluentImageViewer.CodeName = this.CodeName;
            fluentImageViewer.changePageEvent += FluentImageViewer_changePageEvent;
            fluentImageViewer.changeCharpter += FluentImageViewer_changeCharpter;          
            fluentImageViewer.LoadLocal(path, title, chapterUrl);
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.Cursor = Cursors.Hand;
                this.DragMove();
            }
        }

        private void EnterFullScreen()
        {
            if (WindowState == WindowState.Normal)
            {
                mainGrid.RowDefinitions[0].Height = new GridLength(0);
                this.WindowState = WindowState.Maximized;          
            }
            
        }

        private void ExitFullScreen()
        {
            if (WindowState == WindowState.Maximized)
            {
                mainGrid.RowDefinitions[0].Height = new GridLength(26);
                this.WindowState = WindowState.Normal;            
            }
           
        }

        private void Grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.Cursor = Cursors.Arrow;
        }

        private void fullScreen_Click(object sender, RoutedEventArgs e)
        {
            EnterFullScreen();
        }

        private void exitFullScreen_Click(object sender, RoutedEventArgs e)
        {
            ExitFullScreen();
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            
        }

        private void bottomGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            timer.Stop();
        }

        private void bottomGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            timer.Start();
        }

        private void stackPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Cursor = Cursors.Arrow;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            fluentImageViewer.Close();     
            GC.Collect();
        }

        private void LoadNewCharpter(int index)
        {
            var info = CharpterInfoList[index];
            Url = info.m_comicUrl;
            SetTitle(ComicName + " " + info.m_charpterName);
            fluentImageViewer.ChangeCharpter(Title, CodeName, Url);
        }

        private void nextCharpter_Click(object sender, RoutedEventArgs e)
        {
            if (CharpterInfoList.Count > 0 && m_curCharpter + 1 < CharpterInfoList.Count)
            {             
                LoadNewCharpter(++m_curCharpter);
            }
        }

        private void lastCharpter_Click(object sender, RoutedEventArgs e)
        {
            if (CharpterInfoList.Count > 0 && m_curCharpter - 1 >= 0)
            {
                LoadNewCharpter(--m_curCharpter);
            }
        }

        private void DMSystemMaxButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                EnterFullScreen();
            }
            else
            {
                ExitFullScreen();
            }
                
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                if (WindowState == WindowState.Normal)
                {
                    EnterFullScreen();
                }
                else
                {
                    ExitFullScreen();
                }

            }

            if(e.Key == Key.Escape)
            {
                ExitFullScreen();
            }
        }
    }

}
