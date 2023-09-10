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
using System.Windows.Navigation;
using System.Windows.Shapes;
using ComicPlugin;

namespace TestDecoder
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        List<IComicDecoder> decoderList;
        int decoderIndex = 0;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            decoderList = new List<IComicDecoder>();
            LoadPlugins();
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
                //Create a new instance of all found types

                if(type.Name != "CommonParser")
                {
                    decoderList.Add((IComicDecoder)Activator.CreateInstance(type));
                    Dispatcher.Invoke(new Action(() =>
                    {
                        listView.Items.Add(type.Name);
                    }));
                   
                }
                
            }
        }

        private void ImageDeal(string url)
        {
            Console.WriteLine("图片真实地址：{0}", url);
        }

        private void FindComicInfo(ComicDetailInfo info)
        {
            Console.WriteLine("{0}", info.Description);
            Console.WriteLine("=========测试图片获取=========");
            foreach(var i in info.Charpter)
            {
                decoderList[decoderIndex].GetComicImage(i.Value, ImageDeal);
            }
            
        }
        private void AppendText(string text)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                outputTextBox.AppendText(text + "\n");
            }));
        }

        private void TestHotComic()
        {           
            if (decoderList.Count > 0)
            {
                var list = decoderList[decoderIndex].GetHotComic();

                foreach(var i in list)
                {
                    Console.WriteLine("{0}, href:{1}, 图片地址: {2}", i.ComicName, i.Href, i.ImageUrl);
                    AppendText(string.Format("{0}, href:{1}, 图片地址: {2}", i.ComicName, i.Href, i.ImageUrl));
                }

                foreach(var i in list)
                {
                    FindComicInfo(decoderList[decoderIndex].GetComicInfo(i.Href));
                    System.Threading.Thread.Sleep(50);
                }
            }
        }

        private void TestHttp()
        {
            HttpUtil util = new HttpUtil();
            var url = "https://jmcomic3.cc/";
            url = "https://18comic2.art";
            var response = util.GetHttpWebRequest(url);
            response = util.GetWebClient(url);
            response = util.GetWebRequest(url);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //TestHttp();

            decoderIndex = listView.SelectedIndex;
            Task.Run(new Action(() =>
            {
                TestHotComic();
            }));

        }

        
    }
}
