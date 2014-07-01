using QuickGraph;
using System.Diagnostics;
using System.ComponentModel;
using System;

namespace AutoDiagrammer
{
    /// <summary>
    /// A simple identifiable edge.
    /// </summary>
    public class PocEdge : Edge<PocVertex>, INotifyPropertyChanged
    {
        #region Data
        private string id;
        #endregion

        #region Ctor
        public PocEdge(string id, PocVertex source, PocVertex target)
            : base(source, target)
        {
            ID = id;
        }
        #endregion

        #region Public Properties
        public string ID
        {
            get { return id; }
            set
            {
                id = value;
                NotifyPropertyChanged("ID");
            }
        }
        #endregion

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #endregion
    }
}