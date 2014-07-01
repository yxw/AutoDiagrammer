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
using System.Globalization;

namespace AutoDiagrammer
{
    public enum AccessModifierTypes : int
    {
        [Description(@"All")]
        All = 1,

        [Description(@"Public")]
        Public = 2,

        [Description(@"Public And Static")]
        PublicAndStatic = 3
    }



    public class SettingsViewModel : ValidatingViewModelBase
    {
        #region Data
        private IOverlapRemovalParameters overlapRemovalParameters = new OverlapRemovalParametersEx();
        private Dictionary<String, ILayoutParameters> availableLayoutParameters = new Dictionary<String, ILayoutParameters>();
        private static readonly Lazy<SettingsViewModel> instance = new Lazy<SettingsViewModel>(() => new SettingsViewModel());
        private List<String> layoutAlgorithmTypes = new List<string>();
        private const string xmlFileName = "Settings.xml";
        private PocGraphLayout graphLayout;
        private string layoutAlgorithmType;
        private ILayoutParameters layoutParameters = null;
        private string xmlFileLocation;
        private bool showInterfaces = true;
        private bool showConstructorParameters = true;
        private bool showFieldTypes = true;
        private bool showPropertyTypes = true;
        private bool showEvents = true;
        private bool showEnums = true;
        private bool parseMethodBodyIL = false;
        private bool showMethodArguments = true;
        private bool showMethodReturnValues = true;
        private bool showGetMethodForProperty = true;
        private bool showSetMethodForProperty = true;
        private AccessModifierTypes showAccessModifierType = AccessModifierTypes.All;
        private bool shouldRelayoutOnExpandCollapse = false;
        private int dllLoadingTimeOutInSeconds = 20;
        private int graphDrawingTimeOutInSeconds = 20;
        private int maximumNumberOfClassesToAllowOnDiagram = 30;
        private bool includeConstructorParametersAsAssociations = true;
        private bool includePropertyTypesAsAssociations = true;
        private bool includeFieldTypesAsAssociations = true;
        private bool includeMethodArgumentAsAssociations = true;

        private static SimpleRule graphDrawingTimeOutInSecondsRule;

        #endregion

        #region Ctor
        // private to prevent direct instantiation.
        private SettingsViewModel()
        {
            this.AddRule(graphDrawingTimeOutInSecondsRule);
        }

        static SettingsViewModel()
        {

            graphDrawingTimeOutInSecondsRule = new SimpleRule("GraphDrawingTimeOutInSeconds", "Graph drawing timeout in seconds must be > 0",
                      (Object domainObject)=>
                      {
                          SettingsViewModel obj = (SettingsViewModel)domainObject;
                          return obj.GraphDrawingTimeOutInSeconds <= 0;
                      });
        }
        #endregion

        #region Public Mehods

