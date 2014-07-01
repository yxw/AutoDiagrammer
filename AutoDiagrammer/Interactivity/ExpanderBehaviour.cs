using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Interactivity;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using Microsoft.Expression.Interactivity.Core;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Interop;

namespace AutoDiagrammer
{
    public class ExpanderBehaviour : Behavior<Expander>
    {
        #region AssociatedGraph

        public static readonly DependencyProperty AssociatedGraphProperty =
            DependencyProperty.Register("AssociatedGraph", typeof(PocGraphLayout), typeof(ExpanderBehaviour),
                new FrameworkPropertyMetadata((PocGraphLayout)null));

        public PocGraphLayout AssociatedGraph
        {
            get { return (PocGraphLayout)GetValue(AssociatedGraphProperty); }
            set { SetValue(AssociatedGraphProperty, value); }
        }

        #endregion

        #region Ctor
        public ExpanderBehaviour()
        {
            this.ReLayoutCommand = new ActionCommand(this.Relayout);
        }
        #endregion

        #region Public Properties
        public ICommand ReLayoutCommand
        {
            get;
            private set;
        }
        #endregion

        #region Private Methods
        private void Relayout()
        {
            if (SettingsViewModel.Instance.ShouldRelayoutOnExpandCollapse)
                AssociatedGraph.Relayout();
        }
        #endregion

    }





}
