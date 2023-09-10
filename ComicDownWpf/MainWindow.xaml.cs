using ComicDownWpf.decoder;
using System.Windows;
using System;
using ComicDownWpf.controls;
using System.Windows.Data;
using ComicDownWpf.viewmodel;
using ComicDownWpf.converter;
using System.Threading.Tasks;
using ComicPlugin;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ComicDownWpf.pages;
using System.Windows.Input;
using System.Web.UI.WebControls;
using System.Collections.ObjectModel;

namespace ComicDownWpf
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
        Point clickPoint;
 

        public MainWindow()
        {
            InitializeComponent();
        }

        private void DMSkinSimpleWindow_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void DMSkinSimpleWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CommonManager.Close();
        }

        private void DMSkinSimpleWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == System.Windows.Input.Key.Back)
            {
                if(mainFrame.CanGoBack)
                    mainFrame.GoBack();
            }
        }
    
        private void DMSkinSimpleWindow_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            clickPoint = e.GetPosition(this);
        }

        private void DMSkinSimpleWindow_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //Point endPoint = e.GetPosition(this);
            //var rate = (endPoint.Y - clickPoint.Y) * 1.0 / (endPoint.X - clickPoint.Y);
            
            ////Console.WriteLine("斜率: {0}, 角度 {1}", rate, Math.Atan(rate));
            //if (rate > 0 && rate < 1)
            //{
            //    if (mainFrame.CanGoBack)
            //    mainFrame.GoBack();
            //}
        }

        private void DMSkinSimpleWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ButtonState == MouseButtonState.Pressed && e.ChangedButton == MouseButton.Middle)
            {
                if (mainFrame.CanGoBack)
                mainFrame.GoBack();
            }
        }
    }
}
