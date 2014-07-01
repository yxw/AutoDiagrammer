using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.ComponentModel;
using Cinch;

namespace AutoDiagrammer
{
    /// <summary>
    /// A simple identifiable vertex.
    /// </summary>
    [Serializable]
    [DebuggerDisplay("{ToString()}")]
    public class PocVertex : INPCBase
    {
        #region Data
        private String associationToolTip;
        private bool collapseAll;
        private bool collapseConstructors;
        private bool collapseFields;
        private bool collapseProperties;
        private bool collapseMethods;
        private bool collapseEvents;

        #endregion

        #region Ctor
        public PocVertex
            (
                String name,
                String shortName,
                List<String> constructors,
                List<String> fields,
                List<String> properties,
                List<String> interfaces,
                List<MethodData> methods,
                List<String> events,
                List<String> associations,
                bool hasConstructors,
                bool hasFields,
                bool hasProperties,
                bool hasInterfaces,
                bool hasMethods,
                bool hasEvents
            )
        {
            this.Name=name;
            this.ShortName=shortName;
            this.Constructors=constructors;
            this.Fields=fields;
            this.Properties=properties;
            this.Interfaces=interfaces;
            this.Methods=methods;
            this.Events=events;
            this.Associations=associations;
            this.HasConstructors = hasConstructors;
            this.HasFields = hasFields;
            this.HasProperties = hasProperties;
            this.HasInterfaces = hasInterfaces;
            this.HasMethods = hasMethods;
            this.HasEvents = hasEvents;

            StringBuilder sbAssociations = new StringBuilder();
            foreach (String ass in Associations)
            {
                if(ass != "")
                    sbAssociations.AppendLine(String.Format("- {0}", ass));
            }
            associationToolTip = sbAssociations.ToString();
        }
        #endregion

        #region Public Properties

        public String Name { get; private set; }
        public String ShortName { get; private set; }
        public List<String> Constructors { get; private set; }
        public List<String> Fields { get; private set; }
        public List<String> Properties { get; private set; }
        public List<String> Interfaces { get; private set; }
        public List<MethodData> Methods { get; private set; }
        public List<String> Events { get; private set; }
        public List<String> Associations { get; private set; }
        public bool HasConstructors { get; private set; }
        public bool HasFields { get; private set; }
        public bool HasProperties { get; private set; }
        public bool HasInterfaces { get; private set; }
        public bool HasMethods { get; private set; }
        public bool HasEvents { get; private set; }
        public int NumberOfEdgesFromThisVertex { get; set; }
        public int NumberOfEdgesToThisVertex { get; set; }

        public String AssociationToolTip
        {
            get { return associationToolTip; }
        }


        /// <summary>
        /// CollapseEvents
        /// </summary>
        static PropertyChangedEventArgs collapseEventsArgs =
            ObservableHelper.CreateArgs<PocVertex>(x => x.CollapseEvents);

        public bool CollapseEvents
        {
            get { return collapseEvents; }
            set
            {
                collapseEvents = value;
                NotifyPropertyChanged(collapseEventsArgs);
            }
        }


        /// <summary>
        /// CollapseMethods
        /// </summary>
        static PropertyChangedEventArgs collapseMethodsArgs =
            ObservableHelper.CreateArgs<PocVertex>(x => x.CollapseMethods);

        public bool CollapseMethods
        {
            get { return collapseMethods; }
            set
            {
                collapseMethods = value;
                NotifyPropertyChanged(collapseMethodsArgs);
            }
        }


        /// <summary>
        /// CollapseProperties
        /// </summary>
        static PropertyChangedEventArgs collapsePropertiesArgs =
            ObservableHelper.CreateArgs<PocVertex>(x => x.CollapseProperties);

        public bool CollapseProperties
        {
            get { return collapseProperties; }
            set
            {
                collapseProperties = value;
                NotifyPropertyChanged(collapsePropertiesArgs);
            }
        }


        /// <summary>
        /// CollapseFields
        /// </summary>
        static PropertyChangedEventArgs collapseFieldsArgs =
            ObservableHelper.CreateArgs<PocVertex>(x => x.CollapseFields);

        public bool CollapseFields
        {
            get { return collapseFields; }
            set
            {
                collapseFields = value;
                NotifyPropertyChanged(collapseFieldsArgs);
            }
        }


        /// <summary>
        /// CollapseAll
        /// </summary>
        static PropertyChangedEventArgs collapseAllArgs =
            ObservableHelper.CreateArgs<PocVertex>(x => x.CollapseAll);

        public bool CollapseAll
        {
            get { return collapseAll; }
            set
            {
                collapseAll = value;
                CollapseConstructors = collapseAll;
                CollapseFields = collapseAll;
                CollapseProperties = collapseAll;
                CollapseMethods = collapseAll;
                CollapseEvents = collapseAll;
                NotifyPropertyChanged(collapseAllArgs);
            }
        }


        /// <summary>
        /// CollapseConstructors
        /// </summary>
        static PropertyChangedEventArgs collapseConstructorsArgs =
            ObservableHelper.CreateArgs<PocVertex>(x => x.CollapseConstructors);

        public bool CollapseConstructors
        {
            get { return collapseConstructors; }
            set
            {
                collapseConstructors = value;
                NotifyPropertyChanged(collapseConstructorsArgs);
            }
        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            return Name;
        }
        #endregion
    }
}
