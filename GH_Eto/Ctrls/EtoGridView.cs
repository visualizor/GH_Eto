using System;
using System.Collections.Generic;
using System.Reflection;
using Eto.Forms;
using Eto.Drawing;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

namespace Synapse
{
    public class EtoGridView : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the EtoGridView class.
        /// </summary>
        public EtoGridView()
          : base("SynapseGridView", "GridView",
              "column and rows of items",
              "Synapse", "Controls")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
        }


        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.hidden | GH_Exposure.secondary; }
            // TODO: unhide once done
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.gridview;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("cb41ea70-ba6f-40e6-97e1-60f285036f6c"); }
        }
    }
}