using System;
using System.Collections.Generic;
using System.Reflection;
using Eto.Forms;
using Eto.Drawing;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;

namespace Synapse
{
    public class EtoButton : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the EtoButton class.
        /// </summary>
        public EtoButton()
          : base("SynapseButton", "SButton",
              "button",
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

            Button btn = new Button() { Text = "a button", ID = Guid.NewGuid().ToString() };
            for (int i = 0; i< props.Count; i++)
            {
                string n = props[i];
                object val;
                try { val = vals[i].Value; }
                catch (ArgumentOutOfRangeException)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "P, V should correspond each other");
                    return;
                }

                // TODO: more intelligent properties. see github issue #7
                if (n.ToLower() == "size")
                {
                    if (val is GH_Point pt)
                    {
                        Size winsize = new Size((int)pt.Value.X, (int)pt.Value.Y);
                        Util.SetProp(btn, "Size", winsize);
                    }
                    else if (val is GH_Vector vec)
                    {
                        Size winsize = new Size((int)vec.Value.X, (int)vec.Value.Y);
                        Util.SetProp(btn, "Size", winsize);
                    }
                    else if (val is GH_String sstr)
                    {
                        string[] xy = sstr.Value.Split(',');
                        bool xp = int.TryParse(xy[0], out int x);
                        bool yp = int.TryParse(xy[1], out int y);
                        if (xp && yp)
                            Util.SetProp(btn, "Size", new Size(x, y));
                    }
                    else if (val is GH_Rectangle grec)
                    {
                        int x = (int)grec.Value.X.Length;
                        int y = (int)grec.Value.Y.Length;
                        btn.Size = new Size(x, y);
                    }
                    else
                        try { Util.SetProp(btn, "Size", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else if (n.ToLower() == "location" || n.ToLower() == "position")
                {
                    if (val is GH_Point pt)
                        Util.SetProp(btn, "Location", new Eto.Drawing.Point((int)pt.Value.X, (int)pt.Value.Y));
                    else if (val is GH_Vector vec)
                        Util.SetProp(btn, "Location", new Eto.Drawing.Point((int)vec.Value.X, (int)vec.Value.Y));
                    else if (val is GH_String locstr)
                    {
                        if (Point3d.TryParse(locstr.Value, out Point3d rhpt))
                            Util.SetProp(btn, "Location", new Eto.Drawing.Point((int)rhpt.X, (int)rhpt.Y));
                    }
                    else
                        try { Util.SetProp(btn, "Location", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else if (n.ToLower() == "text" || n.ToLower() == "label")
                {
                    if (val is GH_String txt)
                        btn.Text = txt.Value;
                    else
                        btn.Text = val.ToString();
                }
                else if (n.ToLower() == "tooltip" || n.ToLower() == "tool tip" || n.ToLower() == "hint")
                {
                    if (val is GH_String txt)
                        btn.ToolTip = txt.Value;
                    else
                        btn.ToolTip = val.ToString();
                }
                else if (n.ToLower() == "font" || n.ToLower() == "typeface")
                {
                    if (val is GH_String txt)
                    {
                        string fam = txt.Value;
                        try
                        {
                            Font efont = new Font(fam, (float)8.25);
                            btn.Font = efont;
                        }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                    }
                    else if (val is Font f)
                        btn.Font = f;
                    else
                        try { Util.SetProp(btn, "Font", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else if (n.ToLower()=="textcolor" || n.ToLower() == "fontcolor")
                {
                    if (val is GH_Colour gclr)
                        btn.TextColor = Color.FromArgb(gclr.Value.ToArgb());
                    else if (val is GH_String gstr)
                    {
                        if (Color.TryParse(gstr.Value, out Color clr))
                            btn.TextColor = clr;
                    }
                    else if (val is GH_Point pt)
                    {
                        Color clr = Color.FromArgb((int)pt.Value.X, (int)pt.Value.Y, (int)pt.Value.Z);
                        btn.TextColor = clr;
                    }
                    else if (val is GH_Vector vec)
                    {
                        Color clr = Color.FromArgb((int)vec.Value.X, (int)vec.Value.Y, (int)vec.Value.Z);
                        btn.TextColor = clr;
                    }
                    else if (val is Color etoclr)
                        btn.TextColor = etoclr;
                    else
                        try { Util.SetProp(btn, "TextColor", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else if (n.ToLower() == "image")
                {
                    if (val is GH_String gstr)
                    {
                        Bitmap img = new Bitmap(gstr.Value);
                        btn.Image = img;
                    }
                    else
                        try { Util.SetProp(btn, "Image", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else
                    try { Util.SetProp(btn, n, Util.GetGooVal(val)); }
                    catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
            }

            DA.SetData(1, new GH_ObjectWrapper(btn));

            PropertyInfo[] allprops = btn.GetType().GetProperties();
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
                return Properties.Resources.btn;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("f8b49b4f-5bbf-450f-b111-7c0d1ca7b000"); }
        }
    }
}