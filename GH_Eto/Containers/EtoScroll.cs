using System;
using System.Collections.Generic;
using System.Reflection;
using Eto.Forms;
using Eto.Drawing;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

namespace Synapse
{
    public class EtoScroll : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the EtoScroll class.
        /// </summary>
        public EtoScroll()
          : base("SynapseScroll", "Scroll",
              "scrollable container",
              "Synapse", "Container")
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
            pManager.AddGenericParameter("Controls", "C", "controls to go into this container", GH_ParamAccess.list);
            pManager[2].DataMapping = GH_DataMapping.Flatten;
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("All Properties", "A", "list of all accessible properties", GH_ParamAccess.list);
            pManager.AddGenericParameter("Control", "C", "container control that houses other controls", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<string> props = new List<string>();
            List<GH_ObjectWrapper> vals = new List<GH_ObjectWrapper>();
            List<GH_ObjectWrapper> ctrls = new List<GH_ObjectWrapper>();
            DA.GetDataList(0, props);
            DA.GetDataList(1, vals);
            DA.GetDataList(2, ctrls);

            Scrollable scroll = new Scrollable();
            DynamicLayout layout = new DynamicLayout();
            foreach (GH_ObjectWrapper gho in ctrls)
                if (gho.Value is Control c)
                    layout.AddAutoSized(c);
            scroll.Content = layout;

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

                if (n.ToLower() == "border" || n.ToLower() == "bordertype")
                {
                    if (val is GH_String gstr)
                        switch (gstr.Value.ToLower())
                        {
                            case "bezel":
                                scroll.Border = BorderType.Bezel;
                                break;
                            case "line":
                                scroll.Border = BorderType.Line;
                                break;
                            case "none":
                                scroll.Border = BorderType.None;
                                break;
                            default:
                                scroll.Border = BorderType.None;
                                break;
                        }
                    else if (val is GH_Integer gint)
                        switch (gint.Value)
                        {
                            case 0:
                                scroll.Border = BorderType.Bezel;
                                break;
                            case 1:
                                scroll.Border = BorderType.Line;
                                break;
                            case 2:
                                scroll.Border = BorderType.None;
                                break;
                            default:
                                scroll.Border = BorderType.None;
                                break;
                        }
                    else
                        try { Util.SetProp(scroll, "Border", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else   
                    try { Util.SetProp(scroll, n, Util.GetGooVal(val)); }
                    catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
            }

            DA.SetData(1, new GH_ObjectWrapper(scroll));

            PropertyInfo[] allprops = scroll.GetType().GetProperties();
            List<string> printouts = new List<string>();
            foreach (PropertyInfo prop in allprops)
                if (prop.CanWrite)
                    printouts.Add(prop.Name + ": " + prop.PropertyType.ToString());
            DA.SetDataList(0, printouts);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.scroll;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("1b71fb11-0ad7-4f5d-b713-77ec5a6272ec"); }
        }
    }
}