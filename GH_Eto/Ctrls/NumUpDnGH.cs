using System;
using System.Collections.Generic;
using Eto.Drawing;
using Eto.Forms;
using Grasshopper.Kernel;
using System.Reflection;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Types;

namespace Synapse
{
    public class NumUpDnGH : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the NumUpDnGH class.
        /// </summary>
        public NumUpDnGH()
          : base("NumUpDnGH", "UDGH",
              "numeric up down control from a Grasshopper slider",
              "Synapse", "Controls")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Property", "P", "property to set", GH_ParamAccess.tree);
            pManager[0].Optional = true;
            pManager.AddGenericParameter("Property Value", "V", "values for the property", GH_ParamAccess.tree);
            pManager[1].Optional = true;
            pManager.AddGenericParameter("Slider", "S", "link this to a Grasshopper slider", GH_ParamAccess.tree);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("All Properties", "A", "list of all accessible properties", GH_ParamAccess.list);
            pManager.AddGenericParameter("Control", "C", "control to go into a container or the listener", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DA.GetDataTree(0, out GH_Structure<GH_String> allprops);
            DA.GetDataTree(1, out GH_Structure<IGH_Goo> allvals);
            if (allprops.Branches.Count != allvals.Branches.Count)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "P, V should correspond each other");
                return;
            }

            List<NumericStepper> tickers = new List<NumericStepper>();
            foreach (IGH_Param prm in Params.Input[2].Sources)
                if (prm is GH_NumberSlider ghsl)
                {
                    NumericStepper ticker = new NumericStepper()
                    {
                        ID = Guid.NewGuid().ToString(),
                        Value = (double)ghsl.Slider.Value,
                        MaxValue = (double)ghsl.Slider.Maximum,
                        MinValue = (double)ghsl.Slider.Minimum,
                        DecimalPlaces = ghsl.Slider.DecimalPlaces,
                        Increment = ghsl.Slider.Type== Grasshopper.GUI.Base.GH_SliderAccuracy.Integer?1:Math.Pow(10, -ghsl.Slider.DecimalPlaces),
                    };
                    tickers.Add(ticker);
                }

            if (allprops.Branches.Count > 1 && allprops.Branches.Count != tickers.Count)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "P, V, C should be matching tree structures");
                return;
            }

            for (int ti = 0; ti < tickers.Count; ti++)
            {
                if (allprops.Branches.Count == 0 || allvals.Branches.Count == 0)
                    break;
                List<GH_String> props;
                List<IGH_Goo> vals;
                NumericStepper numsteps = tickers[ti];
                try
                {
                    props = allprops.Branches[ti];
                    vals = allvals.Branches[ti];
                }
                catch (ArgumentOutOfRangeException)
                {
                    props = allprops.Branches[0];
                    vals = allvals.Branches[0];
                }

                for (int i = 0; i < props.Count; i++)
                {
                    string n = props[i].Value;
                    object val;
                    try { val = vals[i]; }
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
            }

            GH_ObjectWrapper[] tickerobjs = new GH_ObjectWrapper[tickers.Count];
            for (int i = 0; i < tickerobjs.Length; i++)
                tickerobjs.SetValue(new GH_ObjectWrapper(tickers[i]), i);
            DA.SetDataList(1, tickerobjs);

            PropertyInfo[] objprops = tickers[0].GetType().GetProperties();
            List<string> printouts = new List<string>();
            foreach (PropertyInfo prop in objprops)
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
                return Properties.Resources.tickergh;
            }
        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.primary | GH_Exposure.obscure; }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("25b771d9-719a-4162-82f7-1aaf10f398cd"); }
        }
    }
}