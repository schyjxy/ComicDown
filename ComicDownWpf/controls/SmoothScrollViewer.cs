using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace ComicDownWpf.controls
{
    public class SmoothScrollViewer : ScrollViewer
    {
        static bool isScrollFinished = false;
        static double stopPostion  = 0;
        public static bool ScrollFinished { get { return isScrollFinished; } }

        public double VerticalScrollRatio
        {
            get { return (double)GetValue(VerticalScrollRatioProperty); }
            set { isScrollFinished = false; SetValue(VerticalScrollRatioProperty, value); }
        }

        public static void SetParam(bool isFinished, double newStopPostion)
        {
            isScrollFinished = isFinished;
            stopPostion = newStopPostion;
        }

        //注册VerticalScrollRatio依赖属性
        public static readonly DependencyProperty VerticalScrollRatioProperty =
            DependencyProperty.Register("VerticalScrollRatio", typeof(double), typeof(SmoothScrollViewer), new PropertyMetadata(0.0, new PropertyChangedCallback(V_ScrollRatioChangedCallBack)));

        private static void V_ScrollRatioChangedCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var scrollViewer = (ScrollViewer)(d);
            if (scrollViewer != null)
            {
                double postion = (double)(e.NewValue) * scrollViewer.ScrollableHeight;
                
                if(postion >= stopPostion * scrollViewer.ScrollableHeight)
                {
                    isScrollFinished = true;
                    //Console.WriteLine("滚动完成, {0}, {1}", postion, stopPostion * scrollViewer.ScrollableHeight);
                }
                scrollViewer.ScrollToVerticalOffset(postion);
            }
        }

        /// <summary>
        /// 水平归一化步进长度
        /// </summary>
        public double HorizontalScrollRatio
        {
            get { return (double)GetValue(HorizontalScrollRatioProperty); }
            set { SetValue(HorizontalScrollRatioProperty, value); }
        }

        //注册HorizontalScrollRatio依赖属性
        public static readonly DependencyProperty HorizontalScrollRatioProperty =
            DependencyProperty.Register("HorizontalScrollRatio", typeof(double), typeof(SmoothScrollViewer), new PropertyMetadata(0.0, new PropertyChangedCallback(H_ScrollRatioChangedCallBack)));

        private static void H_ScrollRatioChangedCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var scrollViewer = (ScrollViewer)(d);

            if (scrollViewer != null)
            {
                scrollViewer.ScrollToHorizontalOffset((double)(e.NewValue) * scrollViewer.ScrollableWidth);
            }
        }
    }

    public static class ScrollViewerExtend
    {
        struct ScrollInfo
        {
            public SmoothScrollViewer scrollViewer;
            public double scrollStep;
            public double scrollPosition;
            public ScrollDirection direction;
        }

        static Queue<ScrollInfo> queue = new Queue<ScrollInfo>();
        static Storyboard storyboard = new Storyboard();
        static ScrollViewerExtend()
        {
            storyboard.Completed += Storyboard_Completed;
        }


        /// <summary>
        /// 实现ScrollViewer的平滑滚动
        /// </summary>
        /// <param name="scrollViewer"></param>
        /// <param name="scrollStepRatio">归一化步进长度，每一步走多长</param>
        /// <param name="scrollPositionRatio">归一化位置, 每一步起始位置</param>
        /// <param name="direction">滚动方向</param>
        public static void SmoothScroll(this SmoothScrollViewer scrollViewer, double scrollStepRatio, double scrollPositionRatio, ScrollDirection direction)
        {
            ScrollInfo scroll = new ScrollInfo();
            scroll.scrollViewer = scrollViewer;
            scroll.direction = direction;
            scroll.scrollPosition = scrollPositionRatio;
            scroll.scrollStep = scrollStepRatio;

            if (queue.Count == 0)
            {
                queue.Enqueue(scroll);
                DoScroll();
            }
            else //滚动的太多，用户等不及了
            {
                //Console.WriteLine("用户队列:{0}", queue.Count);
                storyboard.Stop();
                storyboard.Children.Clear();
                queue.Clear();
                queue.Enqueue(scroll);
                DoScroll();
            }
           
        }

        private static void DoScroll()
        {
            if (queue.Count == 0 )
                return;
            ScrollInfo scrollInfo = queue.Peek();

            if (double.IsNaN(scrollInfo.scrollStep) || double.IsNaN(scrollInfo.scrollPosition))
                return;

            DoubleAnimation animation = new DoubleAnimation();
            animation.EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut };
            animation.From = scrollInfo.scrollPosition;         

            if (ScrollDirection.Down == scrollInfo.direction || ScrollDirection.Right == scrollInfo.direction)
            {
                double To = scrollInfo.scrollPosition + scrollInfo.scrollStep;
                animation.To = To > 0.95 ? 1.0 : To;//向下（右）滚动补偿
            }
            else if (ScrollDirection.Up == scrollInfo.direction || ScrollDirection.Left == scrollInfo.direction)
            {
                double To = scrollInfo.scrollPosition - scrollInfo.scrollStep;
                animation.To = To < 0 ? 0.0 : To;//向上（左）滚动补偿
            }

            Console.WriteLine("动画位置:{0} - {1}", animation.From, animation.To);

            scrollInfo.scrollViewer.Dispatcher.Invoke(new Action(() =>
            {
                animation.Duration = new Duration(TimeSpan.FromMilliseconds(1000));
                storyboard.Children.Add(animation);
                Storyboard.SetTarget(animation, scrollInfo.scrollViewer);

                SmoothScrollViewer.SetParam(false, (double)animation.To);
                if (ScrollDirection.Down == scrollInfo.direction || ScrollDirection.Up == scrollInfo.direction)
                    Storyboard.SetTargetProperty(animation, new PropertyPath(SmoothScrollViewer.VerticalScrollRatioProperty));
                else if (ScrollDirection.Right == scrollInfo.direction || ScrollDirection.Left == scrollInfo.direction)
                    Storyboard.SetTargetProperty(animation, new PropertyPath(SmoothScrollViewer.HorizontalScrollRatioProperty));

                storyboard.Begin();
            }));
           
        }


        private static void Storyboard_Completed(object sender, EventArgs e)
        {
            if(queue.Count > 0)
                queue.Dequeue();
        }
    }

    public enum ScrollDirection
    {
        Up, Down, Left, Right
    }
}
