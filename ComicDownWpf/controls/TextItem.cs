using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ComicDownWpf.controls
{
    class TextItem:RadioButton
    {
        public string Url
        {
            get { return (String)GetValue(UrlPropety); }
            set { SetValue(UrlPropety, value); }
        }
        public static readonly DependencyProperty UrlPropety =
            DependencyProperty.Register("Url", typeof(String), typeof(TextItem), new PropertyMetadata(null));
    }
}
