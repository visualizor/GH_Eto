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
            pManager.AddNumberParameter("low", "l", "lower", GH_ParamAccess.item);
            pManager.AddNumberParameter("up", "u", "upper", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Control", "C", "slider control", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double l = 0;
            double u = 0;
            DA.GetData(0, ref l);
            DA.GetData(1, ref u);
            DomainSlider dsl = new DomainSlider(80)
            {
                Lower = l,
                Upper = u
            };
            dsl.PaintCtrl();
            DA.SetData(0, new GH_ObjectWrapper(dsl));
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
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