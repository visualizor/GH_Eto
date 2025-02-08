using System;
using System.Collections.Generic;
using System.Reflection;
using Eto.Forms;
using Eto.Drawing;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using wf=System.Windows.Forms;

namespace Synapse
{
    public class EtoScroll_OBSOLETE : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the EtoScroll class.
        /// </summary>
        public EtoScroll_OBSOLETE()
          : base("SnpScroll", "SScroll",
              "scrollable container",
              "Synapse", "Containers")
        {
        }

        public override void AppendAdditionalMenuItems(wf.ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);
            wf.ToolStripMenuItem click = menu.Items.Add("List Properties", null, Util.OnListProps) as wf.ToolStripMenuItem;
            click.ToolTipText = "put all properties of this control in a check list";
            Util.ListPropLoc = Attributes.Pivot;
            Scrollable dummy = new Scrollable();
            Util.ListPropType = dummy.GetType();
            dummy.Dispose();
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
                else if (n.ToLower() == "padding")
                {
                    if (val is GH_Point pt)
                        scroll.Padding = new Padding((int)pt.Value.X, (int)pt.Value.Y);
                    else if (val is GH_Vector vec)
                        scroll.Padding = new Padding((int)vec.Value.X, (int)vec.Value.Y);
                    else if (val is GH_String pstr)
                    {
                        string[] subs = pstr.Value.Split(',');
                        if (subs.Length == 2)
                        {
                            bool i0 = int.TryParse(subs[0], out int n0);
                            bool i1 = int.TryParse(subs[1], out int n1);
                            if (i0 && i1)
                                scroll.Padding = new Padding(n0, n1);
                        }
                        else if (subs.Length == 4)
                        {
                            bool i0 = int.TryParse(subs[0], out int n0);
                            bool i1 = int.TryParse(subs[1], out int n1);
                            bool i2 = int.TryParse(subs[2], out int n2);
                            bool i3 = int.TryParse(subs[3], out int n3);
                            if (i0 && i1 && i2 && i3)
                                scroll.Padding = new Padding(n0, n1, n2, n3);
                        }
                    }
                    else if (val is GH_Integer pad)
                        scroll.Padding =  new Padding(pad.Value);
                    else if (val is GH_Rectangle grec)
                    {
                        int x = (int)grec.Value.X.Length;
                        int y = (int)grec.Value.Y.Length;
                        scroll.Padding = new Padding(x, y);
                    }
                    else if (val is GH_ComplexNumber gcomp)
                    {
                        int x = (int)gcomp.Value.Real;
                        int y = (int)gcomp.Value.Imaginary;
                        scroll.Padding = new Padding(x, y);
                    }
                    else
                        try { Util.SetProp(scroll, "Padding", Util.GetGooVal(val)); }
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

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.hidden; }
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