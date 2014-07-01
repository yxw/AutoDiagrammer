using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Threading;

namespace AutoDiagrammer
{
    public interface IAssemblyManipulationService
    {
        List<AssemblyTreeViewModel> TreeValues { get; }
        List<AssemblyTreeViewModel> SelectedTreeValues { get; }


        Task<GraphResults> CreateGraph();
        void ReInitialise();
        void CalculateSelectedTreeValues();
        Task LoadNameSpacesAndTypes(String assemblyFileName);

    }
}
