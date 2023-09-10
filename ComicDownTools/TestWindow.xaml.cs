using ComicPlugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ComicDownTools
{
    /// <summary>
    /// TestWindow.xaml 的交互逻辑
    /// </summary>
    public partial class TestWindow : Window
    {
        int decoderIndex = 3;
        List<IComicDecoder> decoderList;

        public TestWindow()
        {
            InitializeComponent();
            decoderList = new List<IComicDecoder>();
            LoadPlugins();
        }

        private uint errorCount;
        private void TestHotComic()
        {
            if (decoderList.Count > 0)
            {
                IComicDecoder decoder = decoderList[decoderIndex];
                var list = decoder.GetHotComic();


                if(list.Count == 0)
                {
                    Console.WriteLine("失败次数:{0}", ++errorCount);
                }


                foreach (var i in list)
                {
                    //Console.WriteLine("{0}, href:{1}, 图片地址: {2}", i.ComicName, i.Href, i.ImageUrl);

                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        Image image = new Image();
                        image.Source = new BitmapImage(new Uri(i.ImageUrl));
                        wrappnel.Children.Add(image);
                    }));

                    ComicDetailInfo info = decoder.GetComicInfo(i.Href);
                    if(info == null)
                    {
                        ++errorCount;
                    }
                    //AppendText(string.Format("{0}, href:{1}, 图片地址: {2}", i.ComicName, i.Href, i.ImageUrl));
                }

                Console.WriteLine("{0}, 完成一轮测试", decoderList[decoderIndex].CodeName);

                //Dispatcher.Invoke(new Action(() =>
                //{
                //    wrappnel?.Children?.Clear();
                //}));

            }
        }

        private void LoadPlugins()
        {
            string path = @"E:\My Documents\同步\ComicDownLoad\ComicDownWpf\bin\x86\Debug\plugins";

            if (Directory.Exists(path))
            {
                string[] files = Directory.GetFiles(path);

                foreach (string file in files)
                {
                    if (file.EndsWith(".dll"))
                    {
                        Assembly.LoadFile(System.IO.Path.GetFullPath(file));
                    }
                }
            }

            Type interfaceType = typeof(IComicDecoder);
            //Fetch all types that implement the interface IPlugin and are a class
            Type[] types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(p => interfaceType.IsAssignableFrom(p) && p.IsClass)
                .ToArray();

            foreach (Type type in types)
            {
                if (type.Name != "CommonParser")
                {
                    decoderList.Add((IComicDecoder)Activator.CreateInstance(type));
                    Dispatcher.Invoke(new Action(() =>
                    {
                        //listView.Items.Add(type.Name);
                    }));

                }

            }
        }

        private void testStart_Click(object sender, RoutedEventArgs e)
        {
            errorCount = 0;
            Task.Run(() =>
            {
                for(int i = 0;i<10;i++)
                {
                    TestHotComic();
                }

                System.Threading.Thread.Sleep(1000);
                
            });
            
        }
    }
}
