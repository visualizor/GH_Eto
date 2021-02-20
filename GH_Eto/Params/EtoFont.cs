using System;
using Eto.Drawing;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;

namespace Synapse
{
    public class EtoFont : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the EtoFont class.
        /// </summary>
        public EtoFont()
          : base("SynapseFont", "SFont",
              "font object for Synapse controls",
              "Synapse", "Parameters")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("FontFamily", "F", "font family name", GH_ParamAccess.item, "Arial");
            pManager.AddNumberParameter("FontSize", "S", "font size", GH_ParamAccess.item, 8.25);
            pManager.AddIntegerParameter("FontStyle", "L", "font style\n0 - none\n1 - bold\n2 - italic", GH_ParamAccess.item, 0);
            pManager.AddIntegerParameter("Decoration", "D", "font decoration\n0 - none\n1 - underline\n2 - strike through", GH_ParamAccess.item, 0);
            Param_Integer pint = pManager[2] as Param_Integer;
            pint.AddNamedValue("None", 0);
            pint.AddNamedValue("Bold", 1);
            pint.AddNamedValue("Italic", 2);
            Param_Integer dint = pManager[3] as Param_Integer;
            dint.AddNamedValue("None", 0);
            dint.AddNamedValue("Underline", 1);
            dint.AddNamedValue("StrikeThrough", 2);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Font", "T", "font object for Synapse controls", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string fam = "";
            double size = double.NaN;
            int style = 0;
            int decor = 0;
            DA.GetData(0, ref fam);
            DA.GetData(1, ref size);
            DA.GetData(2, ref style);
            DA.GetData(3, ref decor);

            Font t = new Font(fam, (float)size, (FontStyle)style, (FontDecoration)decor);

            DA.SetData(0, new GH_ObjectWrapper(t));
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.fonts;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("215e095f-e70a-47c2-88f2-eb1f8968e23a"); }
        }
    }
}