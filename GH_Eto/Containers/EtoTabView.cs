using System;
using System.Collections.Generic;
using System.Reflection;
using Eto.Forms;
using Eto.Drawing;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

namespace Synapse
{
    public class EtoTabView : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the EtoTabView class.
        /// </summary>
        public EtoTabView()
          : base("SynapseTabs", "Tab",
              "tabbed view",
              "Synapse", "Containers")
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
            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "not implemented");
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.tabbed;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("b9e39632-45c5-4452-a3ae-dd2d2ccba1f7"); }
        }
    }
}