using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.IO;

namespace AutoDiagrammer
{
    /// <summary>
    /// File types for Drag operation
    /// </summary>
    public enum FileType { Assembly, Exe, NotSupported }

    /// <summary>
    /// Drag and drop helper
    /// </summary>
    public class DragAndDropHelper
    {

        /// <summary>Returns the FileType </summary>
        /// <param name="fileName">Path of a file.</param>
        public FileType GetFileType(string fileName)
        {
            string extension = System.IO.Path.GetExtension(fileName).ToLower();

            if (extension == ".dll")
                return FileType.Assembly;
            if (extension == ".exe")
                return FileType.Exe;

            return FileType.NotSupported;
        }




        /// <summary>
        /// Checks that the files being dragged are valid
        /// </summary>
        public bool CheckFile(string file)
        {
            FileType type = GetFileType(file);
            return type != FileType.NotSupported;

        }

    }
}
