using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Eto.Drawing;

namespace Synapse
{
    public class EtoIcon : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the EtoIcon class.
        /// </summary>
        public EtoIcon()
          : base("SnpIcon", "SIcon",
              "image icon that can be used by Synapse controls",
              "Synapse", "Parameters")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("File Location", "F", "file location for the image", GH_ParamAccess.item);
            pManager.AddGenericParameter("Size", "S", "optional size to fit image", GH_ParamAccess.item);
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Image Icon", "I", "image icon", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string pth = "";
            GH_ObjectWrapper sizeobj = new GH_ObjectWrapper();
            DA.GetData(0, ref pth);
            bool issized = DA.GetData(1, ref sizeobj);

            Bitmap bitmap = new Bitmap(pth);

            if (!issized)
                DA.SetData(0, new GH_ObjectWrapper(bitmap));
            else
            {
                if (sizeobj.Value is Size es)
                    DA.SetData(0, new GH_ObjectWrapper(bitmap.WithSize(es)));
                else if (sizeobj.Value is GH_ComplexNumber ri)
                    DA.SetData(0, new GH_ObjectWrapper(bitmap.WithSize(new Size((int)ri.Value.Real, (int)ri.Value.Imaginary))));
                else if (sizeobj.Value is GH_Vector gv)
                    DA.SetData(0, new GH_ObjectWrapper(bitmap.WithSize(new Size((int)gv.Value.X, (int)gv.Value.Y))));
                else if (sizeobj.Value is GH_Point gp)
                    DA.SetData(0, new GH_ObjectWrapper(bitmap.WithSize(new Size((int)gp.Value.X, (int)gp.Value.Y))));
                else if (sizeobj.Value is GH_Rectangle grec)
                    DA.SetData(0, new GH_ObjectWrapper(bitmap.WithSize(new Size((int)grec.Value.X.Length, (int)grec.Value.Y.Length))));
                else
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " check size input data type");
            }
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.etoimg;
            }
        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.obscure | GH_Exposure.primary; }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("cfe54b4a-0bc4-4f56-872f-9cbacf6fd908"); }
        }
    }
}