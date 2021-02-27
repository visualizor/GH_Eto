using System;
using System.Collections.Generic;
using System.Reflection;
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

                }
                else if (n.ToLower() == "tickfrequency" || n.ToLower() == "step")
                {

                }
            }

            DA.SetData(1, new GH_ObjectWrapper(csl));
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