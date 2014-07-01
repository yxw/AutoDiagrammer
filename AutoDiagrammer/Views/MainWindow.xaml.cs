using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GraphSharp.Controls;
using GraphSharp.Algorithms.Layout.Simple.FDP;
using GraphSharp.Algorithms.Layout.Simple.Tree;
using GraphSharp.Algorithms.Layout.Compound.FDP;
using GraphSharp.Algorithms.Layout.Simple.Hierarchical;
using System.Windows.Controls.Primitives;
using WPFExtensions.Controls;
using System.ComponentModel;


namespace AutoDiagrammer
{



    public partial class MainWindow : Window, IGraphPrintableWindow
    {
        private Cinch.WPFMessageBoxService messageBoxService = new Cinch.WPFMessageBoxService();
        private DragAndDropHelper dragAndDropHelper = new DragAndDropHelper();
        

        public MainWindow()
        {
            InitializeComponent();
            this.tree.Focus();
 
            //http://blogs.msdn.com/b/llobo/archive/2007/03/05/listening-to-dependencyproperty-changes.aspx
            DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(ZoomControl.ZoomProperty, typeof(ZoomControl));
            if (dpd != null)
            {
                dpd.AddValueChanged(zoomControl, delegate
                {
                    ReciprocalZoom = 1 / zoomControl.Zoom;
                });
            }


            this.MouseWheel += MainWindow_MouseWheel;

        }


        #region Private Methods

        private void MainWindow_Drop(object sender, DragEventArgs e)
        {
            if ((this.DataContext as MainWindowViewModel).OpenFileDragCommand.CanExecute(null))
            {
                try
                {
                    e.Effects = DragDropEffects.None;

                    string[] fileNames =
                        e.Data.GetData(DataFormats.FileDrop, true)
                            as string[];

                    (this.DataContext as MainWindowViewModel).OpenFileDragCommand.Execute(fileNames[0]);
                }
                catch
                {
                    e.Effects = DragDropEffects.None;
                }
                finally
                {
                    // Mark the event as handled, so control's native 
                    //DragOver handler is not called.
                    e.Handled = true;
                }
            }
        }


        private void MainWindow_DragOver(object sender, DragEventArgs e)
        {

            if ((this.DataContext as MainWindowViewModel).OpenFileDragCommand.CanExecute(null))
            {
                try
                {
                    e.Effects = DragDropEffects.None;

                    string[] fileNames =
                        e.Data.GetData(DataFormats.FileDrop, true)
                            as string[];


                    if (fileNames.Count() != 1)
                    {
                        messageBoxService.ShowError("Can only deal with 1 file at a time");
                        return;
                    }

                    //is it a directory, get the files and check them
                    if (dragAndDropHelper.CheckFile(fileNames[0]))
                    {
                        e.Effects = DragDropEffects.Copy;
                    }
                    else
                    {
                        e.Effects = DragDropEffects.None;
                    }
                }
                catch
                {
                    e.Effects = DragDropEffects.None;
                }
                finally
                {
                    // Mark the event as handled, so control's native 
                    //DragOver handler is not called.
                    e.Handled = true;
                }
            }
        }


        private void MainWindow_MouseWheel(object sender, MouseWheelEventArgs e)
        {
           
            double incrementValue = 0.25;
            if (Zoom <= 0.3)
                incrementValue = 0.01;

            if (e.Delta > 0)
            {
                if (Zoom + incrementValue < 100)
                    Zoom += incrementValue;
            }
            else
            {
                if (Zoom - incrementValue > 0.01)
                    Zoom -= incrementValue;
            }
        }
        #endregion


        #region IGraphPrintableWindow Members

        public FrameworkElement GetGraphToPrint
        {
            get 
            {
                return graphLayout;
            }
        }

        public double Zoom
        {
            get
            {
                return zoomControl.Zoom;
            }
            set
            {
                zoomControl.Zoom = value;
            }
        }

        public void ZoomToFit()
        {
            zoomControl.ZoomToFill();
        }

        #endregion

        #region ReciprocalZoom

        public static readonly DependencyProperty ReciprocalZoomProperty =
            DependencyProperty.Register("ReciprocalZoom", typeof(Double), typeof(MainWindow),
                new FrameworkPropertyMetadata((Double)1.0));


        public Double ReciprocalZoom
        {
            get { return (Double)GetValue(ReciprocalZoomProperty); }
            set { SetValue(ReciprocalZoomProperty, value); }
        }

        #endregion

        

     
    }
}
