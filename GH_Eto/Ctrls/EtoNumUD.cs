using System;
using System.Collections.Generic;
using System.Reflection;
using Eto.Forms;
using Eto.Drawing;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using System.Windows.Forms;

namespace Synapse
{
    public class EtoNumUD : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the EtoNumUD class.
        /// </summary>
        public EtoNumUD()
          : base("SnpNumUpDn", "STicker",
              "numeric up-down control",
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

            NumericStepper numsteps = new NumericStepper() { ID = Guid.NewGuid().ToString(), };

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
                    if (val is GH_String gstr)
                    {
                        if (double.TryParse(gstr.Value, out double sli))
                            numsteps.MinValue = sli;
                    }
                    else if (val is GH_Integer gint)
                        numsteps.MinValue = gint.Value;
                    else if (val is GH_Number gnum)
                        numsteps.MinValue = gnum.Value;
                    else if (val is GH_Boolean gbool)
                        if (gbool.Value)
                            numsteps.MinValue = 1;
                        else
                            numsteps.MinValue = 0;
                    else
                        try { Util.SetProp(numsteps, "MinValue", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else if (n.ToLower() == "maxvalue" || n.ToLower() == "maximum")
                {
                    if (val is GH_String gstr)
                    {
                        if (double.TryParse(gstr.Value, out double sli))
                            numsteps.MaxValue = sli;
                    }
                    else if (val is GH_Integer gint)
                        numsteps.MaxValue = gint.Value;
                    else if (val is GH_Number gnum)
                        numsteps.MaxValue = gnum.Value;
                    else if (val is GH_Boolean gbool)
                        if (gbool.Value)
                            numsteps.MaxValue = 1;
                        else
                            numsteps.MaxValue = 0;
                    else
                        try { Util.SetProp(numsteps, "MaxValue", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else if (n.ToLower() == "increment" || n.ToLower() == "step")
                {
                    if (val is GH_Number gnum)
                        numsteps.Increment = gnum.Value;
                    else if (val is GH_Integer gint)
                        numsteps.Increment = gint.Value;
                    else if (val is GH_String gstr)
                    {
                        if (double.TryParse(gstr.Value, out double num))
                            numsteps.Increment = num;
                        else if (int.TryParse(gstr.Value, out int ii))
                            numsteps.Increment = ii;
                    }
                    else
                        try { Util.SetProp(numsteps, "Increment", Util.GetGooVal(val)); }
                        catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
                }
                else
                    try { Util.SetProp(numsteps, n, Util.GetGooVal(val)); }
                    catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, ex.Message); }
            }

            DA.SetData(1, new GH_ObjectWrapper(numsteps));

            PropertyInfo[] allprops = numsteps.GetType().GetProperties();
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
                return Properties.Resources.numupdn;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("d439464e-be51-47a9-a6fb-bef7a83c716c"); }
        }
    }
}