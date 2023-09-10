using ComicDownWpf.decoder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ComicDownWpf.controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Collections;

namespace ComicDownWpf.controls
{
    /// <summary>
    /// CommonComic.xaml 的交互逻辑
    /// </summary>
    public partial class CommonComic : UserControl
    {
        private double posY;
        private string m_cachePath;
        private long pageIndex = 0;
        private double m_ScrollStepLength = 0.0;     //滚动条的归一化步进长度
        private double m_ScrollPosition = 0.0; //滚动条的归一化位置

        public CommonComic()
        {
            InitializeComponent();
        }

        public string ComicName { get; set; }
        public string CodeName { get; set; }


        public UIElementCollection Children { get { return stackPanel.Children; } }


        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //LoadImageByCtrl();
        }

        public void LoadLocal(string path)
        {
            FreeMemory();
            DirectoryInfo dirInfo = new DirectoryInfo(path);           
            FileInfo []fileInfo = dirInfo.GetFiles();
            Array.Sort(fileInfo, new FileNameSort());

            foreach(var i in fileInfo)
            {
                AppendLoaclItem(i.FullName);
            }
        }

        private void stackPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Cursor = Cursors.Arrow;
            var point = e.GetPosition(this);
            var baseLength = smoothScroll.ViewportHeight / (smoothScroll.ExtentHeight - smoothScroll.ViewportHeight);
            var rate = (Math.Abs(posY - point.Y) * 1.0 / smoothScroll.ViewportHeight) * 3;
            var distance = rate * baseLength;
            //Console.WriteLine("占长度的比例:{0}", Math.Abs(posY - point.Y) * 1.0 / smoothScroll.ViewportHeight);

            if (posY - point.Y > 0)
            {
                smoothScroll.SmoothScroll(distance, m_ScrollPosition, ScrollDirection.Down);
            }
            else
            {
                smoothScroll.SmoothScroll(distance, m_ScrollPosition, ScrollDirection.Up);
            }
        }

        private void stackPanel_MouseMove(object sender, MouseEventArgs e)
        {

        }

        public void ReAdjust()//重新调整
        {

        }
        public void FreeMemory()
        {
            FreeSomeImage();
        }

        private void UpdateParam()
        {
            m_ScrollStepLength = smoothScroll.ViewportHeight / (smoothScroll.ExtentHeight - smoothScroll.ViewportHeight);//窗口可见高度/滚动条总高度
            m_ScrollPosition = smoothScroll.ContentVerticalOffset / smoothScroll.ScrollableHeight;//可见内容偏移/总共可偏移的地方
            m_ScrollStepLength = m_ScrollStepLength * 0.6;

            double rate = smoothScroll.ContentVerticalOffset / 1360;
            double nextRate = Math.Round(rate);
            
            if(nextRate > rate)
            {
                if(nextRate < stackPanel.Children.Count)
                {
                    ComicControl comicCtrl = stackPanel.Children[(int)nextRate] as ComicControl;
                    comicCtrl.LoadImage();
                }
               
                //Console.WriteLine("应该翻页了：{0}， {1}", rate, nextRate);
            }         
        }

        private void stackPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Cursor = Cursors.Hand;
            posY = e.GetPosition(this).Y;
            UpdateParam();
        }
        static double last_start_point = 1;

        private void SmoothScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            UpdateParam();
            //Console.WriteLine("last_start_point {0}, {1}", last_start_point, m_ScrollPosition);
            if(last_start_point == m_ScrollPosition)
            {
                //Console.WriteLine("过滤重复数据");
                return;
            }
            last_start_point = m_ScrollPosition;


            if (e.Delta > 0)
            {
                smoothScroll.SmoothScroll(m_ScrollStepLength, m_ScrollPosition, ScrollDirection.Up);
            }
            else
            {
                smoothScroll.SmoothScroll(m_ScrollStepLength, m_ScrollPosition, ScrollDirection.Down);
            }
            e.Handled = true;
        }

        //获取到图片后
        private void GetAImg(string url)
        {
            string fileName = string.Format(@"{0}\{1}.jpg", m_cachePath, pageIndex++);

            Dispatcher.BeginInvoke(new Action(() =>
            {
                ComicControl comicControl = new ComicControl();
                comicControl.Margin = new Thickness(0, 0, 0, 10);
                comicControl.ImageUrl = url;
                comicControl.CodeName = CodeName;
                comicControl.CachePath = fileName;
                stackPanel.Children.Add(comicControl);
            }));
        }

        public void StartShow(string comicName, string codeName, string url)
        {          
            Children.Clear();
            GC.Collect();
            this.ComicName = comicName;
            CodeName = codeName;
            bool hasFile = false;
            pageIndex = 0;
            m_cachePath = Environment.CurrentDirectory + @"\temp\" + comicName;

            if(Directory.Exists(m_cachePath))
            {
                DirectoryInfo dirInfo = new DirectoryInfo(m_cachePath);
                if(dirInfo.GetFiles().Length > 0)
                {
                    hasFile = true;
                }             
            }
           
            if (!hasFile)
            {
                Directory.CreateDirectory(m_cachePath);
                Task.Run(async () =>
                {
                    var list = ParserManager.GetImageList(codeName, url, GetAImg);
                });
            }
            else
            {
                LoadLocal(m_cachePath);
            }            
        }

        private void AppendLoaclItem(string url)
        {
            ComicControl comicControl = new ComicControl();
            comicControl.Margin   = new Thickness(0, 0, 0, 10);
            comicControl.ImageUrl = url;
            comicControl.CodeName = CodeName;
            comicControl.CachePath = m_cachePath;
            stackPanel.Children.Add(comicControl);         
        }

        public void FreeSomeImage()
        {
           foreach(ComicControl control in stackPanel.Children)
           {
              control.FreeMemory();
           }
        }

        private void stackPanel_MouseWheel(object sender, MouseWheelEventArgs e)
        {        
            Console.WriteLine("delta {0}", e.Delta);
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ReAdjust();
        }
    }


}
