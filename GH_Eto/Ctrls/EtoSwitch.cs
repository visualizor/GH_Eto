using System;
using System.Collections.Generic;
using System.Reflection;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Eto.Forms;
using wf = System.Windows.Forms;
using Eto.Drawing;

namespace Synapse
{
    public class EtoSwitch : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the EtoSwitch class.
        /// </summary>
        public EtoSwitch()
          : base("SnpToggle", "SToggle",
              "a toggle switch",
              "Synapse", "Controls")
        {
        }
        public override void AppendAdditionalMenuItems(wf.ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);
            try
            {
                wf.ToolStripMenuItem click = menu.Items.Add("List Properties", null, Util.OnListProps) as wf.ToolStripMenuItem;
                click.ToolTipText = "put all properties of this control in a check list";
            }
            catch (NullReferenceException) { }
            
            Util.ListPropLoc = Attributes.Pivot;
            ToggleButton dummy = new ToggleButton();
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

            ESwitch es = new ESwitch() { ID = Guid.NewGuid().ToString(), };

            for (int i = 0; i < props.Count; i++)
            {
                string n = props[i].Trim();
                object val;
                try { val = vals[i].Value; }
                catch (ArgumentOutOfRangeException)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "P, V should correspond each other");
                    return;
                }

                if (n.ToLower() == "size")
                {
                    if (val is GH_Point pt)
                    {
                        Size size = new Size((int)pt.Value.X, (int)pt.Value.Y);
                        es.Size = size;
                    }
                    else if (val is Size esize)
                        es.Size = esize;
                    else if (val is GH_Vector vec)
                    {
                        Size size = new Size((int)vec.Value.X, (int)vec.Value.Y);
                        es.Size = size;
                    }
                    else if (val is GH_String sstr)
                    {
                        string[] xy = sstr.Value.Split(',');
                        bool xp = int.TryParse(xy[0], out int x);
                        bool yp = int.TryParse(xy[1], out int y);
                        if (xp && yp)
                            es.Size = new Size(x, y);
                    }
                    else if (val is GH_Rectangle grec)
                    {
                        int x = (int)grec.Value.X.Length;
                        int y = (int)grec.Value.Y.Length;
                        es.Size = new Size(x, y);
                    }
                    else if (val is GH_ComplexNumber gcomp)
                    {
                        int x = (int)gcomp.Value.Real;
                        int y = (int)gcomp.Value.Imaginary;
                        es.Size = new Size(x, y);
                    }
                    else
                        try { Util.SetProp(es, "Size", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else if (n.ToLower() == "color" || n.ToLower() == "activecolor")
                {
                    if (val is GH_Colour gclr)
                        es.ActiveColor = Color.FromArgb(gclr.Value.ToArgb());
                    else if (val is GH_String gstr)
                    {
                        if (Color.TryParse(gstr.Value, out Color clr))
                            es.ActiveColor = clr;
                    }
                    else if (val is GH_Point pt)
                    {
                        Color clr = Color.FromArgb((int)pt.Value.X, (int)pt.Value.Y, (int)pt.Value.Z);
                        es.ActiveColor = clr;
                    }
                    else if (val is GH_Vector vec)
                    {
                        Color clr = Color.FromArgb((int)vec.Value.X, (int)vec.Value.Y, (int)vec.Value.Z);
                        es.ActiveColor = clr;
                    }
                    else if (val is Color etoclr)
                        es.ActiveColor = etoclr;
                    else
                        try { Util.SetProp(es, "ActiveColor", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else if (n.ToLower() == "inactivecolor" || n.ToLower()=="backgroundcolor")
                {
                    if (val is GH_Colour gclr)
                        es.InactiveColor = Color.FromArgb(gclr.Value.ToArgb());
                    else if (val is GH_String gstr)
                    {
                        if (Color.TryParse(gstr.Value, out Color clr))
                            es.InactiveColor = clr;
                    }
                    else if (val is GH_Point pt)
                    {
                        Color clr = Color.FromArgb((int)pt.Value.X, (int)pt.Value.Y, (int)pt.Value.Z);
                        es.InactiveColor = clr;
                    }
                    else if (val is GH_Vector vec)
                    {
                        Color clr = Color.FromArgb((int)vec.Value.X, (int)vec.Value.Y, (int)vec.Value.Z);
                        es.InactiveColor = clr;
                    }
                    else if (val is Color etoclr)
                        es.InactiveColor = etoclr;
                    else
                        try { Util.SetProp(es, "InactiveColor", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else if (n.ToLower() == "knobcolor" || n.ToLower() == "handlecolor")
                {
                    if (val is GH_Colour gclr)
                        es.KnobColor = Color.FromArgb(gclr.Value.ToArgb());
                    else if (val is GH_String gstr)
                    {
                        if (Color.TryParse(gstr.Value, out Color clr))
                            es.KnobColor = clr;
                    }
                    else if (val is GH_Point pt)
                    {
                        Color clr = Color.FromArgb((int)pt.Value.X, (int)pt.Value.Y, (int)pt.Value.Z);
                        es.KnobColor = clr;
                    }
                    else if (val is GH_Vector vec)
                    {
                        Color clr = Color.FromArgb((int)vec.Value.X, (int)vec.Value.Y, (int)vec.Value.Z);
                        es.KnobColor = clr;
                    }
                    else if (val is Color etoclr)
                        es.KnobColor = etoclr;
                    else
                        try { Util.SetProp(es, "KnobColor", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else if (n.ToLower() == "tooltip" || n.ToLower() == "tool tip" || n.ToLower() == "hint")
                {
                    if (val is GH_String txt)
                        es.ToolTip = txt.Value;
                    else
                        es.ToolTip = val.ToString();
                }
                else
                    try { Util.SetProp(es, n, Util.GetGooVal(val)); }
                    catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }

            }
            DA.SetData(1, new GH_ObjectWrapper(es));

            PropertyInfo[] allprops = es.GetType().GetProperties();
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
                return Properties.Resources.toggle;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("726109F5-EA8E-42B4-B97C-FD63B86CA4C8"); }
        }
    }
}