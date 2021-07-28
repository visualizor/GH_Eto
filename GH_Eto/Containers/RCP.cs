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
          : base("RemotePanel", "RCP",
              "Rhino remote control panel for Synapse elements",
              "Synapse", "Containers")
        {
        }

        protected IEnumerable<IGH_Param> srcs;
        protected bool rectrl = true;

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Send", "S", "set to true to send window to RCP", GH_ParamAccess.item, false);
            pManager[0].Optional = true;
            pManager.AddGenericParameter("Window", "W", "the window to be wrapped in the RCP", GH_ParamAccess.item);
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
            if (rectrl)
            {
                OnPingDocument().ScheduleSolution(1, delegate {
                    foreach (IGH_Param prm in Params.Input[1].Sources)
                        prm.Attributes.GetTopLevel.DocObject.ExpireSolution(false);
                    rectrl = false;
                });
                return;
            }
            else
                rectrl = true;


            bool send = false;
            GH_ObjectWrapper obj = new GH_ObjectWrapper();
            DA.GetData(0, ref send);
            DA.GetData(1, ref obj);
            SynapseRH rcp;

            Guid id = new Guid("a1dfe99b-c120-490f-bef5-572e68f4900d");
            if (!PlugIn.GetPlugInInfo(id).IsLoaded)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " RCP isn't loaded\n go to Rhino plugin manager");
                return;
            }
            else
            {
                rcp = PlugIn.Find(id) as SynapseRH;
                if (rcp.RemotePanel == null)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " RCP isn't shown\n try \"SynapsePanel\" command in Rhino");
                    return;
                }
            }
            
            if (obj.Value is Form win)
                if (send)
                {
                    if (win.Visible)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " a shown window's content cannot be rendered elsewhere");
                        return;
                    }
                    rcp.RemotePanel.Content = win.Content;
                    win.Tag = EWinTag.InPanel;
                }
                else
                {
                    rcp.RemotePanel.Content = null;
                    win.Tag = EWinTag.Indie;
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