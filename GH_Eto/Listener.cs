using System;
using System.Collections.Generic;
using Eto.Forms;
using Eto.Drawing;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Data;

namespace GH_Eto
{
    public class Listener : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Listener class.
        /// </summary>
        public Listener()
          : base("Listener", "Value",
              "listen to a Synapse component",
              "Synapse", "Containers")
        {
        }

        private bool listening = false;
        private string listenee = "";

        private void Relisten(Control ctrl)
        {
            if (ctrl is Button btn)
            {
                listenee = btn.ID;
                btn.Click += OnBtnClick;
            }
            //TODO: insert more conditions
        }
        private GH_ObjectWrapper GetCtrlValue(Control ctrl)
        {
            //TODO: insert more conditions
             return new GH_ObjectWrapper(true);
        }

        public void OnBtnClick(object s, EventArgs e)
        {
            ExpireSolution(true);
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Synapse Object", "S", "what to listen to", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Value", "V", "the value heard", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_ObjectWrapper obj = new GH_ObjectWrapper();
            DA.GetData(0, ref obj);
            if (obj.Value is Control ctrl)
            {
                if (ctrl.ID != listenee)
                {
                    Relisten(ctrl);
                    listening = true;
                }
                else if (!listening)
                {
                    listening = true;
                }
                else
                {
                    DA.SetData(0, GetCtrlValue(ctrl));
                }
            }
            else
            {
                listening = false;
            }
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
            get { return new Guid("f283eb4d-4481-428f-88ab-ceeffa900d29"); }
        }
    }
}