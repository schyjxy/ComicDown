using ComicDownWpf.viewmodel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;

namespace ComicDownWpf.Windows
{
    /// <summary>
    /// CheckWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CheckWindow : Window
    {
        public CheckWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            CharpterList = new ObservableCollection<CharpterItem>();
            SelectedChapters = new ObservableCollection<CharpterItem>();
        }

        public bool ShouldDownLoad { get; set; }
        public ObservableCollection<CharpterItem> CharpterList { get; set; }
        public ObservableCollection<CharpterItem> SelectedChapters { get; set; }

        private void fullCheck_Click(object sender, RoutedEventArgs e)
        {
            int count = listBox.Items.Count;

            for(int i = 0;i < count;i++)
            {
                listBox.SelectedItems.Add(listBox.Items[i]);
            }
        }

        private void fullUnCheck_Click(object sender, RoutedEventArgs e)
        {
            listBox.SelectedItems.Clear();
        }

        private void SetCheck()
        {
            SelectedChapters.Clear();

            foreach (var i in listBox.SelectedItems)
            {
                CharpterItem item = i as CharpterItem;
                item.IsChecked = true;
                SelectedChapters.Add(item);
            }
        }

        private void downLoadBtn_Click(object sender, RoutedEventArgs e)
        {
            SetCheck();
            ShouldDownLoad = true;
            this.Close();
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            ShouldDownLoad = false;
            this.Close();
        }
    }
}
