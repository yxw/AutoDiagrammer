using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace AutoDiagrammer
{
    /// <summary>
    /// Allows moving of Popup using a Thumb
    /// </summary>
    public class PopupBehaviours
    {
        #region IsMoveEnabled DP
        public static Boolean GetIsMoveEnabledProperty(DependencyObject obj)
        {
            return (Boolean)obj.GetValue(IsMoveEnabledPropertyProperty);
        }

        public static void SetIsMoveEnabledProperty(DependencyObject obj, Boolean value)
        {
            obj.SetValue(IsMoveEnabledPropertyProperty, value);
        }

        // Using a DependencyProperty as the backing store for IsMoveEnabledProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsMoveEnabledPropertyProperty =
            DependencyProperty.RegisterAttached("IsMoveEnabledProperty",
            typeof(Boolean), typeof(PopupBehaviours), new UIPropertyMetadata(false, OnIsMoveStatedChanged));


        private static void OnIsMoveStatedChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            Thumb thumb = (Thumb)sender;

            if (thumb == null) return;

            thumb.DragStarted -= Thumb_DragStarted;
            thumb.DragDelta -= Thumb_DragDelta;
            thumb.DragCompleted -= Thumb_DragCompleted;

            if (e.NewValue != null && e.NewValue.GetType() == typeof(Boolean))
            {
                thumb.DragStarted += Thumb_DragStarted;
                thumb.DragDelta += Thumb_DragDelta;
                thumb.DragCompleted += Thumb_DragCompleted;
            }

        }
        #endregion

        #region Private Methods
        private static void Thumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            Thumb thumb = (Thumb)sender;
            thumb.Cursor = null;
        }

        private static void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Thumb thumb = (Thumb)sender;
            Popup popup = thumb.Tag as Popup;

            if (popup != null)
            {
                popup.HorizontalOffset += e.HorizontalChange;
                popup.VerticalOffset += e.VerticalChange;
            }
        }

        private static void Thumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            Thumb thumb = (Thumb)sender;
            thumb.Cursor = Cursors.Hand;
        }
        #endregion

    }
}
