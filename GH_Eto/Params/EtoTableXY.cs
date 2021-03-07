using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Drawing;
using Eto.Forms;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

namespace Synapse
{
    public class EtoTableXY : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the EtoTableXY class.
        /// </summary>
        public EtoTableXY()
          : base("SynapseTableXY", "STxy",
              "arrange controls into a TableLayout and produces coordinates text strings\ncontrols are placed from top to bottom, left to right",
              "Synapse", "Parameters")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Controls", "C", "list of Snyapse controls to arrange", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Columns", "N", "number of columns in the SynapseTable\nuse a positive integer", GH_ParamAccess.item,2);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Coordinates", "L", "location coordinates for use on a SynapseTable", GH_ParamAccess.list);
            pManager.AddGenericParameter("Dimensions", "D", "Dimension object for use on a SynapseTable", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<GH_ObjectWrapper> ctrls = new List<GH_ObjectWrapper>();
            int cn = 2;
            DA.GetDataList(0, ctrls);
            DA.GetData(1, ref cn);
            int rn = (int)Math.Ceiling(ctrls.Count / (double)cn);
            List<string> coors = new List<string>();
            for (int ci = 0; ci < ctrls.Count; ci++)
                for (int y = 0; y < rn && coors.Count<ctrls.Count; y++)
                {
                    int x = ci % rn;
                    coors.Add(string.Format("{0},{1}", x, y));
                }
            DA.SetDataList(0, coors);
            DA.SetData(1, new GH_ObjectWrapper(new Size(cn, rn)));
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.tablexy;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("c7ad6dcc-7752-4e85-bcbe-3fc89287106a"); }
        }
    }
}