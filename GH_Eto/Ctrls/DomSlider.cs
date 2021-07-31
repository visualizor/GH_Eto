using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Eto.Drawing;

namespace Synapse.Ctrls
{
    public class DomSlider : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the DomSlider class.
        /// </summary>
        public DomSlider()
          : base("DomSlider", "DomSl",
              "domain slider",
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
            pManager.AddGenericParameter("Control", "C", "slider control", GH_ParamAccess.item);
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

            ComboDomSl dsl = new ComboDomSl(140)
            {
                ID = Guid.NewGuid().ToString(),
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

                if (n.ToLower() == "maxvalue" || n.ToLower() == "max")
                {

                }
                else if (n.ToLower() == "minvalue" || n.ToLower() == "min")
                {

                }
                else if (n.ToLower() == "rightstop" || n.ToLower() == "rightend")
                {

                }
                else if (n.ToLower() == "leftstop" || n.ToLower() == "leftend")
                {

                }
                else if (n.ToLower() == "width")
                {

                }
                else if (n.ToLower() == "color")
                {
                    if (val is Color clr)
                        dsl.DomSl.SliderColor = clr;
                    else if (val is GH_String gstr)
                    {
                        if (Color.TryParse(gstr.Value, out Color c))
                            dsl.DomSl.SliderColor = c;
                    }
                    else
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " can't parse color");
                }
                else if (n.ToLower() == "height")
                {
                    if (val is GH_Integer gint)
                        dsl.DomSl.KnobDiameter = gint.Value;
                    else if (val is GH_Number gn)
                        dsl.DomSl.KnobDiameter = (int)Math.Round(gn.Value, 0);
                    else if (val is GH_Interval gitvl)
                        dsl.DomSl.KnobDiameter = (int)Math.Round(gitvl.Value.Length, 0);
                    else if (val is GH_String gstr)
                    {
                        if (double.TryParse(gstr.Value, out double num))
                            dsl.DomSl.KnobDiameter = (int)Math.Round(num, 0);
                    }
                    else
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " can't parse height");
                }
                else
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " not a valid property to set");
            }

            DA.SetData(1, new GH_ObjectWrapper(dsl));
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.domsl;
            }
        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.hidden; }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("27a02089-fe7f-4755-b748-3df08531929d"); }
        }
    }
}