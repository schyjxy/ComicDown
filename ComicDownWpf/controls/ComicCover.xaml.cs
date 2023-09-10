using System;
using System.IO;
using System.Net;
using System.Windows.Media;
using System.Collections;
using System.Collections.Generic;
using System.Windows;

namespace ComicDownWpf.controls
{
    /// <summary>
    /// ComicCover.xaml 的交互逻辑
    /// </summary>
    /// 
    public partial class ComicCover
    {
        public ComicCover()
        {
            InitializeComponent();
            this.MouseRightButtonUp += ComicCover_MouseRightButtonUp;
        }

        private void ComicCover_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
           
        }

        public bool IsNew
        {
            get { return (bool)GetValue(IsNewProperty); }
            set { SetValue(IsNewProperty, value); }
        }

        public static readonly DependencyProperty IsNewProperty =
            DependencyProperty.Register("IsNew", typeof(bool), typeof(ComicCover), new PropertyMetadata(false));


        public bool IsChecked
        {
            get { return (bool)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }


        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register("IsChecked", typeof(bool), typeof(ComicCover), new PropertyMetadata(false));


        public string ComicName
        {
            get { return comicName.Text; }
            set
            {
                comicName.Text = value;
                this.ToolTip = value;
            }
        }

        public string CodeName { get; set; }
        public string Href { get; set; }
        public string ImageUrl { get; set; }

        public ImageSource CoverImg
        {
            get { return coverImage.Source; }
            set {                  
                coverImage.Source = value;                    
            }
        }

        public void FreeMemory()
        {
            CoverImg = null;
            coverImage.Source = null;
            coverImage.UpdateLayout();
            this.UpdateLayout();
        }

        private void Grid_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if(!IsChecked)
            checkImg.Visibility = Visibility.Visible;
        }

        private void Grid_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!IsChecked)
                checkImg.Visibility = Visibility.Hidden;
        }

        private void checkImg_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            IsChecked = !IsChecked;
            e.Handled = true;
        }
    }
}
