using System;
using Eto.Forms;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Parameters;

namespace Synapse
{
    public class EtoFormStyle : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the EtoFormStyle class.
        /// </summary>
        public EtoFormStyle()
          : base("SynapseWinStyle", "WS",
              "windows style",
              "Synapse", "Parameters")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("StyleIndex", "I", "windows style\n0 - Default\n1 - Blank Slate (None)\n2 - Minimal Utility", GH_ParamAccess.item, 0);
            Param_Integer pi = pManager[0] as Param_Integer;
            pi.AddNamedValue("Default", 0);
            pi.AddNamedValue("Blank Slate", 1);
            pi.AddNamedValue("Minimal", 2);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Style", "S", "window style", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int i = 0;
            DA.GetData(0, ref i);
            switch (i)
            {
                case 0:
                    DA.SetData(0, new GH_ObjectWrapper(WindowStyle.Default));
                    break;
                case 1:
                    DA.SetData(0, new GH_ObjectWrapper(WindowStyle.None));
                    break;
                case 2:
                    DA.SetData(0, new GH_ObjectWrapper(WindowStyle.Utility));
                    break;
                default:
                    DA.SetData(0, new GH_ObjectWrapper(WindowStyle.Default));
                    break;
            }
        }


        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.obscure | GH_Exposure.primary; }
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.winstyle;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("82fe8d0a-045c-4564-8c2b-827b61d6e610"); }
        }
    }
}