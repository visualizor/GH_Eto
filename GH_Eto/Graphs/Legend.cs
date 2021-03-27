using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Forms;
using Eto.Drawing;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

namespace Synapse
{
    public class EtoLegend : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the EtoPieLg class.
        /// </summary>
        public EtoLegend()
          : base("SynapseLegend", "Legend",
              "chart legend",
              "Synapse", "Graphs")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Property", "P", "property to set", GH_ParamAccess.list);
            pManager[0].DataMapping = GH_DataMapping.Flatten;
            pManager[0].Optional = true;
            pManager.AddGenericParameter("Property Value", "V", "values for the property", GH_ParamAccess.list);
            pManager[1].DataMapping = GH_DataMapping.Flatten;
            pManager[1].Optional = true;
            pManager.AddGenericParameter("Data", "D", "chart data object from a graph", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("All Properties", "A", "list of all accessible properties", GH_ParamAccess.list);
            pManager.AddGenericParameter("Legend", "L", "legend for chart", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_ObjectWrapper gho = new GH_ObjectWrapper();
            DA.GetData(2, ref gho);
            List<string> props = new List<string>();
            List<GH_ObjectWrapper> vals = new List<GH_ObjectWrapper>();
            DA.GetDataList(0, props);
            DA.GetDataList(1, vals);

            ChartLegend legend;
            if (gho.Value is ChartData cd)
            {
                string[] pcts = new string[cd.AppdVals.Length];
                for (int i = 0; i < pcts.Length; i++)
                {
                    double p = cd.AppdVals[i];
                    string pstr;
                    if (cd.Type == ChartType.Region || cd.Type == ChartType.Pie)
                        pstr = Math.Round(p * 100, 2).ToString();
                    else
                        pstr = p.ToString();
                    pcts.SetValue(pstr, i);
                }
                legend = new ChartLegend(cd.Keys, cd.Colors, pcts);
                switch (cd.Type)
                {
                    case ChartType.Pie:
                        legend.AppdPrefix = "";
                        legend.AppdSuffix = "%";
                        break;
                    case ChartType.Bar:
                        legend.AppdPrefix = "Value: ";
                        legend.AppdSuffix = "";
                        break;
                    case ChartType.Trend:
                        legend.AppdPrefix = "Avg. ";
                        legend.AppdSuffix = "";
                        break;
                    case ChartType.Region:
                        legend.AppdPrefix = "";
                        legend.AppdSuffix = "%";
                        break;
                    default:
                        break;
                }
            }
            else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " not valid pie chart data object\n nothing done");
                return;
            }

            for (int i = 0; i < props.Count; i++)
            {
                string n = props[i];
                object val;
                try { val = vals[i].Value; }
                catch (ArgumentOutOfRangeException)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "P, V should correspond each other");
                    return;
                }

                switch (n.ToLower())
                {
                    case "font":
                        try { legend.SetFont(val as Font); }
                        catch { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " set font failed"); }
                        break;
                    case "marker":
                        if (val is GH_String gstr)
                            legend.Marker = gstr.Value;
                        else if (val is GH_Integer gint)
                            switch (gint.Value)
                            {
                                case 0:
                                    legend.Marker = "bullet";
                                    break;
                                case 1:
                                    legend.Marker = "box";
                                    break;
                                case 2:
                                    legend.Marker = "pointer";
                                    break;
                                default:
                                    break;
                            }
                        break;
                    case "orientation":
                        if (val is GH_Boolean gb)
                            legend.Horizontal = gb.Value;
                        else if (val is GH_String gs)
                            if (gs.Value.ToLower() == "horizontal")
                                legend.Horizontal = true;
                            else
                                legend.Horizontal = false;
                        break;
                    case "showmore":
                        if (val is GH_Boolean gbool)
                            legend.ShowAppd = gbool.Value;
                        else if (val is GH_String pstr)
                            legend.ShowAppd = pstr.Value.ToLower() == "show" || pstr.Value.ToLower() == "true";
                        break;
                    default:
                        break;
                }
            }
            DA.SetData(1, new GH_ObjectWrapper(legend.RenderLegend()));

            DA.SetDataList(0, new string[] {
                "Font: Eto.Drawing.Font",
                "Marker: string",
                "Orientation: int",
                "ShowMore: bool",
            });
        }


        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.pietag;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("01f2a829-9fc5-407e-9cba-4a0760a75851"); }
        }
    }
}