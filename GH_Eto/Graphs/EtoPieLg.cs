using System;
using System.Collections.Generic;
using Eto.Forms;
using Eto.Drawing;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

namespace Synapse
{
    public class EtoPieLg : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the EtoPieLg class.
        /// </summary>
        public EtoPieLg()
          : base("SynapsePieLegend", "Legend",
              "pie chart legend",
              "Synapse", "Graphs")
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
            pManager.AddGenericParameter("PieData", "D", "pie data object from pie chart graph", GH_ParamAccess.item);
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
            string bullet = @"●";
            string box = @"■";
            string pointer = @"◄";
        }

#if !DEBUG
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.hidden; }
        }
#endif

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
            get { return new Guid("01f2a829-9fc5-407e-9cba-4a0760a75851"); }
        }
    }
}