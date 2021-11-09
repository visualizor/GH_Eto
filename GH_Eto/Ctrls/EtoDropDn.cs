using System;
using System.Collections.Generic;
using Eto.Forms;
using Eto.Drawing;
using wf = System.Windows.Forms;
using System.Reflection;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

namespace Synapse
{
    public class EtoDropDn : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the EtoDropDn class.
        /// </summary>
        public EtoDropDn()
          : base("SnpDropMenu", "SDM",
              "dropdown menu",
              "Synapse", "Controls")
        {
        }

        public override void AppendAdditionalMenuItems(wf.ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);
            wf.ToolStripMenuItem click = menu.Items.Add("List Properties", null, Util.OnListProps) as wf.ToolStripMenuItem;
            click.ToolTipText = "put all properties of this control in a check list";
            Util.ListPropLoc = Attributes.Pivot;
            DropDown dummy = new DropDown();
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

            DropDown dd = new DropDown() { ID = Guid.NewGuid().ToString(), };
            foreach (string opt in opts)
                dd.Items.Add(opt);

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

                if (n.ToLower() == "textcolor" || n.ToLower() == "fontcolor")
                {
                    if (val is GH_Colour gclr)
                        dd.TextColor = Color.FromArgb(gclr.Value.ToArgb());
                    else if (val is GH_String gstr)
                    {
                        if (Color.TryParse(gstr.Value, out Color clr))
                            dd.TextColor = clr;
                    }
                    else if (val is GH_Point pt)
                    {
                        Color clr = Color.FromArgb((int)pt.Value.X, (int)pt.Value.Y, (int)pt.Value.Z);
                        dd.TextColor = clr;
                    }
                    else if (val is GH_Vector vec)
                    {
                        Color clr = Color.FromArgb((int)vec.Value.X, (int)vec.Value.Y, (int)vec.Value.Z);
                        dd.TextColor = clr;
                    }
                    else if (val is Color etoclr)
                        dd.TextColor = etoclr;
                    else
                        try { Util.SetProp(dd, "TextColor", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else if (n.ToLower() == "showborder" || n.ToLower() == "border")
                {
                    if (val is GH_Boolean gbool)
                        dd.ShowBorder = gbool.Value;
                    else if (val is GH_Integer gint)
                        if (gint.Value == 0)
                            dd.ShowBorder = false;
                        else if (gint.Value == 1)
                            dd.ShowBorder = true;
                        else
                            dd.ShowBorder = false;
                    else if (val is GH_String gstr)
                        if (gstr.Value.ToLower() == "true")
                            dd.ShowBorder = true;
                        else if (gstr.Value.ToLower() == "false")
                            dd.ShowBorder = false;
                        else
                            dd.ShowBorder = false;
                    else
                        try { Util.SetProp(dd, "ShowBorder" +
                            "", Util.GetGooVal(val)); }
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
                            dd.Font = efont;
                        }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                    }
                    else if (val is Font f)
                        dd.Font = f;
                    else
                        try { Util.SetProp(dd, "Font", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else
                    try { Util.SetProp(dd, n, Util.GetGooVal(val)); }
                    catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
            }

            DA.SetData(1, new GH_ObjectWrapper(dd));

            PropertyInfo[] allprops = dd.GetType().GetProperties();
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

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.dd;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("272234e0-d839-4517-b2fd-5db5e2f17dea"); }
        }
    }
}