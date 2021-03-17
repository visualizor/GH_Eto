using System;
using System.Collections.Generic;
using Eto.Forms;
using Eto.Drawing;
using System.Reflection;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

namespace Synapse
{
    public class EtoTableLO : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the EtoTableLO class.
        /// </summary>
        public EtoTableLO()
          : base("SynapseTable", "STable",
              "table layout of synapse controls",
              "Synapse", "Containers")
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
            pManager.AddGenericParameter("Controls", "C", "controls to go into this container\nprovide a position indices(L) for each item", GH_ParamAccess.list);
            pManager[2].DataMapping = GH_DataMapping.Flatten;
            pManager[2].Optional = true;
            pManager.AddTextParameter("ControlLocation", "L", "zero-index location coordinates such as \"0,2\" meaning putting control on the first column third row\nuse a text string", GH_ParamAccess.list);
            pManager[3].DataMapping = GH_DataMapping.Flatten;
            pManager[3].Optional = true;
            pManager.AddGenericParameter("TableDimension", "D", "number of columns and row as x,y\nsuch as a text string \"2,3\"\nor a point object whose x and y coordinates are read", GH_ParamAccess.item);
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
            GH_ObjectWrapper sizeobj = null;
            List<GH_ObjectWrapper> ctrls = new List<GH_ObjectWrapper>();
            List<string> locs = new List<string>();
            DA.GetDataList(0, props);
            DA.GetDataList(1, vals);
            DA.GetData(4, ref sizeobj);
            DA.GetDataList(2, ctrls);
            DA.GetDataList(3, locs);

