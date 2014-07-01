using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.Win32;
using System.ComponentModel.Composition;

using MEFedMVVM.ViewModelLocator;
using System.Windows.Controls;
using System.Windows.Xps.Packaging;
using System.IO;
using System.Windows.Documents;
using SUT.PrintEngine.Utils;


namespace AutoDiagrammer
{
    /// <summary>
    /// This class implements the IPrintPNGFileService
    /// </summary>
    [PartCreationPolicy(CreationPolicy.Shared)]
    [ExportService(ServiceType.Both, typeof(IPrintPNGFileService))]
    public class PrintPNGFileService : IPrintPNGFileService
    {
        

        #region IPrintPNGFileService Members
        /// <summary>
        /// Prints the file
        /// </summary>
        /// <returns>Exception if the printing failed, otherwise null</returns>
        public Exception Print(FrameworkElement graphLayout)
        {
            try
            {
                //This is using the Awesome code from : http://www.codeproject.com/KB/printing/wpfprintengine.aspx
                //By Saraf Talukder 
                var visualSize = new Size(graphLayout.ActualWidth, graphLayout.ActualHeight);
                var printControl = PrintControlFactory.Create(visualSize, graphLayout);
                printControl.ShowPrintPreview();
                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        


        #endregion
    }
}
