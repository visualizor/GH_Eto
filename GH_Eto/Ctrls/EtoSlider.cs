using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Eto.Forms;
using Eto.Drawing;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

namespace Synapse
{
    public class EtoSlider : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the EtoSlider class.
        /// </summary>
        public EtoSlider()
          : base("SynapseIntSlider", "SSint",
              "simple integer slider",
              "Synapse", "Controls")
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
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("All Properties", "A", "list of all accessible properties", GH_ParamAccess.list);
            pManager.AddGenericParameter("Control", "C", "control to go into a container or the listener", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<string> props = new List<string>();
            List<GH_ObjectWrapper> vals = new List<GH_ObjectWrapper>();
            DA.GetDataList(0, props);
            DA.GetDataList(1, vals);

            Slider sl = new Slider() { ID = Guid.NewGuid().ToString(), };
            sl.ValueChanged += (s,e)=> sl.ToolTip = sl.Value.ToString();

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

                if (n.ToLower() == "minvalue" || n.ToLower() == "minimum")
                {
                    if (val is GH_String gstr)
                    {
                        if (int.TryParse(gstr.Value, out int sli))
                            sl.MinValue = sli;
                    }
                    else if (val is GH_Integer gint)
                        sl.MinValue = gint.Value;
                    else if (val is GH_Number gnum)
                        sl.MinValue = (int)gnum.Value;
                    else if (val is GH_Boolean gbool)
                        if (gbool.Value)
                            sl.MinValue = 1;
                        else
                            sl.MinValue = 0;
                    else
                        try { Util.SetProp(sl, "MinValue", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else if (n.ToLower() == "maxvalue" || n.ToLower() == "maximum")
                {
                    if (val is GH_String gstr)
                    {
                        if (int.TryParse(gstr.Value, out int sli))
                            sl.MaxValue = sli;
                    }
                    else if (val is GH_Integer gint)
                        sl.MaxValue = gint.Value;
                    else if (val is GH_Number gnum)
                        sl.MaxValue = (int)gnum.Value;
                    else if (val is GH_Boolean gbool)
                        if (gbool.Value)
                            sl.MaxValue = 1;
                        else
                            sl.MaxValue = 0;
                    else
                        try { Util.SetProp(sl, "MaxValue", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else if (n.ToLower() == "orientation" || n.ToLower() == "direction")
                {
                    if (val is GH_String ghstr)
                        switch (ghstr.Value.ToLower())
                        {
                            case "horizontal":
                                sl.Orientation = Orientation.Horizontal;
                                break;
                            case "vertical":
                                sl.Orientation = Orientation.Vertical;
                                break;
                            case "h":
                                sl.Orientation = Orientation.Horizontal;
                                break;
                            case "v":
                                sl.Orientation = Orientation.Vertical;
                                break;
                            default:
                                break;
                        }
                    else if (val is GH_Integer ghi)
                    {
                        if (ghi.Value == 0)
                            sl.Orientation = Orientation.Horizontal;
                        else if (ghi.Value == 1)
                            sl.Orientation = Orientation.Vertical;
                    }
                    else if (val is Orientation or)
                        sl.Orientation = or;
                    else
                        try { Util.SetProp(sl, "Orientation", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else if (n.ToLower() == "tickfrequency" || n.ToLower() == "tickstep")
                {
                    if (val is GH_Integer gint)
                        sl.TickFrequency = gint.Value > 0 ? gint.Value : 1;
                    else if (val is GH_Number gnum)
                        sl.TickFrequency = gnum.Value > 0 ? (int)gnum.Value : 1;
                    else if (val is GH_String gstr)
                    {
                        if (int.TryParse(gstr.Value, out int tickint))
                            sl.TickFrequency = tickint;
                    }
                    else
                        try { Util.SetProp(sl, "TickFrequency", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else
                    try { Util.SetProp(sl, n, Util.GetGooVal(val)); }
                    catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
            }

            DA.SetData(1, new GH_ObjectWrapper(sl));

            PropertyInfo[] allprops = sl.GetType().GetProperties();
            List<string> printouts = new List<string>();
            foreach (PropertyInfo prop in allprops)
                if (prop.CanWrite)
                    printouts.Add(prop.Name + ": " + prop.PropertyType.ToString());
            DA.SetDataList(0, printouts);
        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.obscure | GH_Exposure.secondary; }
        }
        
        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.slideronly;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("28baf6ba-1e74-4f32-acdc-c83d3ba67c23"); }
        }
    }
}