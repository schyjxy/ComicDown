using System;
using System.Collections.Generic;
using System.Linq;
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
using AngleSharp;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

namespace ComicDownTools
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        IHtmlDocument document = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        protected IHtmlDocument GetDocumentByHtmlString(string text)
        {
            var context = BrowsingContext.New(Configuration.Default);
            var parser = context.GetService<IHtmlParser>();
            var document = parser.ParseDocument(text);
            return document;
        }
        private void DealCss()
        {
            string ccsString = cssTextBox.Text;
            try
            {
                if (document != null && ccsString.Length > 0)
                {
                    if (cssComoBox.SelectedIndex == 0)
                    {
                        var collect = document.QuerySelectorAll(ccsString);
                        if (collect != null)
                        {
                            StringBuilder strBuilder = new StringBuilder();
                            foreach (var i in collect)
                            {
                                strBuilder.Append(i.OuterHtml);
                            }

                            outTextBox.Text = strBuilder.ToString();
                        }
                        else
                        {
                            outTextBox.Text = "CSS 匹配不到对象";
                        }
                    }

                    if (cssComoBox.SelectedIndex == 1)
                    {
                        var ele = document.QuerySelector(ccsString);
                        if (ele != null)
                        {
                            outTextBox.Text = ele.OuterHtml;
                        }
                        else
                        {
                            outTextBox.Text = "CSS 匹配不到对象";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                outTextBox.Text = "CSS 错误:" + ex.Message;
            }
        }

        private void cssTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            DealCss();
        }

        private async void requestBtn_Click(object sender, RoutedEventArgs e)
        {
            Encoding encoding;

            switch (textEncodeCombox.SelectedIndex)
            {
                case 0: encoding = Encoding.UTF8;break;
                case 1: encoding = Encoding.GetEncoding("gbk"); break;
                default: encoding = Encoding.UTF8; break;
            }

            string htmlStr = await HttpUtil.Get(inputText.Text, encoding);
            outTextBox.Text = htmlStr;
            document = GetDocumentByHtmlString(htmlStr);
                    
        }

        private void cssComoBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cssTextBox == null)
                return;
            DealCss();
        }

        private async void test_Click(object sender, RoutedEventArgs e)
        {

            //CssFinder cssFinder = new CssFinder();
            //var info = await cssFinder.SearchHot(inputText.Text);

            //if (info != null) 
            //{ 
                
            //}
        }
    }
}
