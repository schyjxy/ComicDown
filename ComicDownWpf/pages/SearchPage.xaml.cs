using ComicDownWpf.decoder;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using ComicPlugin;
using System.Text;
using System.Threading.Tasks;
using ComicDownWpf.viewmodel;

namespace ComicDownWpf.pages
{
    /// <summary>
    /// SearchPage.xaml 的交互逻辑
    /// </summary>
    public partial class SearchPage : PageBase
    {
        private SearchResult searchResult;
        public SearchPage()
        {
            InitializeComponent();
        }

        private void SearchPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            
        }

        private void ShowMenu(SearchResult result)
        {
            //stackPanel.Children.Clear();
            //foreach (var item in result.MenuDict)
            //{
            //    stackPanel.Children.Add(new RadioButton 
            //    {
            //        Content = item.Key,
            //        Foreground = new SolidColorBrush(Color.FromArgb(255,255,255,255)),
            //        Margin = new System.Windows.Thickness(5,0,0,0),
            //        FontSize = 16,
                    
            //    });;;
            //}

            //LoadItem(searchResult.DetailInfoList);
            //pageBtn.Visibility = System.Windows.Visibility.Visible;
            //pageBtn.TotalIndex = result.Count;
        }

        private async void GetComicMenu()
        {
            searchResult = ParserManager.GetMenu("www.mangabz.com");
            ShowMenu(searchResult);           
        }

        private void pageBtn_CurrentIndexChanged(object sender, Panuon.UI.Silver.Core.CurrentIndexChangedEventArgs e)
        {
            
        }
    }
}
