using System;
using System.Collections.Generic;
using Eto.Drawing;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

namespace Synapse
{
    public class EtoSize : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the EtoSize class.
        /// </summary>
        public EtoSize()
          : base("SynapseSize", "Size",
              "size object (width and height)",
              "Synapse", "Parameters")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Width", "W", "width", GH_ParamAccess.item, 100);
            pManager.AddIntegerParameter("Height", "H", "height", GH_ParamAccess.item, 60);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Size", "S", "size object for Synapse components", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int x = 0;
            int y = 0;
            DA.GetData(0, ref x);
            DA.GetData(1, ref y);
            DA.SetData(0, new GH_ObjectWrapper(new Size(x, y)));
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.size;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("7e73e9ce-b828-4b65-9ac1-85f050878691"); }
        }
    }
}