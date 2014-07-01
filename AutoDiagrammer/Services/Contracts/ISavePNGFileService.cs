using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;

namespace AutoDiagrammer
{
    public interface ISavePNGFileService
    {
        bool Save(string filePath, FrameworkElement visual);
    }
}
