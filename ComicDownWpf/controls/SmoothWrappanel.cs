using NiL.JS.Expressions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Input;
using System.Text.RegularExpressions;
using AngleSharp.Css;

namespace ComicDownWpf.controls
{

    public class FileNameSort : IComparer
    {
        //调用windos 的 DLL
        [System.Runtime.InteropServices.DllImport("Shlwapi.dll", CharSet = CharSet.Unicode)]
        private static extern int StrCmpLogicalW(string param1, string param2);

        //前后文件名进行比较。
        public int Compare(object name1, object name2)
        {
            if (null == name1 && null == name2)
            {
                return 0;
            }
            if (null == name1)
            {
                return -1;
            }
            if (null == name2)
            {
                return 1;
            }
            return StrCmpLogicalW(name1.ToString(), name2.ToString());
        }

    }

    public enum PageEventTypeEnum
    {
        NextPage,
        LastPage,
        FinialPage,
        NextCharpter,
        LastCharpter,
    }

    public class PageEventArgs : RoutedEventArgs
    {
        public PageEventArgs()
        {

        }

        public PageEventArgs(RoutedEvent routedEvent, object source) : base(routedEvent, source)
        {

        }
        public int Index { get; set; }
        public int PageCount { get; set; }
        public PageEventTypeEnum PageEventType { get; set; }
    }

    class SmoothWrappanel : WrapPanel, IScrollInfo
    {
        TranslateTransform _transForm;
        Size _screenSize;
        Size _totalSize;
        
        int pageBuffer = 2;
        int lastIndex = 0;
        bool m_autoMove = false;
        bool m_disableAnimate = false;
        int m_animateTime = 1;
        double m_lastHeight;
        double m_lastWidth;
        double m_rate = 0;

        public SmoothWrappanel()
        {
            _transForm = new TranslateTransform();
            this.RenderTransform = _transForm;
            this.MouseDown += SmoothWrappanel_MouseDown;
            this.MouseMove += SmoothWrappanel_MouseMove;
            this.Loaded += SmoothWrappanel_Loaded;
            
        }

        private void SmoothWrappanel_Loaded(object sender, RoutedEventArgs e)
        {
            ScrollOwner.ScrollChanged += ScrollOwner_ScrollChanged;
        }

        private void ScrollOwner_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollViewer sv = (ScrollViewer)sender;
            Rect svViewportBounds = new Rect(sv.HorizontalOffset, sv.VerticalOffset, sv.ViewportWidth, sv.ViewportHeight);
            List<int> list = new List<int>();

            for (int i = 0; i < Children.Count; ++i)
            {
                var container = Children[i] as ImagePanel;
                if (container != null)
                {
                    var offset = VisualTreeHelper.GetOffset(container);
                    var bounds = new Rect(offset.X, offset.Y, container.RenderSize.Width, container.RenderSize.Height);

                    if (svViewportBounds.IntersectsWith(bounds))
                    {
                        list.Add(i);
                    }

                }
            }

            if (list.Count > 0)
            {
                int curIndex = list[0];

                if (list.Count == 2)
                {
                    PageEventArgs args = new PageEventArgs(PageDownEvent, this);
                    args.Index = curIndex;
                    args.PageCount = Children.Count;
                    args.PageEventType = PageEventTypeEnum.NextPage;
                    this.RaiseEvent(args);                   
                }
                
                if(VerticalOffset + _screenSize.Height >= _totalSize.Height)
                {
                    PageEventArgs args = new PageEventArgs(PageDownEvent, this);
                    args.Index = curIndex;
                    args.PageCount = Children.Count;
                    args.PageEventType = PageEventTypeEnum.FinialPage;
                    this.RaiseEvent(args);
                }

                SetVisuable(curIndex);
                lastIndex = curIndex;
            }

            m_disableAnimate = false;
            list.Clear();
        }

        public int AnimateTime 
        { 
            get { return m_animateTime; }
            set { m_animateTime = value; }
        }

        public static readonly RoutedEvent PageDownEvent = EventManager.RegisterRoutedEvent("PageChanged",
        RoutingStrategy.Bubble, typeof(EventHandler<PageEventArgs>), typeof(SmoothWrappanel));

        public event RoutedEventHandler PageChanged
        {
            add
            {
                base.AddHandler(PageDownEvent, value);
            }
            remove
            {
                base.RemoveHandler(PageDownEvent, value);
            }
        }

        //MeasureOverride传入父容器分配的可用空间，返回该容器根据其子元素大小计算确定的在布局过程中所需的大小。
        protected override Size MeasureOverride(Size availableSize)
        {
            _screenSize = availableSize;
            availableSize = new Size(availableSize.Width, double.PositiveInfinity);
            _totalSize = base.MeasureOverride(availableSize);//计算每个子控件的高度
            //Console.WriteLine("测量得到的宽高:{0}, {1}", availableSize.Width, availableSize.Height);
            
            if(m_lastHeight != _totalSize.Height && m_lastWidth != _totalSize.Width)
            {
                double cal_value = m_rate * _totalSize.Height;
                if(!double.IsNaN(cal_value) && !double.IsInfinity(cal_value))
                {
                    //Console.WriteLine("发生变化, 当前{0} 将要变为 {1}", VerticalOffset, cal_value);
                    VerticalOffset = cal_value;
                    m_disableAnimate = true;                   
                }
                m_lastHeight = _totalSize.Height;
                m_lastWidth = _totalSize.Width;
            }

            return _totalSize;
        }

