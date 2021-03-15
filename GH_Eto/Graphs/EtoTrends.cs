using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Forms;
using Eto.Drawing;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

namespace Synapse
{
    public class EtoTrends : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the EtoTrends class.
        /// </summary>
        public EtoTrends()
          : base("SynapseTrends", "STrends",
              "line graph",
              "Synapse", "Graphs")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Size", "S", "size", GH_ParamAccess.item, 50);
            pManager.AddNumberParameter("Numbers", "N", "numbers to represent in a line chart\neach branch will generate a polyline", GH_ParamAccess.tree);
            pManager.AddTextParameter("Name", "K", "trend name", GH_ParamAccess.tree);
            pManager[2].Optional = true;
            pManager.AddColourParameter("Color", "C", "regular Grasshopper color will do\nno need to use Synapse colors", GH_ParamAccess.tree);
            pManager[3].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Chart Object", "G", "chart", GH_ParamAccess.item);
            pManager.AddGenericParameter("Chart Data", "D", "chart data if you need to generate a legend", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
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
            get { return new Guid("e57895f7-6d5e-4b92-a8fd-e10c481408dd"); }
        }
    }
}