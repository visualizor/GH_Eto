using System;
using System.Collections.Generic;
using System.Reflection;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Eto.Forms;
using Eto.Drawing;

namespace Synapse
{
    public class EtoColorPicker : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the EtoColorPicker class.
        /// </summary>
        public EtoColorPicker()
          : base("SnpColorPicker", "SPickClr",
              "Synapse color picker control",
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

            ColorPicker picker = new ColorPicker() { ID = Guid.NewGuid().ToString(), };

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

                if (n.ToLower() == "color" || n.ToLower() == "value")
                {
                    if (val is GH_String gs)
                        if (Color.TryParse(gs.Value, out Color cl))
                            picker.Value = cl;
                        else
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " not a valid string representation for a color");
                    else if (val is GH_Colour gc)
                        picker.Value = Color.FromArgb(gc.Value.ToArgb());
                    else
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " not a valid color value");
                }
                else if (n.ToLower() == "size")
                {
                    if (val is GH_Point pt)
                    {
                        Size size = new Size((int)pt.Value.X, (int)pt.Value.Y);
                        picker.Size = size;
                    }
                    else if (val is Size es)
                        picker.Size = es;
                    else if (val is GH_Vector vec)
                    {
                        Size size = new Size((int)vec.Value.X, (int)vec.Value.Y);
                        picker.Size = size;
                    }
                    else if (val is GH_String sstr)
                    {
                        string[] xy = sstr.Value.Split(',');
                        bool xp = int.TryParse(xy[0], out int x);
                        bool yp = int.TryParse(xy[1], out int y);
                        if (xp && yp)
                            picker.Size = new Size(x, y);
                    }
                    else if (val is GH_Rectangle grec)
                    {
                        int x = (int)grec.Value.X.Length;
                        int y = (int)grec.Value.Y.Length;
                        picker.Size = new Size(x, y);
                    }
                    else if (val is GH_ComplexNumber gcomp)
                    {
                        int x = (int)gcomp.Value.Real;
                        int y = (int)gcomp.Value.Imaginary;
                        picker.Size = new Size(x, y);
                    }
                    else
                        try { Util.SetProp(picker, "Size", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else
                    try { Util.SetProp(picker, n, Util.GetGooVal(val)); }
                    catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
            }

            DA.SetData(1, new GH_ObjectWrapper(picker));

            PropertyInfo[] allprops = picker.GetType().GetProperties();
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
                return Properties.Resources.pickcolor;
            }
        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.secondary| GH_Exposure.obscure; }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("ed5bab22-6431-4733-95ce-3bd840b9b563"); }
        }
    }
}