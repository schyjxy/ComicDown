using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;
using ComicDownWpf.decoder;

namespace ComicDownWpf.controls
{
    /// <summary>
    /// ComicControl.xaml 的交互逻辑
    /// </summary>
    public partial class ComicControl : UserControl
    {
        LoadState loadState;
    
        public ComicControl()
        {
            InitializeComponent();
            this.Unloaded += ComicControl_Unloaded;
            loadState = LoadState.Cover;
        }

        enum LoadState
        {
            Cover,
            Success
        }

        private void ComicControl_Unloaded(object sender, RoutedEventArgs e)
        {
            FreeMemory();
        }

        private string m_url;
        
        public string ImageUrl 
        {
            get { return m_url; }

            set 
            { 
                m_url = value;
            }
        }

        public string CodeName { get; internal set; }
        public string CachePath { get; internal set; }
        public double PixelHeight { get; set; }
        public double PixelWidth { get; set; }

        public void GetImage(string imagePath)
        {        
            using (Stream ms = new MemoryStream(File.ReadAllBytes(imagePath)))
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = ms;
                bitmap.EndInit();
                bitmap.Freeze();
                img.Source = bitmap; 
                ms.Dispose();
                ms.Close();
            }
            
        }
        public void FreeMemory()
        {
            img.Source = null;
            img.UpdateLayout();
        }

        private void GetImage()
        {
            Task.Run(new Action(() =>
            {             
                Dispatcher.BeginInvoke(new Action(() => {
                    BitmapImage bitmap = new BitmapImage(new Uri(m_url, UriKind.RelativeOrAbsolute));
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;                 
                    img.Source = bitmap;
                    this.PixelWidth = bitmap.Width;
                    this.PixelHeight = bitmap.Height;
                }));           
            }));
            
        }

        public Size ImageSize { get; set; }

        public void LoadImage()
        {
            if (loadState == LoadState.Success)
                return;

            if (m_url.Contains("http"))
            {
                var result = ParserManager.DownLoadImage(CodeName, CachePath, ImageUrl);//下载成功
                if (result)
                {
                    m_url = CachePath;
                    GetImage();
                }
            }
            else
            {
                GetImage();
            }
        }

        private void UserControl_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if(loadState == LoadState.Cover)
            {
                LoadImage();
                loadState = LoadState.Success;
            }
        }
    }
}