            // initialize with dimensions
            TableLayout table;
            if (sizeobj.Value is GH_Point gpt)
                table = new TableLayout(new Size((int)gpt.Value.X, (int)gpt.Value.Y));
            else if (sizeobj.Value is GH_String gstr)
            {
                string[] xy = gstr.Value.Split(',');
                try { table = new TableLayout(new Size(int.Parse(xy[0]), int.Parse(xy[1]))); }
                catch (Exception ex)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, ex.Message);
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, " likely a bad format string input, try something like \"2,3\"");
                    return;
                }
            }
            else if (sizeobj.Value is GH_Vector gvec)
                table = new TableLayout(new Size((int)gvec.Value.X, (int)gvec.Value.Y));
            else if (sizeobj.Value is GH_Rectangle grec)
            {
                int x = (int)grec.Value.X.Length;
                int y = (int)grec.Value.Y.Length;
                table = new TableLayout(new Size(x, y));
            }
            else if (sizeobj.Value is Size s)
                table = new TableLayout(s);
            else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, " Dimesion cannot be understood\n Try something like\"2,3\" as a text string");
                return;
            }

            //set props
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
                        Size winsize = new Size((int)pt.Value.X, (int)pt.Value.Y);
                        table.Size = winsize;
                    }
                    else if (val is GH_Vector vec)
                    {
                        Size winsize = new Size((int)vec.Value.X, (int)vec.Value.Y);
                        table.Size = winsize;
                    }
                    else if (val is GH_String sstr)
                    {
                        string[] xy = sstr.Value.Split(',');
                        bool xp = int.TryParse(xy[0], out int x);
                        bool yp = int.TryParse(xy[1], out int y);
                        if (xp && yp)
                            table.Size = new Size(x, y);
                        else
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " text cannot be parsed as Size object");
                    }
                    else if (val is GH_Rectangle grec)
                    {
                        int x = (int)grec.Value.X.Length;
                        int y = (int)grec.Value.Y.Length;
                        table.Size = new Size(x, y);
                    }
                    else if (val is GH_ComplexNumber gcomp)
                    {
                        int x = (int)gcomp.Value.Real;
                        int y = (int)gcomp.Value.Imaginary;
                        table.Size = new Size(x, y);
                    }
                    else
                        try { Util.SetProp(table, "Size", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else if (n.ToLower() == "padding")
                {
                    if (val is GH_Integer ghi)
                        table.Padding = ghi.Value;
                    else if (val is GH_String gstr)
                    {
                        if (int.TryParse(gstr.Value, out int v))
                            table.Padding = v;
                        else if (gstr.Value.Split(',') is string[] xy)
                        {
                            if (xy.Length == 2)
                            {
                                bool i0 = int.TryParse(xy[0], out int n0);
                                bool i1 = int.TryParse(xy[1], out int n1);
                                if (i0 && i1)
                                    table.Padding = new Padding(n0, n1);
                                else
                                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " text cannot be parsed as Padding object");
                            }
                            else if (xy.Length == 4)
                            {
                                bool i0 = int.TryParse(xy[0], out int n0);
                                bool i1 = int.TryParse(xy[1], out int n1);
                                bool i2 = int.TryParse(xy[2], out int n2);
                                bool i3 = int.TryParse(xy[3], out int n3);
                                if (i0 && i1 && i2 && i3)
                                    table.Padding = new Padding(n0, n1, n2, n3);
                                else
                                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " text cannot be parsed as Padding object");
                            }
                        }
                    }
                    else if (val is GH_Number gnum)
                        table.Padding = (int)gnum.Value;
                    else if (val is GH_Point pt)
                        table.Padding = new Padding((int)pt.Value.X, (int)pt.Value.Y);
                    else if (val is GH_Vector vec)
                        table.Padding = new Padding((int)vec.Value.X, (int)vec.Value.Y);
                    else if (val is GH_Rectangle grec)
                    {
                        int x = (int)grec.Value.X.Length;
                        int y = (int)grec.Value.Y.Length;
                        table.Padding = new Padding(x, y);
                    }
                    else if (val is GH_ComplexNumber gcomp)
                    {
                        int x = (int)gcomp.Value.Real;
                        int y = (int)gcomp.Value.Imaginary;
                        table.Padding = new Padding(x, y);
                    }
                    else
                        try { Util.SetProp(table, "Padding", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else if (n.ToLower() == "spacing")
                {
                    if (val is GH_Integer ghi)
                        table.Spacing = new Size(ghi.Value, ghi.Value);
                    else if (val is GH_String ghstr)
                    {
                        if (int.TryParse(ghstr.Value, out int v))
                            table.Spacing = new Size(v, v);
                        else if (ghstr.Value.Split(',') is string[] xy)
                            if (int.TryParse(xy[0], out int x) && int.TryParse(xy[1], out int y))
                                table.Spacing = new Size(x, y);
                            else
                                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " cannot parse text string into Size object");
                        else
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " cannot parse text string into Size object");
                    }
                    else if (val is GH_Number gnum)
                        table.Spacing = new Size((int)gnum.Value, (int)gnum.Value);
                    else if (val is GH_Rectangle grec)
                    {
                        int x = (int)grec.Value.X.Length;
                        int y = (int)grec.Value.Y.Length;
                        table.Spacing = new Size(x, y);
                    }
                    else if (val is GH_ComplexNumber gcomp)
                    {
                        int x = (int)gcomp.Value.Real;
                        int y = (int)gcomp.Value.Imaginary;
                        table.Spacing = new Size(x, y);
                    }
                    else
                        try { Util.SetProp(table, "Spacing", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else
                    try { Util.SetProp(table, n, Util.GetGooVal(val)); }
                    catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
            }

            // fill table
            for (int ci = 0; ci < ctrls.Count; ci++)
            {
                if (ctrls[ci].Value is Control c)
                {
                    string loc = locs[ci];
                    string[] coor = loc.Split(',');
                    try
                    {
                        int x = int.Parse(coor[0]);
                        int y = int.Parse(coor[1]);
                        table.Add(c, x, y);
                    }
                    catch (Exception ex)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, ex.Message);
                        continue;
                    }
                }
                else
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " one or more object cannot be added\n are they non-Synapse components?");
            }
            DA.SetData(1, new GH_ObjectWrapper(table));

            PropertyInfo[] allprops = table.GetType().GetProperties();
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
                return Properties.Resources.table;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("3539ebc6-f995-42ed-be41-f57f7136bb6d"); }
        }
    }
}