using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using Cinch;
using System.Reflection;
using MEFedMVVM.ViewModelLocator;
using System.ComponentModel.Composition.Hosting;
using System.IO;

namespace AutoDiagrammer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Initialisation
        /// <summary>
        /// Initiliase Cinch using the CinchBootStrapper. 
        /// </summary>
        public App()
        {
            //CinchBootStrapper.Initialise(new List<Assembly> { typeof(App).Assembly });
            //Assembly assembly = Assembly.GetExecutingAssembly();
            ViewResolver.ResolveViewLookups(new List<Assembly> { typeof(App).Assembly });
            //ViewResolver.ResolveViewLookups(new List<Assembly> { assembly });
            //PopupResolver.ResolvePopupLookups(new List<Assembly> { typeof(App).Assembly });
            //PopupResolver.ResolvePopupLookups(new List<Assembly> { assembly });
            
            //foreach (Assembly ass in new List<Assembly> { typeof(App).Assembly })
            //{
            //    foreach (Type type in ass.GetTypes())
            //    {
            //        foreach (var attrib in type.GetCustomAttributes(typeof(PopupNameToViewLookupKeyMetadataAttribute), true))
            //        {
            //            using (var container = CreateContainer())
            //            {
            //                var export = container.GetExport<IUIVisualizerService>();
            //                if (export != null)
            //                {
            //                    IUIVisualizerService uiVisualizerService = export.Value;
            //                    PopupNameToViewLookupKeyMetadataAttribute viewMetadataAtt = (PopupNameToViewLookupKeyMetadataAttribute)attrib;
            //                    uiVisualizerService.Register(viewMetadataAtt.PopupName, viewMetadataAtt.ViewLookupKey);
            //                }
            //                else
            //                {
            //                    Console.WriteLine("(export != null)");
            //                }
            //            }
            //            //PopupNameToViewLookupKeyMetadataAttribute viewMetadataAtt = (PopupNameToViewLookupKeyMetadataAttribute)attrib;
            //            ////ViewModelRepository.RegisterMissingViewModel(
            //            ////ViewModelRepository.RegisterMissingViewModel(ViewResolver.CreateView());
            //            ////ViewModelBase.ServiceProvider.RegisterService<IUIVisualizerService>(new UIVisualizerService());
            //            ////ViewModelRepository.Instance.Resolver.Container.GetExport<IUIVisualizerService>().Value.Register("AddImageRatingPopup",typeof(AddImageRatingPopup));
            //            //IUIVisualizerService uiVisualizerService =
            //            //    //WPFUIVisualizerService
            //            //    //MainWindowViewModel.uiVisualizerService;
            //            //    //ViewModelRepository.Instance.Resolver.Container.GetExport<IUIVisualizerService>().Value;
            //            ////ViewModelRepository.Instance.Resolver.Container.GetExport<WPFUIVisualizerService>().Value;
            //            ////ViewModelRepository.Instance.Resolver.Container.GetExport<MainWindowViewModel>().Value;
            //            ////MainWindowViewModel;

            //            //uiVisualizerService.Register(viewMetadataAtt.PopupName, viewMetadataAtt.ViewLookupKey);

            //        }
            //    }
            //}
            //IChildWindowService childWindowService =  ViewModelRepository.Instance.Resolver.Container.GetExport<IChildWindowService>().Value;
            //childWindowService.Register("someChildWindowName", Type of Childwindow);

            //AboutWindowPopup childWindowService = ViewModelRepository.Instance.Resolver.Container.GetExport<AboutWindowPopup>().Value;
            //childWindowService.Register("AboutWindowPopup", Type of AboutWindowPopup);
            //IUIVisualizerService uiVisualizerService =
            //IUIVisualizerService popupShower = ViewModelBase.ServiceProvider.Resolve<IUIVisualizerService>();
            //using (var container = CreateContainer())
            //{
            //    var export = container.GetExport<IUIVisualizerService>();
            //    if (export != null) 
            //    {
            //        IUIVisualizerService uiVisualizerService = export.Value;
            //        PopupNameToViewLookupKeyMetadataAttribute viewMetadataAtt = (PopupNameToViewLookupKeyMetadataAttribute)attrib;
            //        uiVisualizerService.Register(viewMetadataAtt.PopupName, viewMetadataAtt.ViewLookupKey);
            //    }
            //    else
            //    {
            //        Console.WriteLine("(export != null)");
            //    }
            //}


            //IUIVisualizerService value = ViewModelRepository.Instance.Resolver.Container.GetExport<IUIVisualizerService>().Value;
            IUIVisualizerService value = new WPFUIVisualizerService();
            //foreach (Assembly current in new List<Assembly> { typeof(App).Assembly })
            //{
                //Type[] types = GetLoadableTypes(typeof(App).Assembly);//typeof(App).Assembly.GetTypes();
                //for (int i = 0; i < types.Length; i++)

            //Assembly assembly = Assembly.GetExecutingAssembly();
            Assembly assembly = typeof(MainWindowViewModel).Assembly;
            Console.WriteLine(assembly.FullName);
            Console.WriteLine(assembly.CodeBase);
            Console.WriteLine(assembly.Location);
            Console.WriteLine(assembly.GetName());


            //Console.WriteLine(assembly.GetName());
            //foreach (Type type in GetLoadableTypes(assembly))
            //{
            //    //Type type = type;
            //    object[] customAttributes = type.GetCustomAttributes(typeof(PopupNameToViewLookupKeyMetadataAttribute), true);
            //    for (int j = 0; j < customAttributes.Length; j++)
            //    {
            //        object obj = customAttributes[j];
            //        PopupNameToViewLookupKeyMetadataAttribute popupNameToViewLookupKeyMetadataAttribute = (PopupNameToViewLookupKeyMetadataAttribute)obj;
            //        //IUIVisualizerService value = ViewModelRepository.Instance.Resolver.Container.GetExport<IUIVisualizerService>().Value;
            //        value.Register(popupNameToViewLookupKeyMetadataAttribute.PopupName, popupNameToViewLookupKeyMetadataAttribute.ViewLookupKey);
            //    }
            //}

            //}

            InitializeComponent();
        }
        /*
        public static IEnumerable<Type> GetLoadableTypes( Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException("assembly");
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }

        private static CompositionContainer CreateContainer()
        {
            var path = Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", "");
            path = Path.GetDirectoryName(path);
            if (path == null) throw new ArgumentException("Unrecognised file.");

            var catalog = new DirectoryCatalog(path, "AutoDiagrammer.*.dll");
            return new CompositionContainer(catalog);
        }
        */
        #endregion
    }
}
