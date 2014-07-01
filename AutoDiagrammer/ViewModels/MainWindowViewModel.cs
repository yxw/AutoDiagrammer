using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using Cinch;
using GraphSharp.Controls;
using MEFedMVVM.ViewModelLocator;
using SUT.PrintEngine.Utils;


namespace AutoDiagrammer
{
    public class PocGraphLayout : GraphLayout<PocVertex, PocEdge, PocGraph> 
    {

    }

    [ExportViewModel("MainWindowViewModel")]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class MainWindowViewModel : ViewModelBase
    {
        #region Data

        private PocGraphLayout graphLayout = new PocGraphLayout();
        private PocGraph graph;
        private IViewAwareStatus viewAwareStatusService;
        private IMessageBoxService messageBoxService;
        private IUIVisualizerService uiVisualizerService;
        private IOpenFileService openFileService;
        private ISaveFileService saveFileService;
        private ISavePNGFileService savePNGService;
        private IPrintPNGFileService printPNGFileService;
        private IGraphPrintableWindow graphPrintableWindow;
        private IAssemblyManipulationService assemblyManipulationService;
        private List<AssemblyTreeViewModel> treeValues = new List<AssemblyTreeViewModel>();
        private List<PocVertex> notAssociatedVertices = new List<PocVertex>();
        private PocVertex selectedNotAssociatedVertex = null;
        private bool hasNotAssociatedVertices = false;

        private bool hasActiveGraph = false;
        private bool hasActiveAssembly = false;
        private bool isGenerallyBusy = false;

        private string mainWaitText;
        private string mainErrorMessage;
        private AsyncType mainAsyncState = AsyncType.Content;

        private string drawerWaitText;
        private string drawerErrorMessage;
        private AsyncType drawerAsyncState = AsyncType.Content;

        #endregion

        #region Ctor
        [ImportingConstructor]
        public MainWindowViewModel(
            IViewAwareStatus viewAwareStatusService, 
            IMessageBoxService messageBoxService,
            IUIVisualizerService uiVisualizerService,
            IOpenFileService openFileService,
            ISaveFileService saveFileService,
            ISavePNGFileService savePNGService,
            IPrintPNGFileService printPNGFileService,
            IAssemblyManipulationService assemblyManipulationService
            )
        {
            

            SettingsViewModel.Instance.SetGraphObject(graphLayout);
            this.assemblyManipulationService = assemblyManipulationService;


            this.viewAwareStatusService = viewAwareStatusService;
            this.viewAwareStatusService.ViewLoaded += ViewAwareStatusService_ViewLoaded;
            this.messageBoxService = messageBoxService;
            this.uiVisualizerService = uiVisualizerService;
            this.openFileService = openFileService;
            this.saveFileService = saveFileService;
            this.savePNGService = savePNGService;
            this.printPNGFileService = printPNGFileService;

            
            //Commands
            SaveSettingsAsXmlCommand = new SimpleCommand<Object, Object>(ExecuteSaveSettingsAsXmlCommand);
            ShowSettingsWindowCommand = new SimpleCommand<Object, Object>(NotBusyCheck,ExecuteShowSettingsWindowCommand);
            OpenFileCommand = new SimpleCommand<Object, Object>(NotBusyCheck, ExecuteOpenFileCommand);
            OpenFileDragCommand = new SimpleCommand<Object, Object>(NotBusyCheck, ExecuteOpenFileDragCommand);
            CommenceDrawingCommand = new SimpleCommand<Object, Object>(CanExecuteCommenceDrawingCommand, ExecuteCommenceDrawingCommand);
            ReLayoutCommand = new SimpleCommand<Object, Object>(NotBusyCheckAndNotDrawingAndHasGraphCheck, ExecuteReLayoutCommand);
            SaveCommand = new SimpleCommand<Object, Object>(NotBusyCheckAndNotDrawingAndHasGraphCheck, ExecuteSaveCommand);
            PrintFileCommand = new SimpleCommand<Object, Object>(NotBusyCheckAndNotDrawingAndHasGraphCheck, ExecutePrintFileCommand);
            ShowSpecificNotAssociatedVertexCommand = new SimpleCommand<Object, Object>(ExecuteShowSpecificNotAssociatedVertexCommand);
            AboutCommand = new SimpleCommand<Object, Object>(NotBusyCheck, ExecuteAboutCommand);
            HelpCommand = new SimpleCommand<Object, Object>(NotBusyCheck, ExecuteHelpCommand);

            CollapseVertexRegionsCommand = new SimpleCommand<Object, Object>(NotBusyCheckAndNotDrawingAndHasGraphCheck, ExecuteCollapseVertexRegionsCommand); 

            Mediator.Instance.Register(this);
            
        }
        #endregion

        #region Mediator Message Sinks

        [MediatorMessageSink("TreeStateChangedMessage")]
        void OnTreeStateChangedMessage(bool dummy)
        {
            assemblyManipulationService.CalculateSelectedTreeValues();
        }

        #endregion

        #region Private Methods

        private void ProcessAssemblyFile(string fileName)
        {
            if (DotNetObject.IsValidDotNetAssembly(fileName))
            {

                MainAsyncState = AsyncType.Busy;
                ApplicationHelper.DoEvents();
                assemblyManipulationService.ReInitialise();

                Task task = assemblyManipulationService.LoadNameSpacesAndTypes(fileName);

                int timeout = SettingsViewModel.Instance.DllLoadingTimeOutInSeconds * 1000;
                bool finishedOk = task.Wait(timeout); // wait 20 seconds before timing out

                if (finishedOk)
                {
                    TreeValues = assemblyManipulationService.TreeValues;
                    HasActiveAssembly = true;
                    MainAsyncState = AsyncType.Content;
                }
                else
                {
                    messageBoxService.ShowError(String.Format(
                        "The loading and reflecting of the Dll/Exe took longer than {0} seconds, maybe try increase this setting and try again",
                        SettingsViewModel.Instance.DllLoadingTimeOutInSeconds));
                }
            }
            else
            {
                HasActiveAssembly = false;
                messageBoxService.ShowError(String.Format("The file {0} is not a valid .NET Assembly", fileName));
            }
        }


        private bool NotBusyCheck(object parameter)
        {
            return !(MainAsyncState == AsyncType.Busy) && !(DrawerAsyncState == AsyncType.Busy) &&
                !isGenerallyBusy;
        }

        private bool NotBusyCheckAndNotDrawingCheck(object parameter)
        {
            return !(MainAsyncState == AsyncType.Busy) && !(DrawerAsyncState == AsyncType.Busy) &&
                HasActiveAssembly;
        }

       

        private bool NotBusyCheckAndNotDrawingAndHasGraphCheck(object parameter)
        {
            return !(MainAsyncState == AsyncType.Busy) && !(DrawerAsyncState == AsyncType.Busy) &&
                HasActiveAssembly && hasActiveGraph;
        }
 
        private void ViewAwareStatusService_ViewLoaded()
        {
            SettingsViewModel.Instance.RehydrateSettingsFromXmlCommand.Execute(null);

            IGraphPrintableWindow graphPrintableWin = 
                this.viewAwareStatusService.View as IGraphPrintableWindow;
            if (graphPrintableWin != null)
            {
                graphPrintableWindow = graphPrintableWin;
            }

        }

        private void AddItemsToGraph(GraphResults graphResults)
        {

            NotAssociatedVertices = graphResults.Vertices
                                    .Where(v => v.NumberOfEdgesFromThisVertex == 0 &&
                                                v.NumberOfEdgesToThisVertex == 0)
                                    .OrderBy(x => x.Name).ToList();
            HasNotAssociatedVertices = NotAssociatedVertices.Any();


            graph = new PocGraph(true);
            graphLayout.Graph = graph;

            graph.Clear();

            List<PocVertex> vertices = graphResults.Vertices
                .Where(v => v.NumberOfEdgesFromThisVertex > 0 ||
                            v.NumberOfEdgesToThisVertex > 0).ToList();

            foreach (PocVertex vertex in vertices)
            {
                if (vertex != null)
                    graph.AddVertex(vertex);
            }

            foreach (PocEdge edge in graphResults.Edges)
            {
                if(edge != null)
                    graph.AddEdge(edge);
            }

            NotifyPropertyChanged(graphLayoutArgs);
        }

        #endregion

        #region Public Properties

        public ICommand ShowSettingsWindowCommand { get; private set; }
        public ICommand SaveSettingsAsXmlCommand { get; private set; }
        public ICommand ReLayoutCommand { get; private set; }
        public ICommand AboutCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand OpenFileCommand { get; private set; }
        public ICommand OpenFileDragCommand { get; private set; }
        public ICommand PrintFileCommand { get; private set; }
        public ICommand CommenceDrawingCommand { get; private set; }
        public ICommand CancelDrawingCommand { get; private set; }
        public ICommand ShowSpecificNotAssociatedVertexCommand { get; private set; }
        public ICommand HelpCommand { get; private set; }
        public ICommand CollapseVertexRegionsCommand { get; private set; }


        


        /// <summary>
        /// NotAssociatedVertices
        /// </summary>
        static PropertyChangedEventArgs notAssociatedVerticesArgs =
            ObservableHelper.CreateArgs<MainWindowViewModel>(x => x.NotAssociatedVertices);

        public List<PocVertex> NotAssociatedVertices
        {
            get { return notAssociatedVertices; }
            private set
            {
                notAssociatedVertices = value;
                NotifyPropertyChanged(notAssociatedVerticesArgs);
            }
        }

        /// <summary>
        /// SelectedNotAssociatedVertex
        /// </summary>
        static PropertyChangedEventArgs selectedNotAssociatedVertexArgs =
            ObservableHelper.CreateArgs<MainWindowViewModel>(x => x.SelectedNotAssociatedVertex);

        public PocVertex SelectedNotAssociatedVertex
        {
            get { return selectedNotAssociatedVertex; }
            set
            {
                selectedNotAssociatedVertex = value;
                NotifyPropertyChanged(selectedNotAssociatedVertexArgs);
            }
        }

        /// <summary>
        /// HasNotAssociatedVertices
        /// </summary>
        static PropertyChangedEventArgs hasNotAssociatedVerticesArgs =
            ObservableHelper.CreateArgs<MainWindowViewModel>(x => x.HasNotAssociatedVertices);

        public bool HasNotAssociatedVertices
        {
            get { return hasNotAssociatedVertices; }
            private set
            {
                hasNotAssociatedVertices = value;
                NotifyPropertyChanged(hasNotAssociatedVerticesArgs);
            }
        }






        
        /// <summary>
        /// TreeValues
        /// </summary>
        static PropertyChangedEventArgs treeValuesArgs =
            ObservableHelper.CreateArgs<MainWindowViewModel>(x => x.TreeValues);

        public List<AssemblyTreeViewModel> TreeValues
        {
            get { return treeValues; }
            private set
            {
                treeValues = value;
                NotifyPropertyChanged(treeValuesArgs);
            }
        }



        /// <summary>
        /// HasActiveAssembly
        /// </summary>
        static PropertyChangedEventArgs hasActiveAssemblyArgs =
            ObservableHelper.CreateArgs<MainWindowViewModel>(x => x.HasActiveAssembly);

        public bool HasActiveAssembly
        {
            get { return hasActiveAssembly; }
            private set
            {
                hasActiveAssembly = value;
                NotifyPropertyChanged(hasActiveAssemblyArgs);
            }
        }


        /// <summary>
        /// MainAsyncState
        /// </summary>
        static PropertyChangedEventArgs mainAsyncStateArgs =
            ObservableHelper.CreateArgs<MainWindowViewModel>(x => x.MainAsyncState);

        public AsyncType MainAsyncState
        {
            get { return mainAsyncState; }
            private set
            {
                mainAsyncState = value;
                NotifyPropertyChanged(mainAsyncStateArgs);
            }
        }



        /// <summary>
        /// MainWaitText
        /// </summary>
        static PropertyChangedEventArgs mainWaitTextArgs =
            ObservableHelper.CreateArgs<MainWindowViewModel>(x => x.MainWaitText);


        public string MainWaitText
        {
            get { return mainWaitText; }
            private set
            {
                mainWaitText = value;
                NotifyPropertyChanged(mainWaitTextArgs);
            }
        }


        /// <summary>
        /// MainErrorMessage
        /// </summary>
        static PropertyChangedEventArgs mainErrorMessageArgs =
            ObservableHelper.CreateArgs<MainWindowViewModel>(x => x.MainErrorMessage);


        public string MainErrorMessage
        {
            get { return mainErrorMessage; }
            private set
            {
                mainErrorMessage = value;
                NotifyPropertyChanged(mainErrorMessageArgs);
            }
        }


        /// <summary>
        /// DrawerAsyncState
        /// </summary>
        static PropertyChangedEventArgs drawerAsyncStateArgs =
            ObservableHelper.CreateArgs<MainWindowViewModel>(x => x.DrawerAsyncState);

        public AsyncType DrawerAsyncState
        {
            get { return drawerAsyncState; }
            private set
            {
                drawerAsyncState = value;
                NotifyPropertyChanged(drawerAsyncStateArgs);
            }
        }



        /// <summary>
        /// DrawerWaitText
        /// </summary>
        static PropertyChangedEventArgs drawerWaitTextArgs =
            ObservableHelper.CreateArgs<MainWindowViewModel>(x => x.DrawerWaitText);


        public string DrawerWaitText
        {
            get { return drawerWaitText; }
            private set
            {
                drawerWaitText = value;
                NotifyPropertyChanged(drawerWaitTextArgs);
            }
        }


        /// <summary>
        /// DrawerErrorMessage
        /// </summary>
        static PropertyChangedEventArgs drawerErrorMessageArgs =
            ObservableHelper.CreateArgs<MainWindowViewModel>(x => x.DrawerErrorMessage);


        public string DrawerErrorMessage
        {
            get { return drawerErrorMessage; }
            private set
            {
                drawerErrorMessage = value;
                NotifyPropertyChanged(drawerErrorMessageArgs);
            }
        }


        /// <summary>
        /// GraphLayout
        /// </summary>
        static PropertyChangedEventArgs graphLayoutArgs =
            ObservableHelper.CreateArgs<MainWindowViewModel>(x => x.GraphLayout);
        


        public PocGraphLayout GraphLayout
        {
            get { return graphLayout; }
        }
        #endregion

        #region Command Handlers
        private void ExecuteHelpCommand(Object parameter)
        {
            try
            {
                uiVisualizerService.ShowDialog("HelpPopup", null);
            }
            catch(Exception ex)
            {
                messageBoxService.ShowError(ex.InnerException.Message);
            }
        }


        #region CollapseVertexRegionsCommand
        private void ExecuteCollapseVertexRegionsCommand(Object parameter)
        {
            isGenerallyBusy = true;
            try
            {
                switch (parameter.ToString())
                {
                    case "All":
                        foreach (PocVertex vertex in graph.Vertices)
                        {
                            vertex.CollapseAll = !vertex.CollapseAll;
                        }
                        break;
                    case "Constructors":
                        foreach (PocVertex vertex in graph.Vertices)
                        {
                            vertex.CollapseConstructors = !vertex.CollapseConstructors;
                        }
                        break;
                    case "Fields":
                        foreach (PocVertex vertex in graph.Vertices)
                        {
                            vertex.CollapseFields = !vertex.CollapseFields;
                        }
                        break;
                    case "Properties":
                        foreach (PocVertex vertex in graph.Vertices)
                        {
                            vertex.CollapseProperties = !vertex.CollapseProperties;
                        }
                        break;
                    case "Methods":
                        foreach (PocVertex vertex in graph.Vertices)
                        {
                            vertex.CollapseMethods = !vertex.CollapseMethods;
                        }
                        break;
                    case "Events":
                        foreach (PocVertex vertex in graph.Vertices)
                        {
                            vertex.CollapseEvents = !vertex.CollapseEvents;
                        }
                        break;
                }


                GraphLayout.Relayout();
            }
            finally
            {
                isGenerallyBusy = false;
            }   

        }
        #endregion

        #region ShowSpecificNotAssociatedVertexCommand
        private void ExecuteShowSpecificNotAssociatedVertexCommand(Object parameter)
        {
            uiVisualizerService.ShowDialog("NonAssociatedVertexPopup", this.SelectedNotAssociatedVertex);

        }
        #endregion

        #region CommenceDrawingCommand
        private bool CanExecuteCommenceDrawingCommand(Object parameter)
        {
            if (assemblyManipulationService == null || assemblyManipulationService.TreeValues == null)
                return false;

            if (!assemblyManipulationService.SelectedTreeValues.Any())
                return false;

            return NotBusyCheck(parameter);
        }

        private void ExecuteCommenceDrawingCommand(Object parameter)
        {
            try
            {

                if (assemblyManipulationService.SelectedTreeValues.Count > SettingsViewModel.Instance.MaximumNumberOfClassesToAllowOnDiagram)
                {
                    messageBoxService.ShowError("Can not show that many classes, it exceeds the MaximumNumberOfClassesToAllowOnDiagram setting\r\n" +
                                                "Which you may change, but be advised this may cause the diagram drawing/layout time to be unacceptable");

                    return;
                }


                DrawerAsyncState = AsyncType.Busy;
                isGenerallyBusy = true;
                hasActiveGraph = false;
                ApplicationHelper.DoEvents();

                Task<GraphResults> task =
                    assemblyManipulationService.CreateGraph();

                int timeout = SettingsViewModel.Instance.GraphDrawingTimeOutInSeconds * 1000;

                bool finishedOk = task.Wait(timeout); // wait 20 seconds before timing out

                if (finishedOk)
                {
                    AddItemsToGraph(task.Result);

                    graphPrintableWindow.ZoomToFit();

                    //TODO Need to also show the non connected ones in a ComboBox
                    //which will launch the popup
                    hasActiveGraph = true;

                }
                else
                {
                    messageBoxService.ShowError(String.Format(
                        "The generating of the class diagram took longer than {0} seconds, maybe try increase this setting and try again",
                        SettingsViewModel.Instance.GraphDrawingTimeOutInSeconds));
                }
            }
            catch (AggregateException AggEx)
            {
                messageBoxService.ShowError(AggEx.InnerException.Message);
            }
            catch (Exception ex)
            {
                messageBoxService.ShowError("Class diagram could not be created\r\n" + ex.Message);
            }
            finally
            {
                DrawerAsyncState = AsyncType.Content;
                ApplicationHelper.DoEvents();
                isGenerallyBusy = false;
                if (graph != null)
                {
                    if (graph.VertexCount == 0)
                        messageBoxService.ShowInformation("Could not find any connected classes in your chosen classes\r\n\r\n" +
                            "So please examine the class definitions using the 'Not Associated Items' drop down in the top right of this application");
                }
            }
        }
        #endregion

        #region ShowSettingsWindowCommand
        private void ExecuteShowSettingsWindowCommand(Object parameter)
        {
            isGenerallyBusy = true;
            try
            {
                bool? result = uiVisualizerService.ShowDialog("SettingsWindowPopup", SettingsViewModel.Instance);
                if (result.HasValue && result.Value)
                {
                    GraphLayout.Relayout();
                }
            }
            finally
            {
                isGenerallyBusy = false;
            }
            
        }
        #endregion

        #region SaveSettingsAsXmlCommand
        private void ExecuteSaveSettingsAsXmlCommand(Object parameter)
        {
            SettingsViewModel.Instance.SaveSettingsAsXmlCommand.Execute(parameter);
        }
        #endregion

        #region ReLayoutCommand
        private void ExecuteReLayoutCommand(Object parameter)
        {
            isGenerallyBusy = true;
            try
            {
                GraphLayout.Relayout();
            }
            finally
            {
                isGenerallyBusy = false;
            }
            
        }
        #endregion

        #region AboutCommand
        private void ExecuteAboutCommand(Object parameter)
        {
            uiVisualizerService.ShowDialog("AboutWindowPopup", new AboutViewModel());
        }
        #endregion

        #region SaveCommand
        private void ExecuteSaveCommand(Object parameter)
        {
            isGenerallyBusy = true;
            try
            {
                saveFileService.InitialDirectory = @"c:\temp";
                saveFileService.OverwritePrompt = true;
                saveFileService.Filter = "*.PNG | PNG Files";

                bool? result = saveFileService.ShowDialog(null);
                String filePath = saveFileService.FileName;

                if (!filePath.ToLower().EndsWith(".png"))
                    filePath += ".png";

                if (result.HasValue && result.Value)
                {
                    FrameworkElement visual = graphPrintableWindow.GetGraphToPrint;
                    Double currentZoom = graphPrintableWindow.Zoom;
                    graphPrintableWindow.Zoom = 1.0;

                    if (savePNGService.Save(filePath, visual))
                    {
                        messageBoxService.ShowInformation(string.Format("Sucessfully saved file to {0}", filePath));
                    }
                    else
                    {
                        messageBoxService.ShowError(string.Format("Error saving file {0}", filePath));
                    }

                    graphPrintableWindow.Zoom = currentZoom;
                }
            }
            finally
            {
                isGenerallyBusy = false;
            }
            


        }
        #endregion


        #region OpenFileCommand
        private void ExecuteOpenFileCommand(Object parameter)
        {
            isGenerallyBusy = true;
            try
            {
                openFileService.InitialDirectory = @"C:\";
                openFileService.Filter = "Assemblies (*.dll)|*.dll|Executables (*.exe)|*.exe";
                openFileService.FileName = "";

                bool? result = openFileService.ShowDialog(null);
                if (result.HasValue && result.Value)
                {
                    if (!openFileService.FileName.Equals(string.Empty))
                    {
                        ProcessAssemblyFile(openFileService.FileName);
                    }
                }
            }
            catch (AggregateException AggEx)
            {
                MainAsyncState = AsyncType.Content;
                HasActiveAssembly = false;
                messageBoxService.ShowError(AggEx.InnerException.Message);
            }
            catch (Exception ex)
            {
                MainAsyncState = AsyncType.Content;
                HasActiveAssembly = false;
                messageBoxService.ShowError(ex.Message);

            }
            finally
            {
                isGenerallyBusy = false;
                MainAsyncState = AsyncType.Content;
                ApplicationHelper.DoEvents();
            }
        }
        #endregion

        #region OpenFileDragCommand
        private void ExecuteOpenFileDragCommand(Object parameter)
        {
            isGenerallyBusy = true;
            try
            {
                if (parameter != null)
                {
                    ProcessAssemblyFile(parameter.ToString());
                }
            }
            catch (AggregateException AggEx)
            {
                MainAsyncState = AsyncType.Content;
                HasActiveAssembly = false;
                messageBoxService.ShowError(AggEx.InnerException.Message);
            }
            catch (Exception ex)
            {
                MainAsyncState = AsyncType.Content;
                HasActiveAssembly = false;
                messageBoxService.ShowError(ex.Message);

            }
            finally
            {
                isGenerallyBusy = false;
                MainAsyncState = AsyncType.Content;
                ApplicationHelper.DoEvents();
            }
        }
        #endregion
     
        #region PrintFileCommand
        private void ExecutePrintFileCommand(Object parameter)
        {
            isGenerallyBusy = true;
            try
            {
                Exception printingEx = printPNGFileService.Print(graphLayout);
                if (printingEx != null)
                {
                    messageBoxService.ShowError("There was an error printing graph");
                }
            }
            finally
            {
                isGenerallyBusy = false;
            }
        }
        #endregion
        #endregion
    }
}