        //ArrangeOverride传入父容易最终分配的控件大小，返回使用的实际大小
        protected override Size ArrangeOverride(Size finalSize)
        {
            var size = base.ArrangeOverride(finalSize);
            m_rate = VerticalOffset * 1.0 / _totalSize.Height;

            //Console.WriteLine("第一个元素高度： {0}, {1} ", Children[0].RenderSize.Height, Children[0].DesiredSize.Height);
            if (ScrollOwner != null)
            {
                int time = AnimateTime;

                if (m_disableAnimate)
                {
                    time = 0;
                }

                var yOffsetAnimation = new DoubleAnimation() { To = -VerticalOffset, Duration = TimeSpan.FromSeconds(time), EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseOut } };
                _transForm.BeginAnimation(TranslateTransform.YProperty, yOffsetAnimation);

                var xOffsetAnimation = new DoubleAnimation() { To = -HorizontalOffset, Duration = TimeSpan.FromSeconds(time), EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseOut } };
                _transForm.BeginAnimation(TranslateTransform.XProperty, xOffsetAnimation);
                ScrollOwner.InvalidateScrollInfo();
            }

            return _screenSize;
        }

        #region IScrollInfo

        public ScrollViewer ScrollOwner { get; set; }
        public bool CanHorizontallyScroll { get; set; }
        public bool CanVerticallyScroll { get; set; }

        public double ExtentHeight { get { return _totalSize.Height; } }
        public double ExtentWidth { get { return _totalSize.Width; } }
        public double HorizontalOffset { get; private set; }
        public double VerticalOffset { get; private set; }
        public double ViewportHeight { get { return _screenSize.Height; } }
        public double ViewportWidth { get { return _screenSize.Width; } }

        private void appendOffset(double x, double y)
        {
            var offset = new Vector(HorizontalOffset + x, VerticalOffset + y);
            offset.Y = range(offset.Y, 0, _totalSize.Height - _screenSize.Height);
            offset.X = range(offset.X, 0, _totalSize.Width - _screenSize.Width);

            HorizontalOffset = offset.X;
            VerticalOffset = offset.Y;

            if(_totalSize.Height == 0)
            {
                return;
            }

            InvalidateArrange();
        }

        //平均分配
        public void SetVisuable(int startIndex)
        {
            var left = startIndex - pageBuffer;
            var right = startIndex + pageBuffer;
            //Console.WriteLine("当前索引:{0}, 区间 {1}, {2}", startIndex, left, right);

            for (int i = 0; i < Children.Count; i++)
            {
                ImagePanel panel = Children[i] as ImagePanel;

                if (i >= left && i <= right)
                {
                   if(!panel.ShouldRead)
                    panel.ShouldRead = true;
                    //if (i < 10)
                    //    Console.WriteLine("{0} 可见", i);
                }
                else
                {
                    if(panel.ShouldRead)
                    panel.ShouldRead = false;
                    //if( i < 10)
                    // Console.WriteLine("{0} 不可见", i);
                }
            }

            //Console.WriteLine("-------end--------");
        }

        double range(double value, double value1, double value2)
        {
            var min = Math.Min(value1, value2);
            var max = Math.Max(value1, value2);

            value = Math.Max(value, min);
            value = Math.Min(value, max);
            return value;
        }

        const double _lineOffset = 90;
        const double _wheelOffset = 90;
        Point last_point;

        public void LineDown()
        {
            appendOffset(0, _screenSize.Height/2);
            //appendOffset(0, _screenSize.Height);
        }

        public void LineUp()
        {
            appendOffset(0, -(_screenSize.Height / 2));
        }

        public void LineLeft()
        {
            appendOffset(-_lineOffset, 0);
        }

        public void LineRight()
        {
            appendOffset(_lineOffset, 0);
        }

        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            return rectangle;
        }

        public void MouseWheelDown()
        {
            appendOffset(0, _lineOffset);//
        }

        public void MouseWheelUp()
        {
            appendOffset(0, -(_lineOffset));
        }

        public void MouseWheelLeft()
        {
            appendOffset(0, _wheelOffset);
        }

        public void MouseWheelRight()
        {
            appendOffset(_wheelOffset, 0);
        }

        public void PageDown()
        {
            appendOffset(0, _screenSize.Height);
        }

        private void SmoothWrappanel_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (m_autoMove)
            {
                //MouseWheelDown();
                var curPoint = e.GetPosition(this);

                if(curPoint.Y - last_point.Y > 0)
                {
                    LineDown();
                }
                else
                {
                    LineUp();
                }
                last_point = curPoint;
               
            }
        }

        private void SmoothWrappanel_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {          
            if(e.ChangedButton == System.Windows.Input.MouseButton.Middle && e.ButtonState == System.Windows.Input.MouseButtonState.Pressed)
            {
                m_autoMove = !m_autoMove;
            }
        }

        public void PageUp()
        {
            appendOffset(0, -_screenSize.Height);
        }

        public void PageLeft()
        {
            appendOffset(-_screenSize.Width, 0);
        }

        public void PageRight()
        {
            appendOffset(_screenSize.Width, 0);
        }

        public void SetVerticalOffset(double offset)
        {
            this.appendOffset(HorizontalOffset, offset - VerticalOffset);
        }

        public void SetHorizontalOffset(double offset)
        {
            this.appendOffset(offset - HorizontalOffset, VerticalOffset);
        }

        #endregion
    }
}