        public void SetGraphObject(PocGraphLayout graphLayout)
        {
            xmlFileLocation = Assembly.GetExecutingAssembly().Location;
            xmlFileLocation = Path.Combine(xmlFileLocation.Substring(0, xmlFileLocation.LastIndexOf(@"\")), xmlFileName);

            this.graphLayout = graphLayout;
            this.graphLayout.OverlapRemovalParameters = OverlapRemovalParameters;
            this.graphLayout.HighlightAlgorithmType = "Simple";

            //Add Layout Algorithm Types
            layoutAlgorithmTypes.Add("BoundedFR");
            layoutAlgorithmTypes.Add("EfficientSugiyama");
            layoutAlgorithmTypes.Add("FR");
            layoutAlgorithmTypes.Add("ISOM");
            layoutAlgorithmTypes.Add("KK");
            layoutAlgorithmTypes.Add("Tree");

            //Add available LayoutParameters 
            availableLayoutParameters.Add("BoundedFR", new BoundedFRLayoutParametersEx()); //Has existing control in GSharp
            availableLayoutParameters.Add("EfficientSugiyama", new EfficientSugiyamaLayoutParametersEx()); //Has existing control in GSharp
            availableLayoutParameters.Add("FR", new FreeFRLayoutParametersEx());
            availableLayoutParameters.Add("ISOM", new ISOMLayoutParametersEx()); //Has existing control in GSharp
            availableLayoutParameters.Add("KK", new KKLayoutParametersEx());
            availableLayoutParameters.Add("Tree", new SimpleTreeLayoutParametersEx()); //Has existing control in GSharp

            //Pick a default Layout Algorithm Type
            LayoutAlgorithmType = "EfficientSugiyama";


            //commands
            ReLayoutCommand = new SimpleCommand<Object, Object>(ExecuteReLayoutCommand);
            SaveSettingsAsXmlCommand = new SimpleCommand<Object, Object>(ExecuteSaveSettingsAsXmlCommand);
            RehydrateSettingsFromXmlCommand = new SimpleCommand<Object, Object>(ExecuteRehydrateSettingsFromXmlCommand);

        }

        #endregion

        #region Public Properties

        public ICommand ReLayoutCommand { get; private set; }
        public ICommand SaveSettingsAsXmlCommand { get; private set; }
        public ICommand RehydrateSettingsFromXmlCommand { get; private set; }



        /// <summary>
        /// Singleton instance
        /// </summary>
        public static SettingsViewModel Instance
        {
            get
            {
                return instance.Value;
            }
        }


        /// <summary>
        /// GraphLayout
        /// </summary>
        static PropertyChangedEventArgs graphLayoutArgs =
            ObservableHelper.CreateArgs<SettingsViewModel>(x => x.GraphLayout);

        public PocGraphLayout GraphLayout
        {
            get { return graphLayout; }
        }


        /// <summary>
        /// LayoutAlgorithmTypes
        /// </summary>
        public List<String> LayoutAlgorithmTypes
        {
            get { return layoutAlgorithmTypes; }
        }


        /// <summary>
        /// MaximumNumberOfClassesToAllowOnDiagram
        /// </summary>
        static PropertyChangedEventArgs maximumNumberOfClassesToAllowOnDiagramArgs =
            ObservableHelper.CreateArgs<SettingsViewModel>(x => x.MaximumNumberOfClassesToAllowOnDiagram);

        public int MaximumNumberOfClassesToAllowOnDiagram
        {
            get { return maximumNumberOfClassesToAllowOnDiagram; }
            set
            {
                maximumNumberOfClassesToAllowOnDiagram = value;
                NotifyPropertyChanged(maximumNumberOfClassesToAllowOnDiagramArgs);
            }
        }


        /// <summary>
        /// LayoutAlgorithmType
        /// </summary>
        static PropertyChangedEventArgs layoutAlgorithmTypeArgs =
            ObservableHelper.CreateArgs<SettingsViewModel>(x => x.LayoutAlgorithmType);

        public string LayoutAlgorithmType
        {
            get { return layoutAlgorithmType; }
            set
            {
                layoutAlgorithmType = value;
                graphLayout.LayoutAlgorithmType = layoutAlgorithmType;
                LayoutParameters = availableLayoutParameters[layoutAlgorithmType];
                NotifyPropertyChanged(graphLayoutArgs);
                NotifyPropertyChanged(layoutAlgorithmTypeArgs);
            }
        }


        


        /// <summary>
        /// DllLoadingTimeOutInSeconds
        /// </summary>
        static PropertyChangedEventArgs dllLoadingTimeOutInSecondsArgs =
            ObservableHelper.CreateArgs<SettingsViewModel>(x => x.DllLoadingTimeOutInSeconds);

        public int DllLoadingTimeOutInSeconds
        {
            get { return dllLoadingTimeOutInSeconds; }
            set
            {
                dllLoadingTimeOutInSeconds = value;
                NotifyPropertyChanged(dllLoadingTimeOutInSecondsArgs);
            }
        }


        /// <summary>
        /// GraphDrawingTimeOutInSeconds
        /// </summary>
        static PropertyChangedEventArgs graphDrawingTimeOutInSecondsArgs =
            ObservableHelper.CreateArgs<SettingsViewModel>(x => x.GraphDrawingTimeOutInSeconds);

        public int GraphDrawingTimeOutInSeconds
        {
            get { return graphDrawingTimeOutInSeconds; }
            set
            {
                graphDrawingTimeOutInSeconds = value;
                NotifyPropertyChanged(graphDrawingTimeOutInSecondsArgs);
            }
        }


        /// <summary>
        /// ShowInterfaces
        /// </summary>
        static PropertyChangedEventArgs showInterfacesArgs =
            ObservableHelper.CreateArgs<SettingsViewModel>(x => x.ShowInterfaces);

        public bool ShowInterfaces
        {
            get { return showInterfaces; }
            set
            {
                showInterfaces = value;
                NotifyPropertyChanged(showInterfacesArgs);
            }
        }


        /// <summary>
        /// ShowConstructorParameters
        /// </summary>
        static PropertyChangedEventArgs showConstructorParametersArgs =
            ObservableHelper.CreateArgs<SettingsViewModel>(x => x.ShowConstructorParameters);

        public bool ShowConstructorParameters
        {
            get { return showConstructorParameters; }
            set
            {
                showConstructorParameters = value;
                NotifyPropertyChanged(showConstructorParametersArgs);
            }
        }


        /// <summary>
        /// ShowFieldTypes
        /// </summary>
        static PropertyChangedEventArgs showFieldTypesArgs =
            ObservableHelper.CreateArgs<SettingsViewModel>(x => x.ShowFieldTypes);

        public bool ShowFieldTypes
        {
            get { return showFieldTypes; }
            set
            {
                showFieldTypes = value;
                NotifyPropertyChanged(showFieldTypesArgs);
            }
        }



        /// <summary>
        /// ShowPropertyTypes
        /// </summary>
        static PropertyChangedEventArgs showPropertyTypesArgs =
            ObservableHelper.CreateArgs<SettingsViewModel>(x => x.ShowPropertyTypes);

        public bool ShowPropertyTypes
        {
            get { return showPropertyTypes; }
            set
            {
                showPropertyTypes = value;
                NotifyPropertyChanged(showPropertyTypesArgs);
            }
        }



        /// <summary>
        /// ShowEvents
        /// </summary>
        static PropertyChangedEventArgs showEventsArgs =
            ObservableHelper.CreateArgs<SettingsViewModel>(x => x.ShowEvents);

        public bool ShowEvents
        {
            get { return showEvents; }
            set
            {
                showEvents = value;
                NotifyPropertyChanged(showEventsArgs);
            }
        }


        /// <summary>
        /// ShowEnums
        /// </summary>
        static PropertyChangedEventArgs showEnumsArgs =
            ObservableHelper.CreateArgs<SettingsViewModel>(x => x.ShowEnums);

        public bool ShowEnums
        {
            get { return showEnums; }
            set
            {
                showEnums = value;
                NotifyPropertyChanged(showEnumsArgs);
            }
        }


        /// <summary>
        /// ParseMethodBodyIL
        /// </summary>
        static PropertyChangedEventArgs parseMethodBodyILArgs =
            ObservableHelper.CreateArgs<SettingsViewModel>(x => x.ParseMethodBodyIL);

        public bool ParseMethodBodyIL
        {
            get { return parseMethodBodyIL; }
            set
            {
                parseMethodBodyIL = value;
                NotifyPropertyChanged(parseMethodBodyILArgs);
            }
        }



        /// <summary>
        /// ShowMethodArguments
        /// </summary>
        static PropertyChangedEventArgs showMethodArgumentsArgs =
            ObservableHelper.CreateArgs<SettingsViewModel>(x => x.ShowMethodArguments);

        public bool ShowMethodArguments
        {
            get { return showMethodArguments; }
            set
            {
                showMethodArguments = value;
                NotifyPropertyChanged(showMethodArgumentsArgs);
            }
        }



        /// <summary>
        /// ShowMethodReturnValues
        /// </summary>
        static PropertyChangedEventArgs showMethodReturnValuesArgs =
            ObservableHelper.CreateArgs<SettingsViewModel>(x => x.ShowMethodReturnValues);

        public bool ShowMethodReturnValues
        {
            get { return showMethodReturnValues; }
            set
            {
                showMethodReturnValues = value;
                NotifyPropertyChanged(showMethodReturnValuesArgs);
            }
        }



        /// <summary>
        /// ShowGetMethodForProperty
        /// </summary>
        static PropertyChangedEventArgs showGetMethodForPropertyArgs =
            ObservableHelper.CreateArgs<SettingsViewModel>(x => x.ShowGetMethodForProperty);

        public bool ShowGetMethodForProperty
        {
            get { return showGetMethodForProperty; }
            set
            {
                showGetMethodForProperty = value;
                NotifyPropertyChanged(showGetMethodForPropertyArgs);
            }
        }



        /// <summary>
        /// ShowSetMethodForProperty
        /// </summary>
        static PropertyChangedEventArgs showSetMethodForPropertyArgs =
            ObservableHelper.CreateArgs<SettingsViewModel>(x => x.ShowSetMethodForProperty);

        public bool ShowSetMethodForProperty
        {
            get { return showSetMethodForProperty; }
            set
            {
                showSetMethodForProperty = value;
                NotifyPropertyChanged(showSetMethodForPropertyArgs);
            }
        }


        /// <summary>
        /// ShowAccessModifierType
        /// </summary>
        static PropertyChangedEventArgs showAccessModifierTypeArgs =
            ObservableHelper.CreateArgs<SettingsViewModel>(x => x.ShowAccessModifierType);

        public AccessModifierTypes ShowAccessModifierType
        {
            get { return showAccessModifierType; }
            set
            {
                showAccessModifierType = value;
                NotifyPropertyChanged(showAccessModifierTypeArgs);
            }
        }


        /// <summary>
        /// Gets the user specified BindingFlags that are used to gather reflective information about
        /// inidividual Types, within the AnalyseType method of the <see cref="DrawableClass">
        /// DrawableClass</see>
        /// </summary>
        public BindingFlags RequiredBindings
        {
            get
            {
                //start with some initial BindingFlags
                BindingFlags bf = BindingFlags.Instance | BindingFlags.DeclaredOnly |
                                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
                //do a siwtch 
                switch (this.ShowAccessModifierType)
                {
                    case AccessModifierTypes.All:
                        bf = BindingFlags.Instance | BindingFlags.DeclaredOnly |
                                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
                        break;
                    case AccessModifierTypes.Public:
                        bf = BindingFlags.Instance | BindingFlags.DeclaredOnly |
                                    BindingFlags.Public;
                        break;
                    case AccessModifierTypes.PublicAndStatic:
                        bf = BindingFlags.Instance | BindingFlags.DeclaredOnly |
                                    BindingFlags.Public | BindingFlags.Static;
                        break;
                    default:
                        bf = BindingFlags.Instance | BindingFlags.DeclaredOnly |
                                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
                        break;
                }
                //return the result
                return bf;
            }
        }



        /// <summary>
        /// ShouldRelayoutOnExpandCollapse
        /// </summary>
        static PropertyChangedEventArgs shouldRelayoutOnExpandCollapseArgs =
            ObservableHelper.CreateArgs<SettingsViewModel>(x => x.ShouldRelayoutOnExpandCollapse);

        public bool ShouldRelayoutOnExpandCollapse
        {
            get { return shouldRelayoutOnExpandCollapse; }
            set
            {
                shouldRelayoutOnExpandCollapse = value;
                NotifyPropertyChanged(shouldRelayoutOnExpandCollapseArgs);
            }
        }


        /// <summary>
        /// LayoutParameters
        /// </summary>
        static PropertyChangedEventArgs layoutParametersArgs =
            ObservableHelper.CreateArgs<SettingsViewModel>(x => x.LayoutParameters);

        public ILayoutParameters LayoutParameters
        {
            get { return layoutParameters; }
            set
            {
                layoutParameters = value;
                graphLayout.LayoutParameters = layoutParameters;
                NotifyPropertyChanged(graphLayoutArgs);
                NotifyPropertyChanged(layoutParametersArgs);
            }
        }




        /// <summary>
        /// IncludeConstructorParametersAsAssociations
        /// </summary>
        static PropertyChangedEventArgs includeConstructorParametersAsAssociationsArgs =
            ObservableHelper.CreateArgs<SettingsViewModel>(x => x.IncludeConstructorParametersAsAssociations);

        public bool IncludeConstructorParametersAsAssociations
        {
            get { return includeConstructorParametersAsAssociations; }
            set
            {
                includeConstructorParametersAsAssociations = value;
                NotifyPropertyChanged(includeConstructorParametersAsAssociationsArgs);
            }
        }


        /// <summary>
        /// IncludePropertyTypesAsAssociations
        /// </summary>
        static PropertyChangedEventArgs includePropertyTypesAsAssociationsArgs =
            ObservableHelper.CreateArgs<SettingsViewModel>(x => x.IncludePropertyTypesAsAssociations);

        public bool IncludePropertyTypesAsAssociations
        {
            get { return includePropertyTypesAsAssociations; }
            set
            {
                includePropertyTypesAsAssociations = value;
                NotifyPropertyChanged(includePropertyTypesAsAssociationsArgs);
            }
        }


        /// <summary>
        /// IncludeFieldTypesAsAssociations
        /// </summary>
        static PropertyChangedEventArgs includeFieldTypesAsAssociationsArgs =
            ObservableHelper.CreateArgs<SettingsViewModel>(x => x.IncludeFieldTypesAsAssociations);

        public bool IncludeFieldTypesAsAssociations
        {
            get { return includeFieldTypesAsAssociations; }
            set
            {
                includeFieldTypesAsAssociations = value;
                NotifyPropertyChanged(includeFieldTypesAsAssociationsArgs);
            }
        }


         /// <summary>
        /// IncludeMethodArgumentAsAssociations
        /// </summary>
        static PropertyChangedEventArgs includeMethodArgumentAsAssociationsArgs =
            ObservableHelper.CreateArgs<SettingsViewModel>(x => x.IncludeMethodArgumentAsAssociations);

        public bool IncludeMethodArgumentAsAssociations
        {
            get { return includeMethodArgumentAsAssociations; }
            set
            {
                includeMethodArgumentAsAssociations = value;
                NotifyPropertyChanged(includeMethodArgumentAsAssociationsArgs);
            }
        }




        /// <summary>
        /// OverlapRemovalParameters
        /// </summary>
        public IOverlapRemovalParameters OverlapRemovalParameters
        {
            get { return overlapRemovalParameters; }
        }
        #endregion

        #region Command Handlers

        private void ExecuteReLayoutCommand(Object parameter)
        {
            GraphLayout.Relayout();
        }

        private void ExecuteSaveSettingsAsXmlCommand(Object parameter)
        {
            XElement settingsXml = new XElement("settings");
            foreach (KeyValuePair<String, ILayoutParameters> layoutKVPair in availableLayoutParameters)
            {
                if (layoutKVPair.Value is ISetting)
                {
                    settingsXml.Add((layoutKVPair.Value as ISetting).GetXmlFragement());
                }
            }
            settingsXml.Add((overlapRemovalParameters as ISetting).GetXmlFragement());

            //Add misc settings
            settingsXml.Add(new XElement("setting", new XAttribute("type", "LayoutAlgorithmType"),
                                new XElement("SelectedType", LayoutAlgorithmType)));
            settingsXml.Add(new XElement("setting", new XAttribute("type", "GeneralSettings"),
                                new XElement("ShouldRelayoutOnExpandCollapse", ShouldRelayoutOnExpandCollapse),
                                new XElement("ShowInterfaces", ShowInterfaces),
                                new XElement("ShowConstructorParameters", ShowConstructorParameters),
                                new XElement("ShowFieldTypes", ShowFieldTypes),
                                new XElement("ShowPropertyTypes", ShowPropertyTypes),
                                new XElement("ShowEvents", ShowEvents),
                                new XElement("ShowEnums", ShowEnums),
                                new XElement("ParseMethodBodyIL", ParseMethodBodyIL),
                                new XElement("ShowMethodArguments", ShowMethodArguments),
                                new XElement("ShowMethodReturnValues", ShowMethodReturnValues),
                                new XElement("ShowGetMethodForProperty", ShowGetMethodForProperty),
                                new XElement("ShowSetMethodForProperty", ShowSetMethodForProperty),
                                new XElement("ShowAccessModifierType", ShowAccessModifierType),
                                new XElement("MaximumNumberOfClassesToAllowOnDiagram", MaximumNumberOfClassesToAllowOnDiagram),
                                new XElement("DllLoadingTimeOutInSeconds", DllLoadingTimeOutInSeconds),
                                new XElement("GraphDrawingTimeOutInSeconds", GraphDrawingTimeOutInSeconds),
                                new XElement("IncludeConstructorParametersAsAssociations", IncludeConstructorParametersAsAssociations),
                                new XElement("IncludePropertyTypesAsAssociations", IncludePropertyTypesAsAssociations),
                                new XElement("IncludeFieldTypesAsAssociations", IncludeFieldTypesAsAssociations),
                                new XElement("IncludeMethodArgumentAsAssociations", IncludeMethodArgumentAsAssociations)));
            settingsXml.Save(xmlFileLocation);
        }




        private void ExecuteRehydrateSettingsFromXmlCommand(Object parameter)
        {
            if (!File.Exists(xmlFileLocation))
                return;

            XElement settingsXml = XElement.Load(xmlFileLocation);


            foreach (XElement el in settingsXml.Elements("setting"))
            {
                string typeOfSetting = el.Attribute("type").Value;
                switch (typeOfSetting)
                {
                    case "Overlap":
                        (overlapRemovalParameters as ISetting).SetFromXmlFragment(el);
                        break;
                    case "LayoutAlgorithmType":
                        LayoutAlgorithmType = el.Descendants().Where(x => x.Name.LocalName == "SelectedType").Single().Value;
                        break;
                    case "GeneralSettings":
                        ShouldRelayoutOnExpandCollapse = Boolean.Parse(el.Descendants().Where(x => x.Name.LocalName == "ShouldRelayoutOnExpandCollapse").Single().Value);
                        ShowInterfaces = Boolean.Parse(el.Descendants().Where(x => x.Name.LocalName == "ShowInterfaces").Single().Value);
                        ShowConstructorParameters = Boolean.Parse(el.Descendants().Where(x => x.Name.LocalName == "ShowConstructorParameters").Single().Value);
                        ShowFieldTypes = Boolean.Parse(el.Descendants().Where(x => x.Name.LocalName == "ShowFieldTypes").Single().Value);
                        ShowPropertyTypes = Boolean.Parse(el.Descendants().Where(x => x.Name.LocalName == "ShowPropertyTypes").Single().Value);
                        ShowEvents = Boolean.Parse(el.Descendants().Where(x => x.Name.LocalName == "ShowEvents").Single().Value);
                        ShowEnums = Boolean.Parse(el.Descendants().Where(x => x.Name.LocalName == "ShowEnums").Single().Value);
                        ParseMethodBodyIL = Boolean.Parse(el.Descendants().Where(x => x.Name.LocalName == "ParseMethodBodyIL").Single().Value);
                        ShowMethodArguments = Boolean.Parse(el.Descendants().Where(x => x.Name.LocalName == "ShowMethodArguments").Single().Value);
                        ShowMethodReturnValues = Boolean.Parse(el.Descendants().Where(x => x.Name.LocalName == "ShowMethodReturnValues").Single().Value);
                        ShowGetMethodForProperty = Boolean.Parse(el.Descendants().Where(x => x.Name.LocalName == "ShowGetMethodForProperty").Single().Value);
                        ShowSetMethodForProperty = Boolean.Parse(el.Descendants().Where(x => x.Name.LocalName == "ShowSetMethodForProperty").Single().Value);
                        ShowAccessModifierType = (AccessModifierTypes)Enum.Parse(typeof(AccessModifierTypes),
                            el.Descendants().Where(x => x.Name.LocalName == "ShowAccessModifierType").Single().Value);
                        MaximumNumberOfClassesToAllowOnDiagram = Int32.Parse(el.Descendants().Where(x => x.Name.LocalName == "MaximumNumberOfClassesToAllowOnDiagram").Single().Value, CultureInfo.InvariantCulture);
                        DllLoadingTimeOutInSeconds = Int32.Parse(el.Descendants().Where(x => x.Name.LocalName == "DllLoadingTimeOutInSeconds").Single().Value, CultureInfo.InvariantCulture);
                        GraphDrawingTimeOutInSeconds = Int32.Parse(el.Descendants().Where(x => x.Name.LocalName == "GraphDrawingTimeOutInSeconds").Single().Value, CultureInfo.InvariantCulture);
                        IncludeConstructorParametersAsAssociations = Boolean.Parse(el.Descendants().Where(x => x.Name.LocalName == "IncludeConstructorParametersAsAssociations").Single().Value);
                        IncludePropertyTypesAsAssociations = Boolean.Parse(el.Descendants().Where(x => x.Name.LocalName == "IncludePropertyTypesAsAssociations").Single().Value);
                        IncludeFieldTypesAsAssociations = Boolean.Parse(el.Descendants().Where(x => x.Name.LocalName == "IncludeFieldTypesAsAssociations").Single().Value);
                        IncludeMethodArgumentAsAssociations = Boolean.Parse(el.Descendants().Where(x => x.Name.LocalName == "IncludeMethodArgumentAsAssociations").Single().Value);
                        break;
                    default:
                        ISetting setting = (ISetting)availableLayoutParameters[typeOfSetting];
                        setting.SetFromXmlFragment(el);
                        break;
                }

            }
        }

        #endregion

        #region Overrides
        /// <summary>
        /// Is the ViewModel Valid
        /// </summary>

        static PropertyChangedEventArgs isValidChangeArgs =
            ObservableHelper.CreateArgs<SettingsViewModel>(x => x.IsValid);

        public override bool IsValid
        {
            get
            {
                return base.IsValid;
            }

        }
        #endregion
    }
}
