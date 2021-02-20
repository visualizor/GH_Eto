using System;
using System.Collections.Generic;
using Eto.Forms;
using Eto.Drawing;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

namespace Synapse.Ctrls
{
    public class EtoLabel : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the EtoLabel class.
        /// </summary>
        public EtoLabel()
          : base("SynapseLabel", "SLabel",
              "label object, just a plain text tag",
              "Synapse", "Controls")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
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
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
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

            Label label = new Label();
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

                if (n.ToLower() == "text" || n.ToLower() == "label")
                {
                    if (val is GH_String gstr)
                        label.Text = gstr.Value;
                    else
                        label.Text = val.ToString();
                }
                else if (n.ToLower() == "textcolor" || n.ToLower() == "fontcolor")
                {
                    if (val is GH_Colour gclr)
                        label.TextColor = Color.FromArgb(gclr.Value.ToArgb());
                    else if (val is GH_String gstr)
                    {
                        if (Color.TryParse(gstr.Value, out Color clr))
                            label.TextColor = clr;
                    }
                    else if (val is GH_Point pt)
                    {
                        Color clr = Color.FromArgb((int)pt.Value.X, (int)pt.Value.Y, (int)pt.Value.Z);
                        label.BackgroundColor = clr;
                    }
                    else if (val is GH_Vector vec)
                    {
                        Color clr = Color.FromArgb((int)vec.Value.X, (int)vec.Value.Y, (int)vec.Value.Z);
                        label.BackgroundColor = clr;
                    }
                    else if (val is Color etoclr)
                        label.BackgroundColor = etoclr;
                    else
                        try { Util.SetProp(label, "BackgroundColor", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else if (n.ToLower() =="font" || n.ToLower() == "typeface")
                {
                    if (val is GH_String txt)
                    {
                        string fam = txt.Value;
                        try
                        {
                            Font efont = new Font(fam, (float)8.25);
                            label.Font = efont;
                        }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                    }
                    else if (val is Font f)
                        label.Font = f;
                    else
                        try { Util.SetProp(label, "Font", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else if (n.ToLower() == "size")
                {
                    if (val is GH_Point pt)
                    {
                        Size winsize = new Size((int)pt.Value.X, (int)pt.Value.Y);
                        Util.SetProp(label, "Size", winsize);
                    }
                    else if (val is GH_Vector vec)
                    {
                        Size winsize = new Size((int)vec.Value.X, (int)vec.Value.Y);
                        Util.SetProp(label, "Size", winsize);
                    }
                    else if (val is GH_String sstr)
                    {
                        string[] xy = sstr.Value.Split(',');
                        bool xp = int.TryParse(xy[0], out int x);
                        bool yp = int.TryParse(xy[1], out int y);
                        if (xp && yp)
                            Util.SetProp(label, "Size", new Size(x, y));
                    }
                    else if (val is GH_Rectangle grec)
                    {
                        int x = (int)grec.Value.X.Length;
                        int y = (int)grec.Value.Y.Length;
                        label.Size = new Size(x, y);
                    }
                    else
                        try { Util.SetProp(label, "Size", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else if (n.ToLower() == "location")
                {

                }
                else if (n.ToLower() == "wrap" || n.ToLower() == "textwrap" || n.ToLower() == "wrapmode")
                {

                }
                else
                    try { Util.SetProp(label, n, Util.GetGooVal(val)); }
                    catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
            }
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.label;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("3b9e516e-c8ae-4e90-b133-48c316254bdf"); }
        }
    }
}