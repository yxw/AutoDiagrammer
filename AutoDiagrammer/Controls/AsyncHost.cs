using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;


namespace AutoDiagrammer
{

    public enum AsyncType { Content = 1, Busy, Error };


    /// <summary>
    /// A generic threading host container that supports content/busy/error controls
    /// </summary>
    public class AsyncHost : Grid
    {
        #region ShouldCollapse

        /// <summary>
        /// ShouldCollapse Dependency Property
        /// </summary>
        public static readonly DependencyProperty ShouldCollapseProperty =
            DependencyProperty.Register("ShouldCollapse", typeof(bool), typeof(AsyncHost),
                new FrameworkPropertyMetadata((bool)false));

        /// <summary>
        /// Gets or sets the ShouldCollapse property. 
        /// </summary>
        public bool ShouldCollapse
        {
            get { return (bool)GetValue(ShouldCollapseProperty); }
            set { SetValue(ShouldCollapseProperty, value); }
        }

        #endregion

        #region AsyncState
        public static readonly DependencyProperty AsyncStateProperty =
            DependencyProperty.Register("AsyncState", typeof(AsyncType), typeof(AsyncHost),
            new FrameworkPropertyMetadata((AsyncType)AsyncType.Content, new PropertyChangedCallback(OnAsyncStateChanged)));

        public AsyncType AsyncState
        {
            get { return (AsyncType)GetValue(AsyncStateProperty); }
            set { SetValue(AsyncStateProperty, value); }
        }

        private static void OnAsyncStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AsyncHost)d).OnAsyncStateChanged(e);
        }

        protected virtual void OnAsyncStateChanged(DependencyPropertyChangedEventArgs e)
        {
            foreach (UIElement element in Children)
            {
                if (element.GetValue(AsyncHost.AsyncContentTypeProperty).Equals(e.NewValue))
                    element.Visibility = Visibility.Visible;
                else
                {

                    element.Visibility = ShouldCollapse ? Visibility.Collapsed : Visibility.Hidden;
                }
            }
        }
        #endregion

        #region AsyncContentType
        public static readonly DependencyProperty AsyncContentTypeProperty =
            DependencyProperty.RegisterAttached("AsyncContentType", typeof(AsyncType), typeof(Control));

        public static void SetAsyncContentType(UIElement element, AsyncType value)
        {
            element.SetValue(AsyncContentTypeProperty, value);
        }

        public static AsyncType GetAsyncContentType(UIElement element)
        {
            return (AsyncType)element.GetValue(AsyncContentTypeProperty);
        }
        #endregion
    }
}
