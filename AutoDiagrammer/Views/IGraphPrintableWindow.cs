using System.Windows;

namespace AutoDiagrammer
{
    public interface IGraphPrintableWindow
    {
        FrameworkElement GetGraphToPrint { get; }
        double Zoom { get; set; }
        void ZoomToFit();
    }
}
