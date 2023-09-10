using ComicDownWpf.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ComicDownWpf.controls
{
    /// <summary>
    /// ComicItemxaml.xaml 的交互逻辑
    /// </summary>
    public partial class CoverItem : UserControl
    {
        public CoverItem()
        {
            InitializeComponent();
            (this.Content as FrameworkElement).DataContext = this;
            this.Loaded += CoverItem_Loaded; 
        }

        private void CoverItem_Loaded(object sender, RoutedEventArgs e)
        {
            //LoadImg();
            MyImage = new BitmapImage(new Uri(ImageUrl));
        }

        public ICommand ClickCommand
        {
            get { return (ICommand)GetValue(ClickCommandProperty); }
            set { SetValue(ClickCommandProperty, value); }
        }

        public CoverItem Parms 
        { 
            get { return this; } 
        
        }

        public static readonly DependencyProperty ClickCommandProperty =
            DependencyProperty.Register("ClickCommand", typeof(ICommand), typeof(CoverItem), new PropertyMetadata(null));

        public bool Checked
        {
            get { return (bool)GetValue(CheckedProperty); }
            set 
            { 
                SetValue(CheckedProperty, value); 
            }
        }

        public ImageSource MyImage
        {
            get { return (ImageSource)GetValue(MyImageProperty); }
            set 
            { 
                SetValue(MyImageProperty, value); 
            }
        }

        public static readonly DependencyProperty MyImageProperty =
            DependencyProperty.Register("MyImage", typeof(ImageSource), typeof(CoverItem), new PropertyMetadata(null));


        public string MyComicName
        {
            get { return (string)GetValue(MyComicNameProperty); }
            set { SetValue(MyComicNameProperty, value); }
        }

        public string CodeName
        {
            get { return (string)GetValue(CodeNameProperty); }
            set { SetValue(CodeNameProperty, value); }
        }

        public string ImageUrl
        {
            get { return (string)GetValue(ImageUrlProperty); }
            set 
            {
                SetValue(ImageUrlProperty, value);            
            }
        }

        public static readonly DependencyProperty ImageUrlProperty =
            DependencyProperty.Register("ImageUrl", typeof(string), typeof(CoverItem), new PropertyMetadata(""));



        public static readonly DependencyProperty CodeNameProperty =
            DependencyProperty.Register("CodeName", typeof(string), typeof(CoverItem), new PropertyMetadata(""));


        public static readonly DependencyProperty MyComicNameProperty =
            DependencyProperty.Register("MyComicName", typeof(string), typeof(CoverItem), new PropertyMetadata(null));


        public string Href
        {
            get { return (string)GetValue(HrefProperty); }
            set { SetValue(HrefProperty, value); }
        }

        public static readonly DependencyProperty HrefProperty =
            DependencyProperty.Register("Href", typeof(string), typeof(CoverItem), new PropertyMetadata(""));


        public static readonly DependencyProperty CheckedProperty =
            DependencyProperty.Register("Checked", typeof(bool), typeof(CoverItem), new PropertyMetadata(false));

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            //checkImg.Visibility = Visibility.Visible;
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            //if (Checked)
            //{
            //    checkImg.Visibility = Visibility.Visible;
            //}
            //else
            //{
            //    checkImg.Visibility = Visibility.Hidden;
            //}
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Checked = !Checked;
        }
    }

    public class BoolVisualConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool ret = (bool)value;

            if (ret)
            {
                return Visibility.Visible;
            }

            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((System.Windows.Visibility)value == Visibility.Visible)
            {
                return true;
            }

            return false;
        }
    }

    public class VisualConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool ret = (bool)value;
             
            if (ret)
            {
                return new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x00, 0x7A, 0xCC));
            }

            return new SolidColorBrush();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
