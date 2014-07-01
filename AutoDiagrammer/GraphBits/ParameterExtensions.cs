using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using GraphSharp.Algorithms.Layout.Simple.FDP;
using GraphSharp.Algorithms.Layout.Compound.FDP;
using GraphSharp.Algorithms.Layout.Simple.Hierarchical;
using GraphSharp.Algorithms.Layout.Simple.Tree;
using GraphSharp.Algorithms.Layout;
using GraphSharp.Algorithms.OverlapRemoval;
using System.Globalization;

namespace AutoDiagrammer
{

    public interface ISetting
    {
        void SetFromXmlFragment(XElement fragment);
        XElement GetXmlFragement();

    }

    #region BoundedFRLayoutParameters
    public class BoundedFRLayoutParametersEx : BoundedFRLayoutParameters, ISetting
	{

        public void SetFromXmlFragment(XElement fragment)
        {
            Width = Double.Parse(fragment.Descendants().Where(x => x.Name.LocalName == "Width").Single().Value, CultureInfo.InvariantCulture);
            Height = Double.Parse(fragment.Descendants().Where(x => x.Name.LocalName == "Height").Single().Value, CultureInfo.InvariantCulture);
            AttractionMultiplier = Double.Parse(fragment.Descendants().Where(x => x.Name.LocalName == "AttractionMultiplier").Single().Value, CultureInfo.InvariantCulture);
            RepulsiveMultiplier = Double.Parse(fragment.Descendants().Where(x => x.Name.LocalName == "RepulsiveMultiplier").Single().Value, CultureInfo.InvariantCulture);
            IterationLimit = Int32.Parse(fragment.Descendants().Where(x => x.Name.LocalName == "IterationLimit").Single().Value, CultureInfo.InvariantCulture);
        }



        public XElement GetXmlFragement()
        {
            return
                new XElement("setting", new XAttribute("type", "BoundedFR"),
                        new XElement("Width", Width),
                        new XElement("Height", Height),
                        new XElement("AttractionMultiplier", AttractionMultiplier),
                        new XElement("RepulsiveMultiplier", RepulsiveMultiplier),
                        new XElement("IterationLimit", IterationLimit));
        }

    } 
    #endregion

    #region EfficientSugiyamaLayoutParameters
    public class EfficientSugiyamaLayoutParametersEx : EfficientSugiyamaLayoutParameters, ISetting
	{

        public void SetFromXmlFragment(XElement fragment)
        {
            LayerDistance = Double.Parse(fragment.Descendants().Where(x => x.Name.LocalName == "LayerDistance").Single().Value, CultureInfo.InvariantCulture);
            VertexDistance = Double.Parse(fragment.Descendants().Where(x => x.Name.LocalName == "VertexDistance").Single().Value, CultureInfo.InvariantCulture);
            PositionMode = Int32.Parse(fragment.Descendants().Where(x => x.Name.LocalName == "PositionMode").Single().Value, CultureInfo.InvariantCulture);
            MinimizeEdgeLength = Boolean.Parse(fragment.Descendants().Where(x => x.Name.LocalName == "MinimizeEdgeLength").Single().Value);
            EdgeRouting = (SugiyamaEdgeRoutings)Enum.Parse(typeof(SugiyamaEdgeRoutings), fragment.Descendants().Where(x => x.Name.LocalName == "EdgeRouting").Single().Value);
            OptimizeWidth = Boolean.Parse(fragment.Descendants().Where(x => x.Name.LocalName == "OptimizeWidth").Single().Value);
        }



        public XElement GetXmlFragement()
        {
            return
                new XElement("setting", new XAttribute("type", "EfficientSugiyama"),
                        new XElement("LayerDistance", LayerDistance),
                        new XElement("VertexDistance", VertexDistance),
                        new XElement("PositionMode", PositionMode),
                        new XElement("MinimizeEdgeLength", MinimizeEdgeLength),
                        new XElement("EdgeRouting", EdgeRouting),
                        new XElement("OptimizeWidth", OptimizeWidth));
        }

    } 
    #endregion

    #region FreeFRLayoutParameters
    public class FreeFRLayoutParametersEx : FreeFRLayoutParameters, ISetting
	{

        public void SetFromXmlFragment(XElement fragment)
        {

            IdealEdgeLength = Double.Parse(fragment.Descendants().Where(x => x.Name.LocalName == "IdealEdgeLength").Single().Value, CultureInfo.InvariantCulture);
        }



        public XElement GetXmlFragement()
        {
            return
                new XElement("setting", new XAttribute("type", "FR"),
                        new XElement("IdealEdgeLength", IdealEdgeLength));
        }

    } 
    #endregion

    #region ISOMLayoutParameters
    public class ISOMLayoutParametersEx : ISOMLayoutParameters, ISetting
	{

        public void SetFromXmlFragment(XElement fragment)
        {
            Width = Double.Parse(fragment.Descendants().Where(x => x.Name.LocalName == "Width").Single().Value, CultureInfo.InvariantCulture);
            Height = Double.Parse(fragment.Descendants().Where(x => x.Name.LocalName == "Height").Single().Value, CultureInfo.InvariantCulture);
            InitialAdaption = Double.Parse(fragment.Descendants().Where(x => x.Name.LocalName == "InitialAdaption").Single().Value, CultureInfo.InvariantCulture);
            MinAdaption = Double.Parse(fragment.Descendants().Where(x => x.Name.LocalName == "MinAdaption").Single().Value, CultureInfo.InvariantCulture);
            CoolingFactor = Double.Parse(fragment.Descendants().Where(x => x.Name.LocalName == "CoolingFactor").Single().Value, CultureInfo.InvariantCulture);
            MaxEpoch = Int32.Parse(fragment.Descendants().Where(x => x.Name.LocalName == "MaxEpoch").Single().Value, CultureInfo.InvariantCulture);
            RadiusConstantTime = Int32.Parse(fragment.Descendants().Where(x => x.Name.LocalName == "RadiusConstantTime").Single().Value, CultureInfo.InvariantCulture);
            InitialRadius = Int32.Parse(fragment.Descendants().Where(x => x.Name.LocalName == "InitialRadius").Single().Value, CultureInfo.InvariantCulture);
            MinRadius = Int32.Parse(fragment.Descendants().Where(x => x.Name.LocalName == "MinRadius").Single().Value, CultureInfo.InvariantCulture);
        }



