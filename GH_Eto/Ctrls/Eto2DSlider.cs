using System;
using System.Collections.Generic;
using Eto.Forms;
using Eto.Drawing;
using sd = System.Drawing;
using Grasshopper.Kernel;
using Grasshopper;
using Grasshopper.Kernel.Types;

namespace Synapse.Ctrls
{
    public class Eto2DSlider : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Eto2DSlider class.
        /// </summary>
        public Eto2DSlider()
          : base("SynapseSampler", "Sampler",
              "similar to a UV slider",
              "Synapse", "Controls")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Size", "D", "dimension for the slider box", GH_ParamAccess.item, 200);
            pManager.AddColourParameter("Color", "C", "colors, use regular Grasshopper color", GH_ParamAccess.item, sd.Color.FromArgb(255,45,45,45));
            pManager.AddIntegerParameter("LineStroke", "W", "lineweight/stroke to render the grid and cursor point", GH_ParamAccess.item, 3);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("All Properties", "A", "list of all accessible properties", GH_ParamAccess.list);
            pManager.AddGenericParameter("Control", "C", "control to go into a container or the listener", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int dim = 200;
            sd.Color clr = sd.Color.Empty;
            int stroke = 3;
            DA.GetData(0, ref dim);
            DA.GetData(1, ref clr);
            DA.GetData(2, ref stroke);

            Bitmap bmp = new Bitmap(new Size(dim, dim), PixelFormat.Format32bppRgba);
            Pen pen = new Pen(clr.ToEto(), stroke);
            TrackPad tp = new TrackPad(bmp, pen) { ID = Guid.NewGuid().ToString(),};
            tp.DrawGrid(true);

            DA.SetData(1, new GH_ObjectWrapper(tp));

        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.slider2d;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("a6c22b4b-8c79-4b8a-a3a7-269ade6321b1"); }
        }
    }
}