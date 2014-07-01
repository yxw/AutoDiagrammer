using System;
using System.Windows;
using System.Windows.Controls;
namespace AutoDiagrammer
{

    /// <summary>
    /// This interface defines a interface that will allow 
    /// a ViewModel to print a PNG file
    /// </summary>
    public interface IPrintPNGFileService
    {
        

         /// <summary>
        /// Prints the file
        /// </summary>
        /// <returns>True is printed sucessfully else false</returns>
        Exception Print(FrameworkElement graphLayout);
    }
    
}
