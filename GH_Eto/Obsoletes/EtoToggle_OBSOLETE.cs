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
    public class EtoToggle_OBSOLETE : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the EtoToggle class.
        /// </summary>
        public EtoToggle_OBSOLETE()
          : base("SnpToggle", "SToggle",
              "a special toggle button",
              "Synapse", "Controls")
        {
        }

        public override void AppendAdditionalMenuItems(wf.ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);
            wf.ToolStripMenuItem click = menu.Items.Add("List Properties", null, Util.OnListProps) as wf.ToolStripMenuItem;
            click.ToolTipText = "put all properties of this control in a check list";
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

            ToggleButton toggle = new ToggleButton() { ID = Guid.NewGuid().ToString(), Text="a toggle",};

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

                if (n.ToLower() == "size")
                {
                    if (val is GH_Point pt)
                    {
                        Size size = new Size((int)pt.Value.X, (int)pt.Value.Y);
                        toggle.Size = size;
                    }
                    else if (val is Size es)
                        toggle.Size = es;
                    else if (val is GH_Vector vec)
                    {
                        Size size = new Size((int)vec.Value.X, (int)vec.Value.Y);
                        toggle.Size = size;
                    }
                    else if (val is GH_String sstr)
                    {
                        string[] xy = sstr.Value.Split(',');
                        bool xp = int.TryParse(xy[0], out int x);
                        bool yp = int.TryParse(xy[1], out int y);
                        if (xp && yp)
                            toggle.Size = new Size(x, y);
                    }
                    else if (val is GH_Rectangle grec)
                    {
                        int x = (int)grec.Value.X.Length;
                        int y = (int)grec.Value.Y.Length;
                        toggle.Size = new Size(x, y);
                    }
                    else if (val is GH_ComplexNumber gcomp)
                    {
                        int x = (int)gcomp.Value.Real;
                        int y = (int)gcomp.Value.Imaginary;
                        toggle.Size = new Size(x, y);
                    }
                    else
                        try { Util.SetProp(toggle, "Size", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else if (n.ToLower() == "text" || n.ToLower() == "label")
                {
                    if (val is GH_String txt)
                        toggle.Text = txt.Value;
                    else
                        toggle.Text = val.ToString();
                }
                else if (n.ToLower() == "tooltip" || n.ToLower() == "tool tip" || n.ToLower() == "hint")
                {
                    if (val is GH_String txt)
                        toggle.ToolTip = txt.Value;
                    else
                        toggle.ToolTip = val.ToString();
                }
                else if (n.ToLower() == "font" || n.ToLower() == "typeface")
                {
                    if (val is GH_String txt)
                    {
                        string fam = txt.Value;
                        try
                        {
                            Font efont = new Font(fam, (float)8.25);
                            toggle.Font = efont;
                        }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                    }
                    else if (val is Font f)
                        toggle.Font = f;
                    else
                        try { Util.SetProp(toggle, "Font", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else if (n.ToLower() == "textcolor" || n.ToLower() == "fontcolor")
                {
                    if (val is GH_Colour gclr)
                        toggle.TextColor = Color.FromArgb(gclr.Value.ToArgb());
                    else if (val is GH_String gstr)
                    {
                        if (Color.TryParse(gstr.Value, out Color clr))
                            toggle.TextColor = clr;
                    }
                    else if (val is GH_Point pt)
                    {
                        Color clr = Color.FromArgb((int)pt.Value.X, (int)pt.Value.Y, (int)pt.Value.Z);
                        toggle.TextColor = clr;
                    }
                    else if (val is GH_Vector vec)
                    {
                        Color clr = Color.FromArgb((int)vec.Value.X, (int)vec.Value.Y, (int)vec.Value.Z);
                        toggle.TextColor = clr;
                    }
                    else if (val is Color etoclr)
                        toggle.TextColor = etoclr;
                    else
                        try { Util.SetProp(toggle, "TextColor", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else if (n.ToLower() == "image")
                {
                    if (val is GH_String gstr)
                    {
                        Bitmap img = new Bitmap(gstr.Value);
                        toggle.Image = img;
                    }
                    else
                        try { Util.SetProp(toggle, "Image", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else if (n.ToLower() == "imageposition" || n.ToLower() == "imageplacement")
                {
                    if (val is GH_String gstr)
                        switch (gstr.Value.ToLower())
                        {
                            case "left":
                                toggle.ImagePosition = ButtonImagePosition.Left;
                                break;
                            case "right":
                                toggle.ImagePosition = ButtonImagePosition.Right;
                                break;
                            case "above":
                                toggle.ImagePosition = ButtonImagePosition.Above;
                                break;
                            case "below":
                                toggle.ImagePosition = ButtonImagePosition.Below;
                                break;
                            case "overlay":
                                toggle.ImagePosition = ButtonImagePosition.Overlay;
                                break;
                            case "overlap":
                                toggle.ImagePosition = ButtonImagePosition.Overlay;
                                break;
                            default:
                                toggle.ImagePosition = ButtonImagePosition.Left;
                                break;
                        }
                    else if (val is GH_Integer gint)
                        try { toggle.ImagePosition = (ButtonImagePosition)gint.Value; }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, ex.Message); }
                    else
                        try { Util.SetProp(toggle, "ImagePosition", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else
                    try { Util.SetProp(toggle, n, Util.GetGooVal(val)); }
                    catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
            }

            DA.SetData(1, new GH_ObjectWrapper(toggle));

            PropertyInfo[] allprops = toggle.GetType().GetProperties();
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

        public override GH_Exposure Exposure 
        {
            get { return GH_Exposure.hidden; }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("a0f20b58-ac54-4cc9-a615-f2ea2772e211"); }
        }
    }
}