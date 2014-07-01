using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using MEFedMVVM.ViewModelLocator;
using System.Windows.Media;
using System.Windows;
using System.IO.Packaging;
using System.IO;
using System.Windows.Xps.Packaging;
using System.Windows.Xps;
using System.Windows.Media.Imaging;

namespace AutoDiagrammer
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [ExportService(ServiceType.Both, typeof(ISavePNGFileService))]
    public class SavePNGFileService : ISavePNGFileService
    {
        public bool Save(string filePath, FrameworkElement visual)
        {
            try
            {
                RenderTargetBitmap bmp = new RenderTargetBitmap(
                    (int)visual.ActualWidth, (int)visual.ActualHeight, 96, 96, PixelFormats.Pbgra32);
                bmp.Render(visual);
                PngBitmapEncoder png = new PngBitmapEncoder();
                png.Frames.Add(BitmapFrame.Create(bmp));

                using (Stream stm = File.Create(filePath))
                {
                    png.Save(stm);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
