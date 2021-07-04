using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Eto.Forms;
using Eto.Drawing;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

namespace Synapse
{
    public class EtoDynamicLO : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the EtoDynamicLO class.
        /// </summary>
        public EtoDynamicLO()
          : base("SynapseDynamicLayout", "SDyLO",
              "dynamic layout, somewhat autosized to controls",
              "Synapse", "Containers")
        {
        }

        protected bool hz = false;

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

            DynamicLayout dyna = new DynamicLayout();

            List<GH_ObjectWrapper> valids = ctrls.FindAll(c => c.Value is Control ctrl);
            Control[] added = valids.Select(c => c.Value as Control).ToArray();

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
                        dyna.Size = winsize;
                    }
                    else if (val is GH_Vector vec)
                    {
                        Size winsize = new Size((int)vec.Value.X, (int)vec.Value.Y);
                        dyna.Size = winsize;
                    }
                    else if (val is GH_String sstr)
                    {
                        string[] xy = sstr.Value.Split(',');
                        bool xp = int.TryParse(xy[0], out int x);
                        bool yp = int.TryParse(xy[1], out int y);
                        if (xp && yp)
                            dyna.Size = new Size(x, y);
                        else
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " text cannot be parsed as Size object");
                    }
                    else if (val is GH_Rectangle grec)
                    {
                        int x = (int)grec.Value.X.Length;
                        int y = (int)grec.Value.Y.Length;
                        dyna.Size = new Size(x, y);
                    }
                    else if (val is GH_ComplexNumber gcomp)
                    {
                        int x = (int)gcomp.Value.Real;
                        int y = (int)gcomp.Value.Imaginary;
                        dyna.Size = new Size(x, y);
                    }
                    else
                        try { Util.SetProp(dyna, "Size", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else if (n.ToLower() == "padding")
                {
                    if (val is GH_Integer ghi)
                        dyna.Padding = ghi.Value;
                    else if (val is GH_String gstr)
                    {
                        if (int.TryParse(gstr.Value, out int v))
                            dyna.Padding = v;
                        else if (gstr.Value.Split(',') is string[] xy)
                        {
                            if (xy.Length == 2)
                            {
                                bool i0 = int.TryParse(xy[0], out int n0);
                                bool i1 = int.TryParse(xy[1], out int n1);
                                if (i0 && i1)
                                    dyna.Padding = new Padding(n0, n1);
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
                                    dyna.Padding = new Padding(n0, n1, n2, n3);
                                else
                                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " text cannot be parsed as Padding object");
                            }
                        }
                    }
                    else if (val is GH_Point pt)
                        dyna.Padding = new Padding((int)pt.Value.X, (int)pt.Value.Y);
                    else if (val is GH_Vector vec)
                        dyna.Padding = new Padding((int)vec.Value.X, (int)vec.Value.Y);
                    else if (val is GH_Rectangle grec)
                    {
                        int x = (int)grec.Value.X.Length;
                        int y = (int)grec.Value.Y.Length;
                        dyna.Padding = new Padding(x, y);
                    }
                    else if (val is GH_ComplexNumber gcomp)
                    {
                        int x = (int)gcomp.Value.Real;
                        int y = (int)gcomp.Value.Imaginary;
                        dyna.Padding = new Padding(x, y);
                    }
                    else
                        try { Util.SetProp(dyna, "Padding", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else if (n.ToLower() == "spacing")
                {
                    if (val is GH_Integer ghi)
                        dyna.Spacing = new Size(ghi.Value, ghi.Value);
                    else if (val is GH_String ghstr)
                    {
                        if (int.TryParse(ghstr.Value, out int v))
                            dyna.Spacing = new Size(v, v);
                        else if (ghstr.Value.Split(',') is string[] xy)
                            if (int.TryParse(xy[0], out int x) && int.TryParse(xy[1], out int y))
                                dyna.Spacing = new Size(x, y);
                            else
                                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " cannot parse text string into Size object");
                        else
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " cannot parse text string into Size object");
                    }
                    else if (val is GH_Number gnum)
                        dyna.Spacing = new Size((int)gnum.Value, (int)gnum.Value);
                    else if (val is GH_Rectangle grec)
                    {
                        int x = (int)grec.Value.X.Length;
                        int y = (int)grec.Value.Y.Length;
                        dyna.Spacing = new Size(x, y);
                    }
                    else if (val is GH_ComplexNumber gcomp)
                    {
                        int x = (int)gcomp.Value.Real;
                        int y = (int)gcomp.Value.Imaginary;
                        dyna.Spacing = new Size(x, y);
                    }
                    else
                        try { Util.SetProp(dyna, "Spacing", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else if (n.ToLower() == "direction" || n.ToLower() == "flow")
                {
                    if (val is GH_Boolean gb)
                        hz = gb.Value;
                    else if (val is GH_Integer gint)
                        if (gint.Value == 0)
                            hz = false;
                        else if (gint.Value == 1)
                            hz = true;
                        else
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " flow direction cannot be set\n check input parameter");
                    else if (val is GH_String gstr)
                        if (gstr.Value.ToLower() == "horizontal" || gstr.Value.ToLower() == "true")
                            hz = true;
                        else if (gstr.Value.ToLower() == "vertical" || gstr.Value.ToLower() == "false")
                            hz = false;
                        else
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " flow direction cannot be set\n check input parameter");
                    else
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " flow direction cannot be set\n check input parameter");
                }
                else
                    try { Util.SetProp(dyna, n, Util.GetGooVal(val)); }
                    catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
            }

            //dyna.AddRange(added);
            if (!hz)
                foreach (Control c in added)
                    dyna.AddAutoSized(c);
            else
            {
                dyna.BeginHorizontal();
                foreach (Control c in added)
                {
                    dyna.BeginHorizontal();
                    dyna.BeginVertical();
                    dyna.AddAutoSized(c);
                    dyna.EndVertical();
                    dyna.EndHorizontal();
                }
                dyna.EndHorizontal();
            }
                

            //TODO: currently not able to add separate columns i.e. flow horizontally

            DA.SetData(1, new GH_ObjectWrapper(dyna));

            PropertyInfo[] allprops = dyna.GetType().GetProperties();
            List<string> printouts = new List<string>();
            foreach (PropertyInfo prop in allprops)
                if (prop.CanWrite)
                    printouts.Add(prop.Name + ": " + prop.PropertyType.ToString());
            printouts.Add("Flow: set boolean to true to flow horizontally, false vertically");
            DA.SetDataList(0, printouts);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.dynam;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("7218eb46-e372-4a66-94a1-f7456f007694"); }
        }
    }
}