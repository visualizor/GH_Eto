using System;
using System.Collections.Generic;

using Eto.Drawing;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

namespace Synapse
{
    public class EtoColor : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the EtoColor class.
        /// </summary>
        public EtoColor()
          : base("SynapseColor", "SColor",
              "color object for synapse controls",
              "Synapse", "Parameters")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Alpha", "A", "alpha channel\nuse integer channels (0 - 255)", GH_ParamAccess.item, 255);
            pManager.AddIntegerParameter("Red", "R", "red channel\nuse integer channels (0 - 255)", GH_ParamAccess.item, 123);
            pManager.AddIntegerParameter("Green", "G", "green channel\nuse integer channels (0 - 255)", GH_ParamAccess.item, 231);
            pManager.AddIntegerParameter("Blue", "B", "blue channel\nuse integer channels (0 - 255)", GH_ParamAccess.item, 132);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Color", "C", "synapse color object", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int a = 255;
            int r = 123;
            int g = 231;
            int b = 132;
            DA.GetData(0, ref a);
            DA.GetData(1, ref r);
            DA.GetData(2, ref g);
            DA.GetData(3, ref b);

            DA.SetData(0, new GH_ObjectWrapper(Color.FromArgb(r, g, b, a)));
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
                return Properties.Resources.color;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("3450a1eb-9c23-4253-8a90-7d50df63ede3"); }
        }
    }
}