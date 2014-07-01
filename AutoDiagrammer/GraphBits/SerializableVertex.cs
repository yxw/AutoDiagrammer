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
    /// A simple Serializable vertex.
    /// </summary>
    [Serializable]
    public class SerializableVertex
    {
        #region Ctor
        public SerializableVertex
            (
                String name,
                String shortName,
                List<String> constructors,
                List<String> fields,
                List<String> properties,
                List<String> interfaces,
                List<SerializableMethodData> methods,
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
        }
        #endregion

        #region Public Properties

        public String Name { get; private set; }
        public String ShortName { get; private set; }
        public List<String> Constructors { get; private set; }
        public List<String> Fields { get; private set; }
        public List<String> Properties { get; private set; }
        public List<String> Interfaces { get; private set; }
        public List<SerializableMethodData> Methods { get; private set; }
        public List<String> Events { get; private set; }
        public List<String> Associations { get; private set; }
        public bool HasConstructors { get; private set; }
        public bool HasFields { get; private set; }
        public bool HasProperties { get; private set; }
        public bool HasInterfaces { get; private set; }
        public bool HasMethods { get; private set; }
        public bool HasEvents { get; private set; }
        #endregion

        #region Overrides
        public override string ToString()
        {
            return Name;
        }
        #endregion
    }
}
