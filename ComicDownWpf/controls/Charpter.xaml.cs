using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace ComicDownWpf.controls
{
    /// <summary>
    /// Charpter.xaml 的交互逻辑
    /// </summary>
    public partial class Charpter : UserControl
    {
        public Charpter()
        {
            InitializeComponent();
            (this.Content as FrameworkElement).DataContext = this;
        }

        public ICommand ClickCommand
        {
            get { return (ICommand)GetValue(ClickCommandProperty); }
            set { SetValue(ClickCommandProperty, value); }
        }

        public static readonly DependencyProperty ClickCommandProperty =
        DependencyProperty.Register("ClickCommand", typeof(ICommand), typeof(Charpter), new PropertyMetadata(null));

        public bool IsChecked
        {
            get { return (bool)GetValue(IsCheckedProperty); }
            set 
            { 
                SetValue(IsCheckedProperty, value);           
            }
        }

        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register("IsChecked", typeof(bool), typeof(Charpter), new PropertyMetadata(false));


        public Charpter Parms
        {
            get { return this; }

        }

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(Charpter), new PropertyMetadata(null));


        public string Href
        {
            get { return (string)GetValue(HrefProperty); }
            set { SetValue(HrefProperty, value); }
        }

        public static readonly DependencyProperty HrefProperty =
            DependencyProperty.Register("Href", typeof(string), typeof(Charpter), new PropertyMetadata(null));


        public bool IsLastRead
        {
            get { return (bool)GetValue(IsLastReadProperty); }
            set { SetValue(IsLastReadProperty, value); }
        }

        public static readonly DependencyProperty IsLastReadProperty =
            DependencyProperty.Register("IsLastRead", typeof(bool), typeof(Charpter), new PropertyMetadata(false));


    }

    public class CharpterBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool ret = (bool)value;

            if (ret)
            {
                return new SolidColorBrush(System.Windows.Media.Color.FromRgb(0x00, 0x7A, 0xCC));
            }

            return new SolidColorBrush(System.Windows.Media.Color.FromRgb(0xC0, 0xC0, 0xC0));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
