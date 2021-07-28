using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Eto.Forms;
using Rhino.PlugIns;
using Grasshopper.Kernel.Types;
using SynapseRCP;

namespace Synapse
{
    public class RCP : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the RCP class.
        /// </summary>
        public RCP()
          : base("RCP", "RCP",
              "Rhino control panel for Synapse elements",
              "Synapse", "Containers")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Send", "S", "set to true to send window to RCP", GH_ParamAccess.item);
            pManager.AddGenericParameter("Window", "W", "the window to be wrapped in the RCP", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Out", "O", "output debug", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool send = false;
            GH_ObjectWrapper obj = new GH_ObjectWrapper();
            DA.GetData(0, ref send);
            DA.GetData(1, ref obj);

            Guid id = new Guid("a1dfe99b-c120-490f-bef5-572e68f4900d");
            
            if (!PlugIn.GetPlugInInfo(id).IsLoaded)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " RCP isn't loaded\n try running \"SynapsePanel\" command in Rhino");
                return;
            }
            
            if (obj.Value is Form win)
            {
                SynapseRH rcp = PlugIn.Find(id) as SynapseRH;
                rcp.RemotePanel.Content = win.Content;
                DA.SetData(0, rcp.RemotePanel.ToString());
            }

        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.rcp;
            }
        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.secondary; }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("cfca88f2-45cb-4b14-a423-d72a98620c2e"); }
        }
    }
}