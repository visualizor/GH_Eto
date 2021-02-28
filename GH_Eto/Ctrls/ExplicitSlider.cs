using System;
using System.Collections.Generic;
using Eto.Forms;
using Eto.Drawing;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

namespace Synapse.Ctrls
{
    public class ExplicitSlider : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ExplicitSlider class.
        /// </summary>
        public ExplicitSlider()
          : base("SynapseSlider", "SSlider",
              "slider with text label",
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

            ComboSlider csl = new ComboSlider() { ID = Guid.NewGuid().ToString(),};
            csl.Init();

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
                    if (val is GH_Integer gint)
                        csl.slider.MinValue = gint.Value;
                    else if (val is GH_Number gnum)
                        csl.slider.MinValue = (int)gnum.Value;
                    else if (val is GH_String gstr)
                        if (int.TryParse(gstr.Value, out int minint))
                            csl.slider.MinValue = minint;
                        else
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " couldn't parse minimum value string");
                    else if (val is GH_Interval gitvl)
                        csl.slider.MinValue = (int)gitvl.Value.Length;
                    else
                        try { Util.SetProp(csl.slider, "MinValue", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else if (n.ToLower() == "maxvalue" || n.ToLower() == "maximum")
                {
                    if (val is GH_Integer gint)
                        csl.slider.MaxValue = gint.Value;
                    else if (val is GH_Number gnum)
                        csl.slider.MaxValue = (int)gnum.Value;
                    else if (val is GH_String gstr)
                        if (int.TryParse(gstr.Value, out int minint))
                            csl.slider.MaxValue = minint;
                        else
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " couldn't parse minimum value string");
                    else if (val is GH_Interval gitvl)
                        csl.slider.MaxValue = (int)gitvl.Value.Length;
                    else
                        try { Util.SetProp(csl.slider, "MaxValue", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else if (n.ToLower() == "tickfrequency" || n.ToLower() == "step" || n.ToLower() =="increment")
                {
                    if (val is GH_Integer gint)
                        csl.slider.TickFrequency = gint.Value > 0 ? gint.Value : 1;
                    else if (val is GH_Number gnum)
                        csl.slider.TickFrequency = gnum.Value > 0 ? (int)gnum.Value : 1;
                    else if (val is GH_String gstr)
                    {
                        if (int.TryParse(gstr.Value, out int tickint))
                            csl.slider.TickFrequency = tickint;
                    }
                    else
                        try { Util.SetProp(csl.slider, "TickFrequency", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else if (n.ToLower()=="snaptotick" || n.ToLower()=="ticksnap" || n.ToLower() == "snap")
                {
                    if (val is GH_Boolean gbool)
                        csl.slider.SnapToTick = gbool.Value;
                    else if (val is GH_Integer gint)
                        if (gint.Value == 1)
                            csl.slider.SnapToTick = true;
                        else
                            csl.slider.SnapToTick = false;
                    else if (val is GH_String gstr)
                        if (gstr.Value.ToLower() == "true")
                            csl.slider.SnapToTick = true;
                        else if (gstr.Value.ToLower() == "false")
                            csl.slider.SnapToTick = false;
                        else
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "cannot set snap\nuse a boolean");
                    else
                        try { Util.SetProp(csl.slider, "SnapToTick", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else if (n.ToLower() == "sliderwidth" || n.ToLower() == "slider width")
                {
                    if (val is GH_Number gnum)
                        csl.slider.Width = (int)gnum.Value;
                    else if (val is GH_Integer gint)
                        csl.slider.Width = gint.Value;
                    else if (val is GH_String gstr)
                        if (int.TryParse(gstr.Value, out int wint))
                            csl.slider.Width = wint;
                        else
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " cannot parse width string\n use an integer");
                    else
                        try { Util.SetProp(csl.slider, "Width", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else if (n.ToLower() == "sliderheight" || n.ToLower() == "slider height")
                {
                    if (val is GH_Number gnum)
                        csl.slider.Height = (int)gnum.Value;
                    else if (val is GH_Integer gint)
                        csl.slider.Height = gint.Value;
                    else if (val is GH_String gstr)
                        if (int.TryParse(gstr.Value, out int wint))
                            csl.slider.Height = wint;
                        else
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " cannot parse height string\n use an integer");
                    else
                        try { Util.SetProp(csl.slider, "Height", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else if (n.ToLower() == "labelwidth" || n.ToLower() == "label width")
                {
                    if (val is GH_Number gnum)
                        csl.label.Width = (int)gnum.Value;
                    else if (val is GH_Integer gint)
                        csl.label.Width = gint.Value;
                    else if (val is GH_String gstr)
                        if (int.TryParse(gstr.Value, out int wint))
                            csl.label.Width = wint;
                        else
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " cannot parse width string\n use an integer");
                    else
                        try { Util.SetProp(csl.label, "Width", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else if (n.ToLower() == "font" || n.ToLower() == "typeface")
                {
                    if (val is GH_String txt)
                    {
                        string fam = txt.Value;
                        try
                        {
                            Font efont = new Font(fam, (float)8.25);
                            csl.label.Font = efont;
                        }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                    }
                    else if (val is Font f)
                        csl.label.Font = f;
                    else
                        try { Util.SetProp(csl.label, "Font", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else if (n.ToLower() == "textcolor" || n.ToLower() == "fontcolor")
                {
                    if (val is GH_Colour gclr)
                        csl.label.TextColor = Color.FromArgb(gclr.Value.ToArgb());
                    else if (val is GH_String gstr)
                    {
                        if (Color.TryParse(gstr.Value, out Color clr))
                            csl.label.TextColor = clr;
                    }
                    else if (val is GH_Point pt)
                    {
                        Color clr = Color.FromArgb((int)pt.Value.X, (int)pt.Value.Y, (int)pt.Value.Z);
                        csl.label.TextColor = clr;
                    }
                    else if (val is GH_Vector vec)
                    {
                        Color clr = Color.FromArgb((int)vec.Value.X, (int)vec.Value.Y, (int)vec.Value.Z);
                        csl.label.TextColor = clr;
                    }
                    else if (val is Color etoclr)
                        csl.label.TextColor = etoclr;
                    else
                        try { Util.SetProp(csl.label, "TextColor", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else if (n.ToLower() == "orientation" || n.ToLower() == "direction")
                {
                    if (val is GH_String ghstr)
                        switch (ghstr.Value.ToLower())
                        {
                            case "horizontal":
                                csl.Orientation = Orientation.Horizontal;
                                csl.slider.Orientation = Orientation.Horizontal;
                                break;
                            case "vertical":
                                csl.Orientation = Orientation.Vertical;
                                csl.slider.Orientation = Orientation.Vertical;
                                break;
                            case "h":
                                csl.Orientation = Orientation.Horizontal;
                                csl.slider.Orientation = Orientation.Horizontal;
                                break;
                            case "v":
                                csl.Orientation = Orientation.Vertical;
                                csl.slider.Orientation = Orientation.Vertical;
                                break;
                            default:
                                break;
                        }
                    else if (val is GH_Integer ghi)
                    {
                        if (ghi.Value == 0)
                        {
                            csl.Orientation = Orientation.Horizontal;
                            csl.slider.Orientation = Orientation.Horizontal;
                        }
                        else if (ghi.Value == 1)
                        {
                            csl.Orientation = Orientation.Vertical;
                            csl.slider.Orientation = Orientation.Vertical;
                        }
                    }
                    else if (val is Orientation or)
                    {
                        csl.Orientation = or;
                        csl.slider.Orientation = or;
                    }
                    else
                        try
                        {
                            Util.SetProp(csl, "Orientation", Util.GetGooVal(val));
                            Util.SetProp(csl.slider, "Orientation", Util.GetGooVal(val));
                        }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else
                    try { Util.SetProp(csl, n, Util.GetGooVal(val)); }
                    catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
            }

            DA.SetData(1, new GH_ObjectWrapper(csl));
            DA.SetDataList(0, new string[]{
                "MinValue",
                "MaxValue",
                "TickFrequency",
                "SnapToTick",
                "SliderWidth",
                "LabelWidth",
                "SliderHeight",
                "Font",
                "TextColor",
                "Width",
                "Orientation",
            });
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.slider;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("4880881b-8604-4fc5-90f0-e514af9e7c09"); }
        }
    }
}