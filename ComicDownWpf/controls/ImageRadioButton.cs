using ComicPlugin;
using Panuon.UI.Silver;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ComicDownWpf.controls
{
    public class ImageRadioButton : RadioButton
    {
        public ImageRadioButton() 
        {
            
        }

        public IComicDecoder Decoder
        {
            get { return (IComicDecoder)GetValue(DecoderProperty); }
            set { SetValue(DecoderProperty, value); }
        }

        public static readonly DependencyProperty DecoderProperty =
            DependencyProperty.Register("Decoder", typeof(IComicDecoder), typeof(ImageRadioButton), new PropertyMetadata(null));

    }
}
