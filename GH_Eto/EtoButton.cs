using System;
using System.Collections.Generic;
using Eto.Forms;
using Eto.Drawing;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Data;

namespace GH_Eto
{
    public class EtoButton : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the EtoButton class.
        /// </summary>
        public EtoButton()
          : base("SynapseButton", "SButton",
              "button",
              "Synapse", "Controls")
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
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Control", "C", "control to go into a container or the listener", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<string> pnames = new List<string>();
            List<GH_ObjectWrapper> vals = new List<GH_ObjectWrapper>();
            DA.GetDataList(0, pnames);
            DA.GetDataList(1, vals);

            Button btn = new Button() { Text = "a button", ID = Guid.NewGuid().ToString() };
            // TODO: set properties

            DA.SetData(0, new GH_ObjectWrapper(btn));
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
            get { return new Guid("f8b49b4f-5bbf-450f-b111-7c0d1ca7b000"); }
        }
    }
}