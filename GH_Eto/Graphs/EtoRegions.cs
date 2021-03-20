using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Drawing;
using Eto.Forms;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

namespace Synapse
{
    public class EtoRegions : EtoPie
    {
        /// <summary>
        /// Initializes a new instance of the EtoRegions class.
        /// </summary>
        public EtoRegions()
          : base()
        {
            Name = "SynapseRegions";
            NickName = "Regions";
            Description = "region chart showing proportions";
            Category = "Synapse";
            SubCategory = "Graphs";
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("98cd3423-8f1e-454c-b12e-f03f45f491e1"); }
        }
    }
}