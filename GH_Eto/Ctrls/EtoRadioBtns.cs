using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Eto.Forms;
using Eto.Drawing;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

namespace Synapse.Ctrls
{
    public class EtoRadioBtns : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the EtoRadioBtns class.
        /// </summary>
        public EtoRadioBtns()
          : base("SynapseSingleSelect", "PickOne",
              "radio buttons",
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
            pManager.AddTextParameter("Options", "O", "list of options from which a user can pick one\nvalue query will return the index of selected item so pay attention to list order", GH_ParamAccess.list, new string[] { "Alpha", "Beta", "Omega", });
            pManager[2].DataMapping = GH_DataMapping.Flatten;
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
            List<string> opts = new List<string>();
            DA.GetDataList(0, props);
            DA.GetDataList(1, vals);
            DA.GetDataList(2, opts);

            RadioButtonList rblist = new RadioButtonList()
            {
                ID = Guid.NewGuid().ToString(),
                Spacing = new Size(5, 5),
                Orientation = Orientation.Vertical,
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

                if (n.ToLower() == "orientation")
                {
                    if (val is GH_String ghstr)
                        switch (ghstr.Value.ToLower())
                        {
                            case "horizontal":
                                rblist.Orientation = Orientation.Horizontal;
                                break;
                            case "vertical":
                                rblist.Orientation = Orientation.Vertical;
                                break;
                            case "h":
                                rblist.Orientation = Orientation.Horizontal;
                                break;
                            case "v":
                                rblist.Orientation = Orientation.Vertical;
                                break;
                            default:
                                break;
                        }
                    else if (val is GH_Integer ghi)
                    {
                        if (ghi.Value == 0)
                            rblist.Orientation = Orientation.Horizontal;
                        else if (ghi.Value == 1)
                            rblist.Orientation = Orientation.Vertical;
                        else
                            rblist.Orientation = Orientation.Vertical;
                    }
                    else if (val is GH_Boolean gbool)
                        if (gbool.Value)
                            rblist.Orientation = Orientation.Vertical;
                        else
                            rblist.Orientation = Orientation.Horizontal;
                    else if (val is Orientation or)
                        rblist.Orientation = or;
                    else
                        try { Util.SetProp(rblist, "Orientation", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else if (n.ToLower() == "spacing")
                {
                    if (val is GH_Integer ghi)
                        rblist.Spacing = new Size(ghi.Value, ghi.Value);
                    else if (val is GH_String ghstr)
                    {
                        if (ghstr.Value.Contains(","))
                        {
                            string[] xy = ghstr.Value.Split(',');
                            try
                            {
                                int x = int.Parse(xy[0]);
                                int y = int.Parse(xy[1]);
                                rblist.Spacing = new Size(x, y);
                            }
                            catch (Exception ex)
                            {
                                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, ex.Message);
                            }
                        }
                        else if (int.TryParse(ghstr.Value, out int v))
                            rblist.Spacing = new Size(v, v);
                    }
                    else
                        try { Util.SetProp(rblist, "Spacing", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else if (n.ToLower() == "textcolor" || n.ToLower() == "fontcolor")
                {
                    if (val is GH_Colour gclr)
                        rblist.TextColor = Color.FromArgb(gclr.Value.ToArgb());
                    else if (val is GH_String gstr)
                    {
                        if (Color.TryParse(gstr.Value, out Color clr))
                            rblist.TextColor = clr;
                    }
                    else if (val is GH_Point pt)
                    {
                        Color clr = Color.FromArgb((int)pt.Value.X, (int)pt.Value.Y, (int)pt.Value.Z);
                        rblist.TextColor = clr;
                    }
                    else if (val is GH_Vector vec)
                    {
                        Color clr = Color.FromArgb((int)vec.Value.X, (int)vec.Value.Y, (int)vec.Value.Z);
                        rblist.TextColor = clr;
                    }
                    else if (val is Color etoclr)
                        rblist.TextColor = etoclr;
                    else
                        try { Util.SetProp(rblist, "TextColor", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else
                    try { Util.SetProp(rblist, n, Util.GetGooVal(val)); }
                    catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
            }
            foreach (string opt in opts)
                rblist.Items.Add(opt);

            DA.SetData(1, new GH_ObjectWrapper(rblist));

            PropertyInfo[] allprops = rblist.GetType().GetProperties();
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
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("9c95eb54-c5bb-4ad0-859b-42ab104bfa59"); }
        }
    }
}