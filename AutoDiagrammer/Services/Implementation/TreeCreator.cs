using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;
using System.Threading.Tasks;
using System.Reflection;
using System.ComponentModel.Composition;
using MEFedMVVM.ViewModelLocator;
using System.Security.Policy;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace AutoDiagrammer
{
    /// <summary>
    /// This class implements the ITreeCreator
    /// </summary>
    [PartCreationPolicy(CreationPolicy.Shared)]
    [ExportService(ServiceType.Both, typeof(ITreeCreator))]
    public class TreeCreator : ITreeCreator
    {
        #region ITreeCreator Members
        public List<AssemblyTreeViewModel> ScanAssemblyAndCreateTree(String assemblyFileName)
        {
            AppDomain childDomain = BuildChildDomain(AppDomain.CurrentDomain, assemblyFileName);

            try
            {
                List<AssemblyTreeViewModel> tree = new List<AssemblyTreeViewModel>();

                Type loaderType = typeof(SeperateAppDomainAssemblyLoader);
                if (loaderType.Assembly != null)
                {
                    SeperateAppDomainAssemblyLoader loader =
                        (SeperateAppDomainAssemblyLoader)childDomain.CreateInstanceFrom(
                            loaderType.Assembly.Location, loaderType.FullName).Unwrap();
                   

                    loader.Initialise(
                        assemblyFileName,
                        SettingsViewModel.Instance.RequiredBindings,
                        SettingsViewModel.Instance.ShowConstructorParameters,
                        SettingsViewModel.Instance.ShowFieldTypes,
                        SettingsViewModel.Instance.ShowPropertyTypes,
                        SettingsViewModel.Instance.ShowInterfaces,
                        SettingsViewModel.Instance.ParseMethodBodyIL,
                        SettingsViewModel.Instance.ShowMethodArguments,
                        SettingsViewModel.Instance.ShowMethodReturnValues,
                        SettingsViewModel.Instance.ShowGetMethodForProperty,
                        SettingsViewModel.Instance.ShowSetMethodForProperty,
                        SettingsViewModel.Instance.ShowEvents,
                        SettingsViewModel.Instance.IncludeConstructorParametersAsAssociations,
                        SettingsViewModel.Instance.IncludePropertyTypesAsAssociations,
                        SettingsViewModel.Instance.IncludeFieldTypesAsAssociations,
                        SettingsViewModel.Instance.IncludeMethodArgumentAsAssociations);
                    tree = loader.ScanAssemblyAndCreateTree();
                }

                return tree;
            }
            catch (AggregateException aggEx)
            {
                throw new InvalidOperationException(
                    string.Format("Could not load namespaces for the assembly file : {0}\r\n\r\n{1}",
                    assemblyFileName,
                    aggEx.InnerException.Message));
            }
            finally
            {
                AppDomain.Unload(childDomain);
            }
        }
        #endregion

        #region Private Methods
        private AppDomain BuildChildDomain(AppDomain parentDomain, string fileName)
        {
            Evidence evidence = new Evidence(parentDomain.Evidence);
            AppDomainSetup setup = parentDomain.SetupInformation;
            FileInfo fi = new FileInfo(fileName);
            AppDomain newAppDomain = AppDomain.CreateDomain("DiscoveryRegion", evidence, setup);


            return newAppDomain;
        }
        #endregion
    }



    public class SeperateAppDomainAssemblyLoader : MarshalByRefObject
    {
        #region Data
        private String assemblyFileName;
        private Assembly assembly;

        private static BindingFlags requiredBindings;
        private static bool showConstructorParameters;
        private static bool showFieldTypes;
        private static bool showPropertyTypes;
        private static bool showInterfaces;
        private static bool parseMethodBodyIL;
        private static bool showMethodArguments;
        private static bool showMethodReturnValues;
        private static bool showGetMethodForProperty;
        private static bool showSetMethodForProperty;
        private static bool showEvents;
        private static bool includeConstructorParametersAsAssociations;
        private static bool includePropertyTypesAsAssociations;
        private static bool includeFieldTypesAsAssociations;
        private static bool includeMethodArgumentAsAssociations;
        #endregion

        #region Public Methods
        public void Initialise(
            String assemblyFileName,
            BindingFlags requiredBindings,
            bool showConstructorParameters,
            bool showFieldTypes,
            bool showPropertyTypes,
            bool showInterfaces,
            bool parseMethodBodyIL,
            bool showMethodArguments,
            bool showMethodReturnValues,
            bool showGetMethodForProperty,
            bool showSetMethodForProperty,
            bool showEvents,
            bool includeConstructorParametersAsAssociations,
            bool includePropertyTypesAsAssociations,
            bool includeFieldTypesAsAssociations,
            bool includeMethodArgumentAsAssociations)
        {
            this.assemblyFileName = assemblyFileName;
            SeperateAppDomainAssemblyLoader.requiredBindings = requiredBindings;
            SeperateAppDomainAssemblyLoader.showConstructorParameters = showConstructorParameters;
            SeperateAppDomainAssemblyLoader.showFieldTypes = showFieldTypes;
            SeperateAppDomainAssemblyLoader.showPropertyTypes = showPropertyTypes;
            SeperateAppDomainAssemblyLoader.showInterfaces = showInterfaces;
            SeperateAppDomainAssemblyLoader.parseMethodBodyIL = parseMethodBodyIL;
            SeperateAppDomainAssemblyLoader.showMethodArguments = showMethodArguments;
            SeperateAppDomainAssemblyLoader.showMethodReturnValues = showMethodReturnValues;
            SeperateAppDomainAssemblyLoader.showGetMethodForProperty = showGetMethodForProperty;
            SeperateAppDomainAssemblyLoader.showSetMethodForProperty = showSetMethodForProperty;
            SeperateAppDomainAssemblyLoader.showEvents = showEvents;
            SeperateAppDomainAssemblyLoader.includeConstructorParametersAsAssociations = includeConstructorParametersAsAssociations;
            SeperateAppDomainAssemblyLoader.includePropertyTypesAsAssociations = includePropertyTypesAsAssociations;
            SeperateAppDomainAssemblyLoader.includeFieldTypesAsAssociations = includeFieldTypesAsAssociations;
            SeperateAppDomainAssemblyLoader.includeMethodArgumentAsAssociations = includeMethodArgumentAsAssociations;

            assembly = Assembly.LoadFrom(assemblyFileName);
        }
        #endregion

        #region Private/Internal Methods
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        internal List<AssemblyTreeViewModel> ScanAssemblyAndCreateTree()
        {
            AppDomain curDomain = AppDomain.CurrentDomain;

            try
            {
                AppDomain.CurrentDomain.AssemblyResolve += ReflectionOnlyResolveEventHandler;
                List<AssemblyTreeViewModel> tree = GroupAndCreateTree(assemblyFileName);
                return tree;
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyResolve -= ReflectionOnlyResolveEventHandler;
            }
        }



        private Assembly ReflectionOnlyResolveEventHandler(object sender, ResolveEventArgs args)
        {
            DirectoryInfo directory = new DirectoryInfo(assemblyFileName);

            Assembly loadedAssembly =
                AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(asm => string.Equals(asm.FullName, args.Name, 
                        StringComparison.OrdinalIgnoreCase));

            if (loadedAssembly != null)
            {
                return loadedAssembly;
            }

            AssemblyName assemblyName = new AssemblyName(args.Name);
            string dependentAssemblyFilename = Path.Combine(
                directory.FullName, assemblyName.Name + ".dll");

            if (File.Exists(dependentAssemblyFilename))
            {
                return Assembly.LoadFrom(dependentAssemblyFilename);
            }
            return Assembly.Load(args.Name);

        }


        private List<AssemblyTreeViewModel> GroupAndCreateTree(String assemblyFileName)
        {


            AssemblyTreeViewModel root = null;
            List<AssemblyTreeViewModel> tree = new List<AssemblyTreeViewModel>();

            var groupedTypes = from t in assembly.GetTypes()
                                where DotNetObject.IsWantedForDiagramType(t)
                                group t by t.Namespace into g
                                select new { NameSpace = g.Key, Types = g };

            foreach (var g in groupedTypes)
            {
                if (g.NameSpace != null)
                {

                    AssemblyTreeViewModel sub = null;
                    AssemblyTreeViewModel parentToAddTo = null;

                    if (tree.Count == 0)
                    {
                        root = new AssemblyTreeViewModel(RepresentationType.AssemblyOrExe,
                            String.Format("Assembly : {0}", assembly.GetName().Name), null, null);
                        tree.Add(root);
                        //Add the types
                        AddTypes(g.Types, root);
                    }
                    else
                    {
                        string trimmedNamespace = g.NameSpace;
                        if (g.NameSpace.Contains("."))
                            trimmedNamespace = g.NameSpace.Substring(0, g.NameSpace.LastIndexOf("."));

                        if (g.NameSpace.Equals(String.Empty))
                            parentToAddTo = root;
                        else
                            parentToAddTo = FindCorrectTreeNodeToAddTo(root, trimmedNamespace);

                        if (parentToAddTo == null)
                            parentToAddTo = root;

                        sub = new AssemblyTreeViewModel(
                            RepresentationType.Namespace, g.NameSpace, null, parentToAddTo);

                        parentToAddTo.Children.Add(sub);
                        //add the types
                        AddTypes(g.Types, sub);
                    }
                }
            }

            return tree;
        }

        private AssemblyTreeViewModel FindCorrectTreeNodeToAddTo(
            AssemblyTreeViewModel node, String @namespace)
        {
            var results = node.Children.Where(x => x.Name == @namespace);

            if (results.Count() > 0)
                return results.First();


            foreach (AssemblyTreeViewModel child in node.Children)
            {
                AssemblyTreeViewModel assemblyTreeViewModel = 
                    FindCorrectTreeNodeToAddTo(child, @namespace);

                if (assemblyTreeViewModel != null)
                    return assemblyTreeViewModel;
            }

            return null;
        }


        private void AddTypes(IGrouping<String, Type> types, AssemblyTreeViewModel parent)
        {
            TypeReflector.RequiredBindings = SeperateAppDomainAssemblyLoader.requiredBindings;
            TypeReflector.ShowConstructorParameters = SeperateAppDomainAssemblyLoader.showConstructorParameters;
            TypeReflector.ShowFieldTypes = SeperateAppDomainAssemblyLoader.showFieldTypes;
            TypeReflector.ShowPropertyTypes = SeperateAppDomainAssemblyLoader.showPropertyTypes;
            TypeReflector.ShowInterfaces = SeperateAppDomainAssemblyLoader.showInterfaces;
            TypeReflector.ParseMethodBodyIL = SeperateAppDomainAssemblyLoader.parseMethodBodyIL;
            TypeReflector.ShowMethodArguments = SeperateAppDomainAssemblyLoader.showMethodArguments;
            TypeReflector.ShowMethodReturnValues = SeperateAppDomainAssemblyLoader.showMethodReturnValues;
            TypeReflector.ShowGetMethodForProperty = SeperateAppDomainAssemblyLoader.showGetMethodForProperty;
            TypeReflector.ShowSetMethodForProperty = SeperateAppDomainAssemblyLoader.showSetMethodForProperty;
            TypeReflector.ShowEvents = SeperateAppDomainAssemblyLoader.showEvents;
            TypeReflector.IncludeConstructorParametersAsAssociations = SeperateAppDomainAssemblyLoader.includeConstructorParametersAsAssociations;
            TypeReflector.IncludePropertyTypesAsAssociations = SeperateAppDomainAssemblyLoader.includePropertyTypesAsAssociations;
            TypeReflector.IncludeFieldTypesAsAssociations = SeperateAppDomainAssemblyLoader.includeFieldTypesAsAssociations;
            TypeReflector.IncludeMethodArgumentAsAssociations = SeperateAppDomainAssemblyLoader.includeMethodArgumentAsAssociations;


            //Load ILReaader Globals
            MethodBodyReader.LoadOpCodes();



            foreach (var t in types)
            {
                TypeReflector typeReflector = new TypeReflector(t);
                typeReflector.ReflectOnType();


                SerializableVertex vertex = new SerializableVertex(
                    typeReflector.Name,
                    typeReflector.ShortName,
                    typeReflector.Constructors,
                    typeReflector.Fields,
                    typeReflector.Properties,
                    typeReflector.Interfaces,
                    typeReflector.Methods,
                    typeReflector.Events,
                    new List<string>(typeReflector.Associations),
                    typeReflector.HasConstructors,
                    typeReflector.HasFields,
                    typeReflector.HasProperties,
                    typeReflector.HasInterfaces,
                    typeReflector.HasMethods,
                    typeReflector.HasEvents);

                AssemblyTreeViewModel newNode = 
                    new AssemblyTreeViewModel(RepresentationType.Class, t.Name, vertex, parent);
                parent.Children.Add(newNode);
            }
        }

        #endregion
    }
}