        public XElement GetXmlFragement()
        {
            return
                new XElement("setting", new XAttribute("type", "ISOM"),
                        new XElement("Width", Width),
                        new XElement("Height", Height),
                        new XElement("InitialAdaption", InitialAdaption),
                        new XElement("MinAdaption", MinAdaption),
                        new XElement("CoolingFactor", CoolingFactor),
                        new XElement("MaxEpoch", MaxEpoch),
                        new XElement("RadiusConstantTime", RadiusConstantTime),
                        new XElement("InitialRadius", InitialRadius),
                        new XElement("MinRadius", MinRadius));
        }

    } 
    #endregion

    #region KKLayoutParameters
    public class KKLayoutParametersEx : KKLayoutParameters, ISetting
	{

        public void SetFromXmlFragment(XElement fragment)
        {
            Width = Double.Parse(fragment.Descendants().Where(x => x.Name.LocalName == "Width").Single().Value, CultureInfo.InvariantCulture);
            Height = Double.Parse(fragment.Descendants().Where(x => x.Name.LocalName == "Height").Single().Value, CultureInfo.InvariantCulture);
            DisconnectedMultiplier = Double.Parse(fragment.Descendants().Where(x => x.Name.LocalName == "DisconnectedMultiplier").Single().Value, CultureInfo.InvariantCulture);
            K = Double.Parse(fragment.Descendants().Where(x => x.Name.LocalName == "K").Single().Value, CultureInfo.InvariantCulture);
            LengthFactor = Double.Parse(fragment.Descendants().Where(x => x.Name.LocalName == "LengthFactor").Single().Value, CultureInfo.InvariantCulture);
            MaxIterations = Int32.Parse(fragment.Descendants().Where(x => x.Name.LocalName == "MaxIterations").Single().Value, CultureInfo.InvariantCulture);
            ExchangeVertices = Boolean.Parse(fragment.Descendants().Where(x => x.Name.LocalName == "ExchangeVertices").Single().Value);
            AdjustForGravity = Boolean.Parse(fragment.Descendants().Where(x => x.Name.LocalName == "AdjustForGravity").Single().Value);
        }



        public XElement GetXmlFragement()
        {
            return
                new XElement("setting", new XAttribute("type", "KK"),
                        new XElement("Width", Width),
                        new XElement("Height", Height),
                        new XElement("DisconnectedMultiplier", DisconnectedMultiplier),
                        new XElement("K", K),
                        new XElement("LengthFactor", LengthFactor),
                        new XElement("MaxIterations", MaxIterations),
                        new XElement("ExchangeVertices", ExchangeVertices),
                        new XElement("AdjustForGravity", AdjustForGravity));
        }

    } 
    #endregion

    #region SimpleTreeLayoutParameters
    public class SimpleTreeLayoutParametersEx : SimpleTreeLayoutParameters, ISetting
	{

        public void SetFromXmlFragment(XElement fragment)
        {
            LayerGap = Double.Parse(fragment.Descendants().Where(x => x.Name.LocalName == "LayerGap").Single().Value, CultureInfo.InvariantCulture);
            VertexGap = Double.Parse(fragment.Descendants().Where(x => x.Name.LocalName == "VertexGap").Single().Value, CultureInfo.InvariantCulture);
            SpanningTreeGeneration = (SpanningTreeGeneration)Enum.Parse(typeof(SpanningTreeGeneration), fragment.Descendants().Where(x => x.Name.LocalName == "SpanningTreeGeneration").Single().Value);
            Direction = (LayoutDirection)Enum.Parse(typeof(LayoutDirection), fragment.Descendants().Where(x => x.Name.LocalName == "Direction").Single().Value);
        }



        public XElement GetXmlFragement()
        {
            return
                new XElement("setting", new XAttribute("type", "Tree"),
                        new XElement("LayerGap", LayerGap),
                        new XElement("VertexGap", VertexGap),
                        new XElement("SpanningTreeGeneration", SpanningTreeGeneration),
                        new XElement("Direction", Direction));
        }

    } 
    #endregion

    #region OverlapRemovalParameters
    public class OverlapRemovalParametersEx : OverlapRemovalParameters, ISetting
	{

        public void SetFromXmlFragment(XElement fragment)
        {
            HorizontalGap = float.Parse(fragment.Descendants().Where(x => x.Name.LocalName == "HorizontalGap").Single().Value, CultureInfo.InvariantCulture);
            VerticalGap = float.Parse(fragment.Descendants().Where(x => x.Name.LocalName == "VerticalGap").Single().Value, CultureInfo.InvariantCulture);
        }



        public XElement GetXmlFragement()
        {
            return
                new XElement("setting", new XAttribute("type", "Overlap"),
                        new XElement("HorizontalGap", HorizontalGap),
                        new XElement("VerticalGap", VerticalGap));
        }

    } 
    #endregion

    
}
