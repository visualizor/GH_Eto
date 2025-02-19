using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Eto.Forms;
using Grasshopper.Kernel.Parameters;

namespace Synapse
{
    public class EtoPopUp : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the EtoPopUp class.
        /// </summary>
        public EtoPopUp()
          : base("SnpPopUp", "SPop",
              "Pop up window with a message",
              "Synapse", "Containers")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Show", "T", "trigger to show pop up window", GH_ParamAccess.item, false);
            pManager.AddTextParameter("Message", "M", "message to display", GH_ParamAccess.item, "no message");
            pManager.AddIntegerParameter("Urgency", "U", "type of message shown", GH_ParamAccess.item, 0);
            Param_Integer prm2 = pManager[2] as Param_Integer;
            prm2.AddNamedValue("Info", 0);
            prm2.AddNamedValue("Warning", 1);
            prm2.AddNamedValue("Error", 2);
            prm2.AddNamedValue("Question", 3);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool t = false;
            string msg = "";
            int urg = 0;
            DA.GetData(0, ref t);
            DA.GetData(1, ref msg);
            DA.GetData(2, ref urg);

            if (t)
                MessageBox.Show(msg, (MessageBoxType)urg);
        }


        public override GH_Exposure Exposure => GH_Exposure.hidden;

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
            get { return new Guid("8632887D-24DF-4E81-BA3B-70839B233AC5"); }
        }
    }
}