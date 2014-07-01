using System;
using System.IO;
using System.Reflection;
using System.Windows;
using Cinch;

namespace AutoDiagrammer
{
    [PopupNameToViewLookupKeyMetadata("HelpPopup", typeof(HelpPopup))]
    public partial class HelpPopup : Window
    {
        public HelpPopup()
        {
            InitializeComponent();
            FileInfo assLocation = new FileInfo(Assembly.GetExecutingAssembly().Location);
            String helpFileLocation = Path.Combine(assLocation.Directory.FullName, @"HtmlHelp/AutoDiagrammerHelp.htm");
            if (File.Exists(helpFileLocation))
            {
                wb.Navigate(new Uri(helpFileLocation, UriKind.RelativeOrAbsolute));
            }
            else
            {
                throw new ApplicationException(String.Format("Can not find the file {0}\r\n\r\nThe AutoDiagrammer.exe help file 'AutoDiagrammerHelp.htm' " +
                    "and all related help file images are expected to be located in a subdirectory under {1} called 'HtmlHelp'", helpFileLocation, assLocation));
            }
        }
    }
}
