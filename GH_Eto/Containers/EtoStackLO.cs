using System;
using System.Collections.Generic;
using System.Reflection;
using Eto.Forms;
using Eto.Drawing;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Data;

namespace Synapse
{
    public class EtoStackLO : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the EtoStackLO class.
        /// </summary>
        public EtoStackLO()
          : base("SnpStack", "SStack",
              "stack layout",
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

            StackLayout stack = new StackLayout() { Size = new Size(60,40), };
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
                
                if (n.ToLower() == "spacing")
                {
                    if (val is GH_Integer ghi)
                        stack.Spacing = ghi.Value;
                    else if (val is GH_String ghstr)
                    {
                        if (int.TryParse(ghstr.Value, out int v))
                            stack.Spacing = v;
                    }
                    else
                        try { Util.SetProp(stack, "Spacing", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else if (n.ToLower() == "orientation")
                {
                    if (val is GH_String ghstr)
                        switch (ghstr.Value.ToLower())
                        {
                            case "horizontal":
                                stack.Orientation = Orientation.Horizontal;
                                break;
                            case "vertical":
                                stack.Orientation = Orientation.Vertical;
                                break;
                            case "h":
                                stack.Orientation = Orientation.Horizontal;
                                break;
                            case "v":
                                stack.Orientation = Orientation.Vertical;
                                break;
                            default:
                                break;
                        }
                    else if (val is GH_Integer ghi)
                    {
                        if (ghi.Value == 0)
                            stack.Orientation = Orientation.Horizontal;
                        else if (ghi.Value == 1)
                            stack.Orientation = Orientation.Vertical;
                    }
                    else if (val is Orientation or)
                        stack.Orientation = or;
                    else
                        try { Util.SetProp(stack, "Orientation", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else if (n.ToLower() == "padding")
                {
                    if (val is GH_Integer ghi)
                        stack.Padding = new Padding(ghi.Value);
                    else if (val is GH_Number ghn)
                        stack.Padding = new Padding((int)ghn.Value);
                    else if (val is GH_String ghstr)
                    {
                        string s = ghstr.Value;
                        if (!s.Contains(","))
                            try { Util.SetProp(stack, "Padding", Util.GetGooVal(val)); }
                            catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                        else
                        {
                            string[] nums = s.Split(',');
                            if (nums.Length == 2)
                            {
                                bool a = int.TryParse(nums[0], out int na);
                                bool b = int.TryParse(nums[1], out int nb);
                                if (a && b)
                                    stack.Padding = new Padding(na, nb);
                            }
                            else if (nums.Length == 4)
                            {
                                bool a = int.TryParse(nums[0], out int na);
                                bool b = int.TryParse(nums[1], out int nb);
                                bool c = int.TryParse(nums[2], out int nc);
                                bool d = int.TryParse(nums[3], out int nd);
                                if (a && b && c && d)
                                    stack.Padding = new Padding(na, nb, nc, nd);
                            }
                            else
                                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " couldn't parse padding integers");
                        }
                    }
                    else if (val is Padding pad)
                        stack.Padding = pad;
                    else if (val is GH_Rectangle grec)
                    {
                        int x = (int)grec.Value.X.Length;
                        int y = (int)grec.Value.Y.Length;
                        stack.Padding = new Padding(x, y);
                    }
                    else if (val is GH_ComplexNumber gcomp)
                    {
                        int x = (int)gcomp.Value.Real;
                        int y = (int)gcomp.Value.Imaginary;
                        stack.Padding = new Padding(x, y);
                    }
                    else
                        try { Util.SetProp(stack, "Padding", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else if (n.ToLower() == "size")
                {
                    if (val is GH_Point pt)
                    {
                        Size size = new Size((int)pt.Value.X, (int)pt.Value.Y);
                        stack.Size = size;
                    }
                    else if (val is GH_Vector vec)
                    {
                        Size size = new Size((int)vec.Value.X, (int)vec.Value.Y);
                        stack.Size = size;
                    }
                    else if (val is GH_String sstr)
                    {
                        string[] xy = sstr.Value.Split(',');
                        bool xp = int.TryParse(xy[0], out int x);
                        bool yp = int.TryParse(xy[1], out int y);
                        if (xp && yp)
                            stack.Size = new Size(x, y);
                    }
                    else if (val is GH_Rectangle grec)
                    {
                        int x = (int)grec.Value.X.Length;
                        int y = (int)grec.Value.Y.Length;
                        stack.Size = new Size(x, y);
                    }
                    else if (val is GH_ComplexNumber gcomp)
                    {
                        int x = (int)gcomp.Value.Real;
                        int y = (int)gcomp.Value.Imaginary;
                        stack.Size = new Size(x, y);
                    }
                    else
                        try { Util.SetProp(stack, "Size", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else
                    try { Util.SetProp(stack, n, Util.GetGooVal(val)); }
                    catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
            }

            foreach (GH_ObjectWrapper wrapped in ctrls)
                if (wrapped.Value is Control ctrl)
                    stack.Items.Add(ctrl);
                else
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " one or more object cannot be added\n are they non-Synapse components?");

            DA.SetData(1, new GH_ObjectWrapper(stack));

            PropertyInfo[] allprops = stack.GetType().GetProperties();
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
                return Properties.Resources.stack;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("62648c00-6af9-41a1-ab56-e3dc59541fe4"); }
        }
    }
}