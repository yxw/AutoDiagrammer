using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoDiagrammer
{
    public class GraphResults
    {
        #region Data
        public List<PocVertex> Vertices { get; private set; }
        public List<PocEdge> Edges { get; private set; }
        #endregion

        #region Ctor
        public GraphResults(List<PocVertex> vertices, List<PocEdge> edges)
        {
            this.Vertices = vertices;
            this.Edges = edges;
        }
        #endregion
    }
}
