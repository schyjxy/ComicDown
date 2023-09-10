using ComicDownWpf.decoder;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace ComicDownWpf.controls
{
    /// <summary>
    /// ImagePanel.xaml 的交互逻辑
    /// </summary>
    public partial class ImagePanel : UserControl
    {
        private int m_index;
        private string m_url;
        private DownLoadState downLoadState;
        private readonly object lockObj = new object();
        private double imageRate = -1;
        private bool m_shouldLoad = false;
        private bool m_stop = false;
        private Stream ms = null;

        enum DownLoadState
        {
            Idle,
            Downing,
            Success,
            DownFailed,
            LoadFinished
        }

        public ImagePanel()
        {
            InitializeComponent();
            Loaded += ImagePanel_Loaded;
            m_shouldLoad = false;
            //IsEnabledChanged += ImagePanel_IsEnabledChanged;
            downLoadState = DownLoadState.Idle;
        }

        public string ImageUrl
        {
            get { return m_url; }
            set { m_url = value; }
        }

        public string CodeName { get; internal set; }
        public string CachePath { get; internal set; }

        private bool DownImage()
        {
            var failCount = 0;
            bool result = false;

            if (ImageUrl == null)
            {
                return false;
            }

            while (failCount++ < 3)
            {
                if (m_stop)
                {
                    downLoadState = DownLoadState.DownFailed;
                    return false;
                }

                //Console.WriteLine("控件{0}，下载:{1}", Index, ImageUrl);
                result = ParserManager.DownLoadImage(CodeName, CachePath, ImageUrl);//下载成功

                if (result)
                    break;

                File.Delete(CachePath);
                Thread.Sleep(50);
            }

            if (result)
            {
                BitmapImage bitmap = new BitmapImage(new Uri(CachePath, UriKind.Absolute));
                imageRate = bitmap.Height * 1.0 / bitmap.Width;
                downLoadState = DownLoadState.Success;

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    InvalidateMeasure();
                }));
            }
            else
            {
                downLoadState = DownLoadState.DownFailed;
            }

            return result;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            var size = base.MeasureOverride(constraint);

            if (imageRate == -1)
            {
                return size;
            }

            var newSize = new Size(constraint.Width, imageRate * constraint.Width);
            return newSize;
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            // Console.WriteLine("Index {0} 得到高度:{1}, 得到宽度：{2}\n", Index, arrangeBounds.Height, arrangeBounds.Width);          
            return base.ArrangeOverride(arrangeBounds);
        }

        private void ImagePanel_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            //Console.WriteLine("{0}  控件{1} Load 事件加载", DateTime.Now.ToString(), Index);        
            if (downLoadState != DownLoadState.LoadFinished)
            {
                CheckIfPrepared();
            }
        }

        private bool CheckIfPrepared()
        {
            Task.Run(() =>
            {
               lock(lockObj)
               {
                    //Console.WriteLine("{0} 控件{1} CheckIfPrepared", DateTime.Now.ToString(), Index);

                    if (downLoadState != DownLoadState.Idle)
                    {
                        // Console.WriteLine("{0} 控件{1} CheckIfPrepared 返回", DateTime.Now.ToString(), Index);
                        return;
                    }

                    if (!File.Exists(CachePath))
                    {
                        //Console.WriteLine("{0} 线程{1} 控件{2} CheckIfPrepared 下载图片", DateTime.Now.ToString(), Thread.CurrentThread.ManagedThreadId,  Index);
                        downLoadState = DownLoadState.Downing;
                        if (DownImage())
                        {
                            //Console.WriteLine("{0} 线程{1} 控件{2}  下载图片成功，加载图片", DateTime.Now.ToString(), Thread.CurrentThread.ManagedThreadId, Index);
                            if (ShouldRead)
                            {
                                LoadCache();
                            }                           
                        }
                        else
                        {
                            //Console.WriteLine("{0} 线程{1} 控件{2}  下载图片失败", DateTime.Now.ToString(), Thread.CurrentThread.ManagedThreadId, Index);
                        }
                    }
                    else
                    {
                        //Console.WriteLine("{0} 控件{1} CheckIfPrepared 检测到文件已经存在", DateTime.Now.ToString(), Index);
                        if (imageRate == -1)
                        {
                            BitmapImage bitmap = new BitmapImage(new Uri(CachePath, UriKind.Absolute));
                            imageRate = bitmap.Height * 1.0 / bitmap.Width;
                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                InvalidateMeasure();
                            }));
                        }

                    }
                }
      
            });

            return true;
        }

        public int Index
        {
            get { return m_index; }
            set
            {   m_index = value;
                textBox.Text = m_index.ToString();
            }
        }

        public bool ShouldRead
        {
            get { return m_shouldLoad; }
            set
            {
                if (value && m_shouldLoad != value)//要加载图片
                {                
                    Task.Run(() =>
                    {
                        lock (lockObj)
                        {
                            if (!File.Exists(CachePath))
                            {
                                Console.WriteLine("线程{0} 控件{1} ShouldRead 下载图片", Thread.CurrentThread.ManagedThreadId, Index);
                                downLoadState = DownLoadState.Downing;

                                if (DownImage())
                                {
                                    //Console.WriteLine("{0} 控件{1} ShouldRead 下载图片完毕", Thread.CurrentThread.ManagedThreadId, Index);
                                    LoadCache();
                                }

                            }
                            else
                            {
                                Console.WriteLine("线程{0} 控件{1} ShouldRead 直接加载图片", Thread.CurrentThread.ManagedThreadId, Index);
                                LoadCache();
                            }
                        }
  
                    });
                }

                if (!value && m_shouldLoad != value)//释放内存
                {
                    Task.Run(() =>
                    {
                        lock (lockObj)
                        {
                            //Console.WriteLine("线程 {0} 控件{1} 准备释放", Thread.CurrentThread.ManagedThreadId, Index);
                            Dispatcher.Invoke(new Action(() =>
                            {
                                FreeImage();
                            }));
                        }

                    });

                                   
                }

                m_shouldLoad = value;
            }
        }

        private void LoadCache()
        {
            if (!File.Exists(CachePath) )
                return;
            Console.WriteLine("======={0} 线程{1} 加载控件{2}========", DateTime.Now.ToString(), Thread.CurrentThread.ManagedThreadId, Index);
          
            if (ms != null)
            {
                ms.Dispose();
                ms.Close();
            }

            ms = new MemoryStream(File.ReadAllBytes(CachePath));

            Dispatcher.Invoke(new Action(() =>
            {
                BitmapImage m_bitmap = new BitmapImage();
                m_bitmap.BeginInit();
                m_bitmap.CacheOption = BitmapCacheOption.None;
                m_bitmap.StreamSource = ms;
                m_bitmap.EndInit();
                m_bitmap.Freeze();

                img.Source = m_bitmap;
                img.UpdateLayout();
                //Console.WriteLine("索引图片 {0}, 比例:{1}", Index, imageRate);
                //Console.WriteLine("当前宽:{0}, 高:{1}", this.RenderSize.Width, this.RenderSize.Height);              
                imageRate = m_bitmap.Height * 1.0 / m_bitmap.Width;
                downLoadState = DownLoadState.LoadFinished;
            }));     
        }

        private void FreeImage()
        {
            img.Source = null;
            img.UpdateLayout();
            ms?.Dispose();
            ms?.Close();
            ms = null;
            downLoadState = DownLoadState.Idle;
            Console.WriteLine("线程 {0} 控件{1}释放", Thread.CurrentThread.ManagedThreadId, Index);
        }

        public void Close()
        {
            m_stop = true;
            FreeImage();
            GC.Collect();
        }
    }
}
