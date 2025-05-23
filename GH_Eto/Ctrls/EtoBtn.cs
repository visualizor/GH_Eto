﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Eto.Forms;
using Eto.Drawing;

using Grasshopper.Kernel;
using Grasshopper;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Special;
using wf = System.Windows.Forms;
using Rhino;
using GH_IO.Serialization;

namespace Synapse.Ctrls
{
    public class EtoBtn : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the EtoBtn class.
        /// </summary>
        public EtoBtn()
          : base("SnpButton", "SBtn",
              "button, true on click, false on release",
              "Synapse", "Controls")
        {
        }

        internal bool trueonly = true;

        public override void AppendAdditionalMenuItems(wf.ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);
            try
            {
                wf.ToolStripMenuItem click = menu.Items.Add("List Properties", null, Util.OnListProps) as wf.ToolStripMenuItem;
                wf.ToolStripMenuItem to = menu.Items.Add("TrueOnly", null, OnTO) as wf.ToolStripMenuItem;
                to.Checked = trueonly;
                to.ToolTipText = !trueonly ? "check to only fire a true after clicking" : "uncheck to get true while pressed, false after released";
                click.ToolTipText = "put all properties of this control in a check list";
                
            }
            catch (NullReferenceException)
            {
                
            }
            Util.ListPropLoc = Attributes.Pivot;
            Button dummy = new Button();
            Util.ListPropType = dummy.GetType();
            dummy.Dispose();
        }

        protected void OnTO(object s, EventArgs e)
        {
            trueonly = !trueonly;
            ExpireSolution(true);
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

            Button btn = new Button() 
            {
                Text = "a button", 
                ID = Guid.NewGuid().ToString() ,
                Tag = trueonly,
            };
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
                        btn.Size = size;
                    }
                    else if (val is Size es)
                        btn.Size = es;
                    else if (val is GH_Vector vec)
                    {
                        Size size = new Size((int)vec.Value.X, (int)vec.Value.Y);
                        btn.Size = size;
                    }
                    else if (val is GH_String sstr)
                    {
                        string[] xy = sstr.Value.Split(',');
                        bool xp = int.TryParse(xy[0], out int x);
                        bool yp = int.TryParse(xy[1], out int y);
                        if (xp && yp)
                            btn.Size = new Size(x, y);
                    }
                    else if (val is GH_Rectangle grec)
                    {
                        int x = (int)grec.Value.X.Length;
                        int y = (int)grec.Value.Y.Length;
                        btn.Size = new Size(x, y);
                    }
                    else if (val is GH_ComplexNumber gcomp)
                    {
                        int x = (int)gcomp.Value.Real;
                        int y = (int)gcomp.Value.Imaginary;
                        btn.Size = new Size(x, y);
                    }
                    else
                        try { Util.SetProp(btn, "Size", Util.GetGooVal(val)); }
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
                else if (n.ToLower() == "textcolor" || n.ToLower() == "fontcolor")
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
                else if (n.ToLower() == "imageposition" || n.ToLower() == "imageplacement")
                {
                    if (val is GH_String gstr)
                        switch (gstr.Value.ToLower())
                        {
                            case "left":
                                btn.ImagePosition = ButtonImagePosition.Left;
                                break;
                            case "right":
                                btn.ImagePosition = ButtonImagePosition.Right;
                                break;
                            case "above":
                                btn.ImagePosition = ButtonImagePosition.Above;
                                break;
                            case "below":
                                btn.ImagePosition = ButtonImagePosition.Below;
                                break;
                            case "overlay":
                                btn.ImagePosition = ButtonImagePosition.Overlay;
                                break;
                            case "overlap":
                                btn.ImagePosition = ButtonImagePosition.Overlay;
                                break;
                            default:
                                btn.ImagePosition = ButtonImagePosition.Left;
                                break;
                        }
                    else if (val is GH_Integer gint)
                        try { btn.ImagePosition = (ButtonImagePosition)gint.Value; }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, ex.Message); }
                    else
                        try { Util.SetProp(btn, "ImagePosition", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else
                    try { Util.SetProp(btn, n, Util.GetGooVal(val)); }
                    catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
            }

            Message = trueonly ? "TrueOnly" : "";

            DA.SetData(1, new GH_ObjectWrapper(btn));

            PropertyInfo[] allprops = btn.GetType().GetProperties();
            List<string> printouts = new List<string>();
            foreach (PropertyInfo prop in allprops)
                if (prop.CanWrite)
                    printouts.Add(prop.Name + ": " + prop.PropertyType.ToString());
            DA.SetDataList(0, printouts);
        }


        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.secondary; }
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetBoolean("trueonly", trueonly);
            return base.Write(writer);
        }
        public override bool Read(GH_IReader reader)
        {
            reader.TryGetBoolean("trueonly", ref trueonly);
            return base.Read(reader);
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
            get { return new Guid("22691AC4-A76B-4CBA-A02D-5DD4CB5D06C5"); }
        }
    }
}