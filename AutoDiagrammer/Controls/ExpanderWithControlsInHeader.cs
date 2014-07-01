using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace AutoDiagrammer
{
    public class ExpanderWithControlsInHeader : Expander
    {

        static ExpanderWithControlsInHeader()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ExpanderWithControlsInHeader),
            new FrameworkPropertyMetadata(typeof(ExpanderWithControlsInHeader)));
        }

        #region HeaderContentControl

        /// <summary>
        /// HeaderContentControl Dependency Property
        /// </summary>
        public static readonly DependencyProperty HeaderContentControlProperty =
            DependencyProperty.Register("HeaderContentControl", typeof(FrameworkElement), typeof(ExpanderWithControlsInHeader),
                new FrameworkPropertyMetadata((FrameworkElement)null));


        public FrameworkElement HeaderContentControl
        {
            get { return (FrameworkElement)GetValue(HeaderContentControlProperty); }
            set { SetValue(HeaderContentControlProperty, value); }
        }

        #endregion
    }
}
