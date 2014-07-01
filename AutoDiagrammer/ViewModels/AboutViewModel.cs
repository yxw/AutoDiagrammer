using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GraphSharp.Algorithms.OverlapRemoval;
using GraphSharp.Algorithms.Layout;
using GraphSharp.Algorithms.Layout.Simple.FDP;
using GraphSharp.Algorithms.Layout.Compound.FDP;
using GraphSharp.Algorithms.Layout.Simple.Hierarchical;
using GraphSharp.Algorithms.Layout.Simple.Tree;
using System.Reflection;
using System.IO;
using System.Xml.Linq;
using Cinch;
using System.Windows.Input;
using System.ComponentModel;
using System.Diagnostics;

namespace AutoDiagrammer
{
    public class AboutViewModel : INPCBase
    {
        #region Ctor
        public AboutViewModel()
        {
            LaunchWebCommand = new SimpleCommand<Object, Object>(ExecuteLaunchWebCommand);
        }
        #endregion

        #region Public Properties


        public ICommand LaunchWebCommand { get; private set; }
        #endregion

        #region Command Handlers

        private void ExecuteLaunchWebCommand(Object parameter)
        {
            Process process = new Process();
            process.StartInfo = new ProcessStartInfo("http://sachabarber.net");
            process.Start();
        }

        #endregion

    }



}
