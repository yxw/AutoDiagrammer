using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cinch;
using MEFedMVVM.ViewModelLocator;
using System.ComponentModel.Composition;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Threading;
using System.Text.RegularExpressions;

namespace AutoDiagrammer
{
    /// <summary>
    /// This class implements the IPrintXPSFileService
    /// </summary>
    [PartCreationPolicy(CreationPolicy.Shared)]
    [ExportService(ServiceType.Both, typeof(IAssemblyManipulationService))]
    public class AssemblyManipulationService : IAssemblyManipulationService
    {
        #region Data
        private ITreeCreator treeCreator;
        private List<AssemblyTreeViewModel> treeValues;
        private List<AssemblyTreeViewModel> selectedTreeValues = new List<AssemblyTreeViewModel>();
        #endregion

        #region Ctor
        // private to prevent direct instantiation.
        public AssemblyManipulationService()
        {
            treeCreator = ViewModelRepository.Instance.Resolver.Container.GetExport<ITreeCreator>().Value;
        }
        #endregion

        #region Public Properties

        public List<AssemblyTreeViewModel> TreeValues
        {
            get { return treeValues; }
        }


        public List<AssemblyTreeViewModel> SelectedTreeValues
        {
            get
            {
                return selectedTreeValues;
            }
        }
        #endregion

        #region Public Methods

        public Task<GraphResults> CreateGraph()
        {
            Task<GraphResults> task = Task.Factory.StartNew<GraphResults>(() =>
                {
                    //for each item in selectedTreeItems
                    //1. Create all PocVertex, and are them to Reflect() which will store an internal
                    //   List<Name> which are the Associations needed by that Vertex
                    //2. Go through each PocVertex Associations and see if we have that Association
                    //   Vertex and if so create a new PocEdge


                    List<PocVertex> vertices = new List<PocVertex>();
                    Parallel.For(0, selectedTreeValues.Count, (i) =>
                    {
                        SerializableVertex serializableVertex = selectedTreeValues[i].Vertex;
                        PocVertex vertex = new PocVertex(
                            serializableVertex.Name,
                            serializableVertex.ShortName,
                            serializableVertex.Constructors,
                            serializableVertex.Fields,
                            serializableVertex.Properties,
                            serializableVertex.Interfaces,
                            TranslateMethods(serializableVertex.Methods),
                            serializableVertex.Events,
                            serializableVertex.Associations,
                            serializableVertex.HasConstructors,
                            serializableVertex.HasFields,
                            serializableVertex.HasProperties,
                            serializableVertex.HasInterfaces,
                            serializableVertex.HasMethods,
                            serializableVertex.HasEvents);


                        vertices.Add(vertex);
                    });

                    List<PocEdge> edges = new List<PocEdge>();
                    Parallel.ForEach(vertices, (x) =>
                        {
                            PocVertex vertex1 = x;

                            foreach (String associationName in vertex1.Associations)
                            {
                                var matchinVertices = from vert in vertices
                                                      where vert.Name == associationName
                                                      select vert;
                                PocVertex vertex2 = matchinVertices.SingleOrDefault();

                                if (vertex2 != null)
                                {
                                    if (vertex1.Name != vertex2.Name)
                                    {
                                        //TODO : Need to make sure both of these are in the
                                        //list of selected items in the tree before they are added
                                        edges.Add(AddNewGraphEdge(vertex1, vertex2));
                                        vertex1.NumberOfEdgesFromThisVertex += 1;
                                        vertex2.NumberOfEdgesToThisVertex += 1;
                                    }
                                }

                            }
                        });
                    

                    return new GraphResults(vertices, edges);

                });
            return task;

        }

        public void ReInitialise()
        {
            selectedTreeValues = new List<AssemblyTreeViewModel>();
            treeValues = new List<AssemblyTreeViewModel>();
        }

        public void CalculateSelectedTreeValues()
        {
            List<AssemblyTreeViewModel> results = new List<AssemblyTreeViewModel>();

            if (treeValues != null && treeValues.Count > 0)
                RecurseTreeLookingForSelected(treeValues[0], results);

            selectedTreeValues = results;
        }


        public Task LoadNameSpacesAndTypes(String assemblyFileName)
        {
            Task task = Task.Factory.StartNew(() =>
                {

                    try
                    {
                        treeValues = treeCreator.ScanAssemblyAndCreateTree(assemblyFileName);
                        return treeValues;
                    }
                    catch (Exception ex)
                    {
                        treeValues = null;
                        throw ex;
                    }
                });
            return task;

        }

        #endregion

        #region Private Methods

        private List<MethodData> TranslateMethods(List<SerializableMethodData> serializedMethods)
        {
            return (from x in serializedMethods 
                    select new MethodData(x.MethodName, x.MethodBodyIL)).ToList();

        }


        private void RecurseTreeLookingForSelected(AssemblyTreeViewModel parent,    List<AssemblyTreeViewModel> selectedTreeValues)
        {
            if (parent.IsChecked == true && parent.NodeType == RepresentationType.Class)
            {
                if (!selectedTreeValues.Contains(parent))
                    selectedTreeValues.Add(parent);
            }

            foreach (AssemblyTreeViewModel child in parent.Children)
            {
                if (child.IsChecked == true && child.NodeType == RepresentationType.Class)
                {
                    if (!selectedTreeValues.Contains(child))
                        selectedTreeValues.Add(child);
                }
                RecurseTreeLookingForSelected(child, selectedTreeValues);
            }
        }

        private PocEdge AddNewGraphEdge(PocVertex from, PocVertex to)
        {
            string edgeString = string.Format("{0}-{1} Connected", from.Name, to.Name);
            PocEdge newEdge = new PocEdge(edgeString, from, to);
            return newEdge;
        }
        #endregion
    }
}
