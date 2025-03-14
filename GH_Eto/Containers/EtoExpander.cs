﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Eto.Forms;
using Eto.Drawing;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using wf = System.Windows.Forms;
using GH_IO.Serialization;

namespace Synapse
{
    public class EtoExpander : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the EtoExpander class.
        /// </summary>
        public EtoExpander()
          : base("SnpExpander", "SXpdr",
              "expand/collapse controls",
              "Synapse", "Containers")
        {
        }

        protected bool stretchy = false;
        protected void OnStretch(object s, EventArgs e)
        {
            stretchy = !stretchy;
            ExpireSolution(true);
        }

        public override void AppendAdditionalMenuItems(wf.ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);
            try
            {
                menu.Items.Add(stretchy ? "Fix control sizes" : "Flex control sizes", null, OnStretch);
                wf.ToolStripMenuItem click = menu.Items.Add("List Properties", null, Util.OnListProps) as wf.ToolStripMenuItem;
                click.ToolTipText = "put all properties of this control in a check list";
            }
            catch (NullReferenceException) { }
            Util.ListPropLoc = Attributes.Pivot;
            Expander dummy = new Expander();
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
            List<GH_ObjectWrapper> ctrls = new List<GH_ObjectWrapper>();
            DA.GetDataList(0, props);
            DA.GetDataList(1, vals);
            DA.GetDataList(2, ctrls);
            Message = stretchy ? "ResizeCtrl" : "FixedCtrl";

            Expander xpdr = new Expander() { ID = Guid.NewGuid().ToString() ,};

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
                        xpdr.Size = winsize;
                    }
                    else if (val is GH_Vector vec)
                    {
                        Size winsize = new Size((int)vec.Value.X, (int)vec.Value.Y);
                        xpdr.Size = winsize;
                    }
                    else if (val is GH_String sstr)
                    {
                        string[] xy = sstr.Value.Split(',');
                        bool xp = int.TryParse(xy[0], out int x);
                        bool yp = int.TryParse(xy[1], out int y);
                        if (xp && yp)
                            xpdr.Size = new Size(x, y);
                        else
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " text cannot be parsed as Size object");
                    }
                    else if (val is GH_Rectangle grec)
                    {
                        int x = (int)grec.Value.X.Length;
                        int y = (int)grec.Value.Y.Length;
                        xpdr.Size = new Size(x, y);
                    }
                    else if (val is GH_ComplexNumber gcomp)
                    {
                        int x = (int)gcomp.Value.Real;
                        int y = (int)gcomp.Value.Imaginary;
                        xpdr.Size = new Size(x, y);
                    }
                    else
                        try { Util.SetProp(xpdr, "Size", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else if (n.ToLower() == "padding")
                {
                    if (val is GH_Integer ghi)
                        xpdr.Padding = ghi.Value;
                    else if (val is GH_String gstr)
                    {
                        if (int.TryParse(gstr.Value, out int v))
                            xpdr.Padding = v;
                        else if (gstr.Value.Split(',') is string[] xy)
                        {
                            if (xy.Length == 2)
                            {
                                bool i0 = int.TryParse(xy[0], out int n0);
                                bool i1 = int.TryParse(xy[1], out int n1);
                                if (i0 && i1)
                                    xpdr.Padding = new Padding(n0, n1);
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
                                    xpdr.Padding = new Padding(n0, n1, n2, n3);
                                else
                                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " text cannot be parsed as Padding object");
                            }
                        }
                    }
                    else if (val is GH_Number gnum)
                        xpdr.Padding = (int)gnum.Value;
                    else if (val is GH_Point pt)
                        xpdr.Padding = new Padding((int)pt.Value.X, (int)pt.Value.Y);
                    else if (val is GH_Vector vec)
                        xpdr.Padding = new Padding((int)vec.Value.X, (int)vec.Value.Y);
                    else if (val is GH_Rectangle grec)
                    {
                        int x = (int)grec.Value.X.Length;
                        int y = (int)grec.Value.Y.Length;
                        xpdr.Padding = new Padding(x, y);
                    }
                    else if (val is GH_ComplexNumber gcomp)
                    {
                        int x = (int)gcomp.Value.Real;
                        int y = (int)gcomp.Value.Imaginary;
                        xpdr.Padding = new Padding(x, y);
                    }
                    else
                        try { Util.SetProp(xpdr, "Padding", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else
                    try { Util.SetProp(xpdr, n, Util.GetGooVal(val)); }
                    catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
            }

            DynamicLayout content = new DynamicLayout() { DefaultPadding = 1, };
            foreach (GH_ObjectWrapper ghobj in ctrls)
                if (ghobj.Value is Control ctrl)
                    content.AddAutoSized(ctrl,xscale:stretchy, yscale:stretchy);
                else
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " one or more object cannot be added\n are they non-Synapse components?");
            xpdr.Content = content;

            DA.SetData(1, new GH_ObjectWrapper(xpdr));

            PropertyInfo[] allprops = xpdr.GetType().GetProperties();
            List<string> printouts = new List<string>();
            foreach (PropertyInfo prop in allprops)
                if (prop.CanWrite)
                    printouts.Add(prop.Name + ": " + prop.PropertyType.ToString());
            DA.SetDataList(0, printouts);
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetBoolean("stretchy", stretchy);
            return base.Write(writer);
        }
        public override bool Read(GH_IReader reader)
        {
            reader.TryGetBoolean("stretchy", ref stretchy);
            return base.Read(reader);
        }


        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.expander;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("3D4DB0AE-8C94-42DD-860D-D111C623FEA4"); }
        }
    }
}