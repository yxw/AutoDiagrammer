﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;

namespace AutoDiagrammer
{
    public interface ITreeCreator
    {
        List<AssemblyTreeViewModel> ScanAssemblyAndCreateTree(String assemblyFileName);
    }
}
